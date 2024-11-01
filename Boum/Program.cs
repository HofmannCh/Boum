using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CommandLine;

namespace Boum
{
    internal class Program
    {
        private readonly Options _options;
        private readonly Regex? _fileRegex;
        private readonly Regex? _anyRegex;
        private bool _isTerminated = false;

        public class Options
        {
            [Value(0, Default = null, MetaValue = "DIRECTORY", HelpText = "The folder that should be analyzed.")] public string? Path { get; set; }

            [Option('r', "file-regex", MetaValue = "REGEX", Default = null, HelpText = "Regex for file name, case insensitive.")] public string? FileRegex { get; set; }

            [Option('a', "any-regex", MetaValue = "REGEX", Default = null, HelpText = "Regex for directory or file name, case insensitive. With this option it automatically shows empty folders.")] public string? AnyRegex { get; set; }

            [Option('e', "show-empty-folders", MetaValue = "BOOL", Default = false, HelpText = "Per default empty folders are hidden. This forces to show them.")] public bool ShowEmptyFolders { get; set; }

            [Option('l', "legacy", MetaValue = "BOOL", Default = false, HelpText = "Use \"+-| \" chars instead of fancy unicode.")] public bool Legacy { get; set; }

            [Option('d', "depth", MetaValue = "BOOL", Default = 20, HelpText = "The max folder recursion depth.")] public int Depth { get; set; }

            [Option('t', "table-view", MetaValue = "BOOL", Default = false, HelpText = "Show matching files as table.")] public bool TableView { get; set; }
        }

        static void Main(string[] args)
        {
            //args = args.Concat(["-h"]).ToArray();
            var options = Parser.Default.ParseArguments<Options>(args).Value;

            if (options == null) return; // When -h for help

            new Program(options).Execute();

#if DEBUG
            Console.ReadKey();
#endif
        }

        public Program(Options options)
        {
            _options = options;
            _fileRegex = string.IsNullOrWhiteSpace(options.FileRegex) ? null : new Regex(options.FileRegex, RegexOptions.IgnoreCase);
            _anyRegex = string.IsNullOrWhiteSpace(options.AnyRegex) ? null : new Regex(options.AnyRegex, RegexOptions.IgnoreCase);
        }

        public class FormatOptions
        {
            public string Key { get; set; }
            public string KeyLast { get; set; }
            public string Indent { get; set; }
            public string IndentLast { get; set; }
        }

        public class File
        {
            public required string Name { get; set; }

            public string GetFilePrintString(string prefix) => prefix + "" + Name;

            public string GetDisplay() => Name;
        }

        public class Folder
        {
            public required string Name { get; set; }
            public required string FullPath { get; set; }
            public required Folder[] Folders { get; set; }
            public required File[] Files { get; set; }

            private bool? _isEmpty = null;
            public bool IsEmpty() => _isEmpty ??= Folders.All(x => x.IsEmpty()) && !Files.Any();

            public IEnumerable<string> GetFolderPrintString(FormatOptions format, string relativePath, string keyPrefix, string noPrefix)
            {
                string prefixK, prefixI, prefixN;
                void SetPrefixes(bool isNotLast)
                {
                    if (isNotLast)
                        (prefixK, prefixI, prefixN) = (keyPrefix + format.Key, keyPrefix + format.Indent, noPrefix + format.Indent);
                    else
                        (prefixK, prefixI, prefixN) = (keyPrefix + format.KeyLast, keyPrefix + format.IndentLast, noPrefix + format.IndentLast);
                }

                for (var folderI = 0; folderI < Folders.Length; folderI++)
                {
                    var folder = Folders[folderI];

                    SetPrefixes(folderI < Folders.Length - 1 || Files.Length >= 1);

                    yield return prefixK + "" + folder.GetDisplay();

                    foreach (var res in folder.GetFolderPrintString(format, relativePath, prefixI, prefixN))
                    {
                        yield return res;
                    }
                }

                for (int fileI = 0; fileI < Files.Length; fileI++)
                {
                    var file = Files[fileI];

                    SetPrefixes(fileI < Files.Length - 1);

                    yield return prefixK + "" + file.GetDisplay();
                }
            }

