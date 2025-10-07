# Console Folder Analyzer

A convenient console tool for recursive visualization of folder and file structures with detailed statistics and color-coded size indicators. Quickly browse directories and get detailed information interactively.

---

## Features

- Recursive tree display of folders and files
- Color coding by file/folder size:
  - 🟢 Green - up to 100 MB (small)
  - 🟡 Yellow - 100 MB to 1 GB (medium)
  - 🟠 Orange - 1 GB to 5 GB (large)
  - 🔴 Red - over 5 GB (very large)
- Size displayed next to each item (bytes, MB, GB)
- Auto-detection of folders and files based on extension existence
- Empty folders (0 bytes) highlighted (⚪️)
- Interactive navigation with commands
- Optional display of creation and last modification dates
- Summary statistics: total files, folders, total size
- Graceful handling of file/folder access errors

---

## Supported Commands

- `all file info in *name folder*` - Info about all files in a folder
- `all file info in all folders *name folder*` - Recursive info about all files in folder and subfolders
- `info *name file*` - Info about a specific file
- `info *name folder*` - Info about a folder
- `to *name folder*` - Change to specified folder
- `back` - Go back or exit current view

---

## Flexible User Settings

Customize your experience with rich configuration options accessible anytime via a console menu:

- Show/hide size info
- Show creation and/or last modification dates
- Enable or disable size-based highlighting
- Display full absolute path or shortened path with "..." for better readability
- Set custom size thresholds (in MB) for color highlighting:
  - Minimum size
  - Medium size
  - Above average
  - Maximum size

---

## Installation and Running

1. Clone the repository:
   
git clone https://github.com/Rywent/Console-folder-analyzer.git

2. Open in Visual Studio or any IDE supporting .NET 8
3. Build the solution
4. Run from console or IDE and follow the interactive menu

---

## Requirements

- .NET 6 SDK or higher
- Compatible with Windows, Linux, and macOS

---

## Example Output

ProjectRoot (25GB) 🔴
├── SubFolder (2GB) 🟠
│ ├── file1.txt (3MB) 🟢
│ └── empty_folder (0B) ⚪️
└── file_in_root.ext (500MB) 🟡

## Contribution

Always open for contributions! Feel free to open issues or submit pull requests with your ideas or fixes.

Thank you for using **Console Folder Analyzer**!
