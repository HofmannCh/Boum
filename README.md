# Boum
A console app for listing folders and files called Boum (which means DE Baum and EN Tree)

## Config
```
boum.exe [PATH]

  -r REGEX, --file-regex=REGEX          Regex for file name, case insensitive.

  -a REGEX, --any-regex=REGEX           Regex for directory or file name, case insensitive. With this option it automatically shows empty
                                        folders.

  -e BOOL, --show-empty-folders=BOOL    (Default: false) Per default empty folders are hidden. This forces to show them.

  -l BOOL, --legacy=BOOL                (Default: false) Use "+-| " chars instead of fancy unicode.

  -d BOOL, --depth=BOOL                 (Default: 20) The max folder recursion depth.

  -t BOOL, --table-view=BOOL            (Default: false) Show matching files as table.

  --help                                Display this help screen.

  --version                             Display version information.

  value pos. 0 DIRECTORY                The folder that should be analyzed.
```

## Examples
My folder structure:
```
Boum
 ├──Folder 1
 │   └──New Microsoft Excel Worksheet.xlsx
 ├──Folder 2
 │   ├──Subfolder 2.1
 │   ├──Subfolder 2.2
 │   │   └──New Microsoft Word Document.docx
 │   ├──Subfolder 2.3
 │   │   └──New Microsoft PowerPoint Presentation.pptx
 │   └──New Text Document.txt
 └──Folder 3
     ├──Subfolder 3.1
     └──New Microsoft Word Document.docx
```
---

Run without args
`boum.exe .`
```
Boum (3x 0x)
 ├──Folder 1 (0x 1x)
 │   └──New Microsoft Excel Worksheet.xlsx
 ├──Folder 2 (2x 1x)
 │   ├──Subfolder 2.2 (0x 1x)
 │   │   └──New Microsoft Word Document.docx
 │   ├──Subfolder 2.3 (0x 1x)
 │   │   └──New Microsoft PowerPoint Presentation.pptx
 │   └──New Text Document.txt
 └──Folder 3 (0x 1x)
     └──New Microsoft Word Document.docx
```

Run with
`boum.exe . -e`
```
Boum (3x 0x)
 ├──Folder 1 (0x 1x)
 │   └──New Microsoft Excel Worksheet.xlsx
 ├──Folder 2 (3x 1x)
 │   ├──Subfolder 2.1 (0x 0x)
 │   ├──Subfolder 2.2 (0x 1x)
 │   │   └──New Microsoft Word Document.docx
 │   ├──Subfolder 2.3 (0x 1x)
 │   │   └──New Microsoft PowerPoint Presentation.pptx
 │   └──New Text Document.txt
 └──Folder 3 (1x 1x)
     ├──Subfolder 3.1 (0x 0x)
     └──New Microsoft Word Document.docx
```

Run with
`boum.exe . -l`
```
Boum (3x 0x)
 +- Folder 1 (0x 1x)
 |   +- New Microsoft Excel Worksheet.xlsx
 +- Folder 2 (2x 1x)
 |   +- Subfolder 2.2 (0x 1x)
 |   |   +- New Microsoft Word Document.docx
 |   +- Subfolder 2.3 (0x 1x)
 |   |   +- New Microsoft PowerPoint Presentation.pptx
 |   +- New Text Document.txt
 +- Folder 3 (0x 1x)
     +- New Microsoft Word Document.docx
```

Run with
`boum.exe . -t`
```
New Microsoft Excel Worksheet.xlsx                                                                   Folder 1
New Microsoft Word Document.docx                                                                     Folder 2\Subfolder 2.2
New Microsoft PowerPoint Presentation.pptx                                                           Folder 2\Subfolder 2.3
New Text Document.txt                                                                                Folder 2
New Microsoft Word Document.docx                                                                     Folder 3
```

Run with file REGEX
`boum.exe . -r .*\.docx`
```
Boum (2x 0x)
 ├──Folder 2 (1x 0x)
 │   └──Subfolder 2.2 (0x 1x)
 │       └──New Microsoft Word Document.docx
 └──Folder 3 (0x 1x)
     └──New Microsoft Word Document.docx
```

Run with file REGEX and table
`boum.exe . -r .*\.docx -t`
```
New Microsoft Word Document.docx                                                                     Folder 2\Subfolder 2.2
New Microsoft Word Document.docx                                                                     Folder 3
```

Run with file REGEX
`boum.exe . -r Subfolder`
```
```

Run with folder and file REGEX
`boum.exe . -a Subfolder`
```
Boum (2x 0x)
 ├──Folder 2 (3x 0x)
 │   ├──Subfolder 2.1 (0x 0x)
 │   ├──Subfolder 2.2 (0x 0x)
 │   └──Subfolder 2.3 (0x 0x)
 └──Folder 3 (1x 0x)
     └──Subfolder 3.1 (0x 0x)
```
