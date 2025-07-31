using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFolderAnalyzer
{
    /// <summary>
    /// Provides methods for printing folder and file information to the console.
    /// Utilizes settings for display options and formatting.
    /// </summary>
    internal class PrintInformation
    {
        SettingsManager _settings;
        DirectoryScanner _scaner = new DirectoryScanner();
        public PrintInformation (SettingsManager settings)
        {
            _settings = settings;
        }

        #region Directory tree

        /// <summary>
        /// Recursively prints the directory tree starting from the specified folder.
        /// </summary>
        public void PrintFolder(NodeFolder folder, List<bool> drawPipes)
        {
            // if the folder is root, then we output its name
            if (drawPipes.Count == 0)
            {
                Console.WriteLine($"{folder.nameFolder.ToUpper()}(FOLDER)");
            }
            else
            {
                // formation of the indent
                string indent = GetIndent(drawPipes);

                // branch: is it the last element or not
                bool isLast = drawPipes[drawPipes.Count - 1];
                string branch = isLast ? "└── " : "├── ";

                // folder output depending on settings
                Console.WriteLine($"{indent}{branch}{folder.nameFolder}(FOLDER)");
            }

            int totalCount = folder.filesInFolder.Count + folder.childFolder.Count;

            for (int i = 0; i < totalCount; i++)
            {
                bool isLastElement = (i == totalCount - 1);

                if (i < folder.filesInFolder.Count)
                {
                    // files
                    // adding nesting
                    var fileDrawPipes = new List<bool>(drawPipes) { isLastElement };

                    // get an indent
                    string indentForFile = GetIndent(fileDrawPipes);

                    string fullFilePath = folder.filesInFolder[i];
                    string fileNameOnly = Path.GetFileName(fullFilePath);

                    string fileBranch = isLastElement ? "└── " : "├── ";

                    FileInfo fi = new FileInfo(fullFilePath);
                    long fileSize = fi.Length;
                    double megabytes = (double)fileSize / (1024 * 1024);

                    // output of the file depending on the settings

                    Console.Write($"{indentForFile}{fileBranch}"); // branch and retreat separately so that it doesn't get colored

                    if (_settings.Highlight)
                    {
                        if (fi.Length == 0)
                            Console.ForegroundColor = ConsoleColor.White;
                        else if (megabytes <= _settings.minSizeLight)
                            Console.ForegroundColor = ConsoleColor.Green;
                        else if (megabytes <= _settings.mediumSizeLight)
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        else if (megabytes > _settings.aboveAverageSizeLight)
                            Console.ForegroundColor = ConsoleColor.Red;
                        else if (megabytes > _settings.maxSizeLight)
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                    }
                    else
                    {
                        Console.ResetColor();
                    }
                    Console.Write(fileNameOnly);
                    Console.ResetColor();

                    if (_settings.ShowSize)
                        Console.Write($" ({fileSize} B) ({megabytes:F2} MB)");
                    if (_settings.ShowCreationDate)
                        Console.Write($" ||Created: {fi.CreationTime}||");
                    if (_settings.ShowDateChange)
                        Console.Write($" ||Last change: {fi.LastWriteTime}||");

                    Console.WriteLine();
                }
                else
                {
                    // folders
                    int folderIndex = i - folder.filesInFolder.Count;
                    NodeFolder childNode = folder.childFolder[folderIndex];

                    // passing the level of nesting
                    var newDrawPipes = new List<bool>(drawPipes) { isLastElement };
                    PrintFolder(childNode, newDrawPipes);
                }
            }
        }

        /// <summary>
        /// Generates indentation string based on the nesting level and branch drawing flags.
        /// </summary>
        private static string GetIndent(List<bool> drawPipes)
        {
            string indent = "";
            // if the level is not the last
            for (int i = 0; i < drawPipes.Count - 1; i++)
            {
                indent += drawPipes[i] ? "    " : "│   ";
            }
            return indent;
        }

        #endregion

        #region File information

        /// <summary>
        /// Prints detailed information for an array of files.
        /// </summary>
        public void PrintFilesInfo(FileInfo[] files)
        {
            PrintHeaderFile();

            foreach (var file in files)
            {
                PrintFileInfo(file);
            }
        }

        /// <summary>
        /// Prints the header row for the files information table.
        /// </summary>
        public static void PrintHeaderFile()
        {
            const string headerFormat = "{0,-35} {1,-15} {2,-12:F2} {3,-12:F2} {4,-20} {5,-20} {6,-20} {7,-20} {8,-16} {9}"; // special format

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(headerFormat,
                "   File Name", "File Extension", "Weight(B)", "Weight (MB)", "IsReadOnly", "Creation Date",
                "Last Access", "Last Write", "Attributes", "Absolute Path"); // columns
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('=', 200)); // separating line
        }

        /// <summary>
        /// Formats file information into a single string line with specified layout.
        /// </summary>
        string FormatFileInfo(FileInfo fileInfo)
        {
            const string rowFormat = "{0,-15} {1,-12} {2,-12:F2} {3,-20} {4,-20} {5,-20} {6,-16} {7,-25} {8}";

            string extension = !string.IsNullOrEmpty(fileInfo.Extension) ? fileInfo.Extension : "None";
            string isReadOnly = fileInfo.IsReadOnly ? "Yes" : "No";
            string attributes = fileInfo.Attributes.ToString();

            string filePath = _settings.ShortenAbsolutePath ? PathSeparator(fileInfo.FullName) : fileInfo.FullName;

            return string.Format(rowFormat,
                extension,
                fileInfo.Length,
                (double)fileInfo.Length / (1024 * 1024),
                isReadOnly,
                fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                fileInfo.LastAccessTime.ToString("yyyy-MM-dd HH:mm:ss"),
                fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                attributes,
                filePath
            );
        }

        /// <summary>
        /// Prints information of a single file with colored file name.
        /// </summary>
        void PrintFileInfo(FileInfo fileInfo)
        {
            // split the name and extension
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);

            // output the name
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(fileNameWithoutExt.PadRight(35));
            Console.ResetColor();
            Console.Write(" "); // indentation after the name

            // output of the rest
            string rest = FormatFileInfo(fileInfo);
            Console.WriteLine(rest);
        }

        #endregion

        #region Folder information

        /// <summary>
        /// Prints detailed information about a folder.
        /// </summary>
        public void PrintFolderInfo(string folderPath)
        {
            PrintHeaderFolder();

            PrintFolderLine(folderPath);
        }

        /// <summary>
        /// Prints the header row for the folders information table.
        /// </summary>
        public static void PrintHeaderFolder()
        {
            const string headerFormat = "{0,-35} {1,-10} {2,10} {3,12} {4,14} {5,-20} {6,-20} {7,-20} {8,25} {9}";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(headerFormat,
                "   Folder Name", "Num File", "Num subfolder", "Weight(B)", "Weight (MB)",  "Creation Date",
                "Last Access", "Last Write", "Attributes", "Absolute Path"); // columns
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('=', 200)); // separating line
        }

        /// <summary>
        /// Prints a single line of folder information with colored folder name.
        /// </summary>
        void PrintFolderLine(string folderPath)
        {
            const string rowFormat = "{0,-10} {1,-10} {2,12:N0} {3,14:F2} {4,-20} {5,-20} {6,-20} {7,25} {8}";

            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            long size = _scaner.GetFolderSize(dirInfo);

            string folderPathShort = _settings.ShortenAbsolutePath ? PathSeparator(dirInfo.FullName) : dirInfo.FullName;

            // print the folder name in color only
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(dirInfo.Name.PadRight(35));
            Console.ResetColor();
            Console.Write(" "); // space after the name

            // forming the rest of the line without a name
            string rest = string.Format(rowFormat,
                dirInfo.GetFiles().Length,
                dirInfo.GetDirectories().Length,
                size,
                (double)size / (1024 * 1024),
                dirInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                dirInfo.LastAccessTime.ToString("yyyy-MM-dd HH:mm:ss"),
                dirInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                dirInfo.Attributes.ToString(),
                folderPathShort
            );

            Console.WriteLine(rest);
        }

        #endregion

        /// <summary>
        /// Shortens the absolute path by collapsing middle directories into an ellipsis.
        /// </summary>
        string PathSeparator(string path)
        {
            string[] parts = path.Split(Path.DirectorySeparatorChar);
            if (parts.Length < 4)
                return path;
            string[] front = parts.Take(2).ToArray();
            string[] back = parts.Skip(parts.Length - 2).ToArray();
            string shortPath = string.Join(Path.DirectorySeparatorChar, front)
                   + Path.DirectorySeparatorChar + "..." + Path.DirectorySeparatorChar
                   + string.Join(Path.DirectorySeparatorChar, back);

            return shortPath;
        }





    }
}