            public string GetDisplay() => $"{Name} ({Folders.Length}x {Files.Length}x)";
        }

        private void Execute()
        {
            Console.CancelKeyPress += (_, _) =>
            {
                lock (this) _isTerminated = true;
                Console.WriteLine("App Terminated");
            };

            var fullPath = Path.GetFullPath(_options.Path ?? ".");
            Console.WriteLine(fullPath);

            if (!Directory.Exists(fullPath))
                throw new Exception($"{fullPath} isn't a directory");

            Folder folder;
            try
            {
                folder = ParseFolder(fullPath, 0);
            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.Error.WriteLine(e.Message);
                return;
            }

            if (_anyRegex != null)
                RemoveAny(folder);

            if (!_options.ShowEmptyFolders && _anyRegex == null)
                RemoveEmpty(folder);

            var format = _options.Legacy
                ? new FormatOptions
                {
                    Key = " +- ",
                    KeyLast = " +- ",
                    Indent = " |  ",
                    IndentLast = "    ",
                }
                : new FormatOptions
                {
                    Key = " ├──",
                    KeyLast = " └──",
                    Indent = " │  ",
                    IndentLast = "    ",
                };

            //Console.ReadKey();
            if (_options.TableView)
            {
                //Console.WriteLine(folder);
                foreach (var line in PrintTableFolder(folder, fullPath))
                {
                    lock (this)
                        if (_isTerminated) return;
                    Console.WriteLine(line);
                }
            }
            else
            {
                Console.WriteLine(folder.GetDisplay());
                foreach (var line in folder.GetFolderPrintString(format, fullPath, string.Empty, string.Empty))
                {
                    lock (this)
                        if (_isTerminated) return;
                    Console.WriteLine(line);
                }
            }
        }

        private IEnumerable<string> PrintTableFolder(Folder folder, string rootFolder)
        {
            foreach (var folderFolder in folder.Folders)
            {
                foreach (var line in PrintTableFolder(folderFolder, rootFolder))
                    yield return line;
            }

            var relativePath = Path.GetRelativePath(rootFolder, folder.FullPath);
            foreach (var folderFile in folder.Files)
            {
                var name = folderFile.Name;
                if (name.Length > 100)
                    name = name.Substring(0, 100);
                yield return $"{name,-100} {relativePath}";
            }
        }

        private Folder ParseFolder(string fullPath, int level)
        {
            if (level >= _options.Depth)
                return new Folder
                {
                    Name = Path.GetFileName(fullPath),
                    FullPath = fullPath,
                    Folders = [],
                    Files = []
                };

            return new Folder
            {
                Name = Path.GetFileName(fullPath),
                FullPath = fullPath,
                Folders = Directory.GetDirectories(fullPath)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Select(x => ParseFolder(x, level + 1))
                    .ToArray(),
                Files = Directory.GetFiles(fullPath)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new File() { Name = Path.GetFileName(x) })
                    .Where(x => DoesMatchRegex(x.Name))
                    .ToArray()
            };
        }

        private bool DoesMatchRegex(string data)
        {
            return _fileRegex == null || _fileRegex.IsMatch(data);
        }

        private bool RemoveAny(Folder folder) // assume _anyRegex isn't null
        {
            var folders = folder.Folders.Where(x => RemoveAny(x)).ToArray();
            var files = folder.Files.Where(x => _anyRegex.IsMatch(x.Name)).ToArray();

            if (folders.Any() || files.Any() || _anyRegex.IsMatch(folder.Name))
            {
                folder.Folders = folders;
                folder.Files = files;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RemoveEmpty(Folder folder)
        {
            folder.Folders = folder.Folders.Where(x => !x.IsEmpty()).ToArray();

            foreach (var f in folder.Folders)
                RemoveEmpty(f);

            return;
        }
    }
}
