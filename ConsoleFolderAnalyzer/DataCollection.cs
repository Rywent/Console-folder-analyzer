using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace ConsoleFolderAnalyzer
{
    /// <summary>
    /// Manages user input and interaction for collecting and displaying file and folder data.
    /// Processes commands and navigates directories accordingly.
    /// </summary>
    internal class DataCollection
    {
        string _HomePath;
        string _currentPath;

        readonly SettingsManager _settings;
        DirectoryScanner _scanner = new DirectoryScanner();
        PrintInformation _printInfo;
        

        string[] actionItems = new string[] // list of available commands
        {
            "<all file info in *name folder*> - get information about all files in the folder",
            "<all file info in all folders *name folder*> - retrieving information about files in the specified folder and all subfolders",
            "<info *name file*> - get information about the file by name",
            "<info *name folder*> - get information about the folder by name",
            "<to *name folder*> - move to the specified folder",
        };

        public DataCollection (SettingsManager settings)
        {
            _settings = settings;
            _printInfo = new PrintInformation(settings);
        }
        /// <summary>
        /// Handles the main input loop for receiving paths and user commands.
        /// Validates paths and manages interaction flow based on input.
        /// </summary>
        public void InputPaths()
        {
            Console.Clear();
            Console.Write("Enter the full path to the file or folder. (also specify the extension): ");
            _HomePath = Console.ReadLine().Trim();

            if (_HomePath.ToLower() == "back")
            {
                return;
            }

            // here we check if user folder exists at all
            bool isValidFile = File.Exists(_HomePath);
            bool isValidFolder = Directory.Exists(_HomePath);

            if (!isValidFile && !isValidFolder)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file or folder does not exist.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }

            _currentPath = _HomePath; // current path - home
            
            // the main loop of interaction with the user within the current directory
            while (true)
            {
                Console.Clear();

                if (File.Exists(_currentPath))
                {
                    // information about file
                    _printInfo.PrintFilesInfo(new FileInfo[] { new FileInfo(_currentPath) });
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    _currentPath = _HomePath;
                    return;
                }
                else if (Directory.Exists(_currentPath))
                {
                    try
                    {
                        // info DirectoryTree
                        NodeFolder folder = _scanner.RecursionDirectory(_currentPath);
                        _printInfo.PrintFolder(folder, new List<bool>());

                        DrawAction(); // print command menu

                        var (action, name) = GetMenuOption(); // getting user input

                        //checking the accuracy of the command
                        if (action == "to") { ChangeDirectory(name); }
                        else if (action == "info") { GetInfo(name); }
                        else if (action == "all file info in") { PrintInfoFilesInFolder(name); }
                        else if (action == "all file info in all folders") { PrintInfoAllFilesInAllFolders(name); }
                        else if (action == "back" || action == "exit") { return; }
                        else
                        {
                            Console.WriteLine("Unknown command, please try again.");
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error while processing directory: {ex.Message}");
                        Console.ForegroundColor = ConsoleColor.White;
                        _currentPath = _HomePath;
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        return;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Current path is invalid.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Press any key");
                    Console.ReadKey();
                    return;
                }
            }
        }

        /// <summary>
        /// Retrieves and displays information about all files in the specified folder and all its subfolders.
        /// </summary>
        bool PrintInfoAllFilesInAllFolders(string folderName)
        {
            string targetPath = null;
            bool simpleName = !folderName.Contains(Path.DirectorySeparatorChar) && !folderName.Contains(Path.AltDirectorySeparatorChar);

            if (simpleName) // if only the name was entered
            {
                string currentFolderName = new DirectoryInfo(_currentPath).Name;

                if (string.Equals(currentFolderName, folderName, StringComparison.OrdinalIgnoreCase))
                {
                    // the folder name matches the current one, use the current path
                    targetPath = _currentPath;
                }
                else
                {
                    //looking for the required folder in the current directory
                    string foundFolderPath = _scanner.FindFolderRecursive(_currentPath, folderName); 
                    if (foundFolderPath != null)
                    {
                        targetPath = foundFolderPath;
                    }
                    else
                    {
                        targetPath = Path.Combine(_currentPath, folderName);
                    }
                }
            }
            else
            {
                targetPath = folderName; // if you entered the full path to the folder, just use it
            }

            // if such a folder exists, we display information about the files in it
            if (Directory.Exists(targetPath))
            {
                string[] files = _scanner.FindAllFiles(targetPath);
                FileInfo[] fileInfos = files.Select(path => new FileInfo(path)).ToArray();

                _printInfo.PrintFilesInfo(fileInfos);
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                _currentPath = _HomePath;
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Folder '{folderName}' not found starting from '{_currentPath}'.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                _currentPath = _HomePath;
                return false;
            }
        }

        /// <summary>
        /// Retrieves and displays information about all files in the specified folder (non-recursive).
        /// </summary>
        bool PrintInfoFilesInFolder(string folderName)
        {
            string targetPath = null;

            bool simpleName = !folderName.Contains(Path.DirectorySeparatorChar) && !folderName.Contains(Path.AltDirectorySeparatorChar);

            if(simpleName)
            {
                
                string currentFolderName = new DirectoryInfo(_currentPath).Name;
                if (string.Equals(currentFolderName, folderName, StringComparison.OrdinalIgnoreCase))
                {
                    // the folder name matches the current one, use the current path
                    targetPath = _currentPath;
                }
                else
                {
                    // searching for the folder recursively
                    string foundFolderPath = _scanner.FindFolderRecursive(_currentPath, folderName);

                    if (foundFolderPath != null)
                    {
                        targetPath = foundFolderPath;
                    }
                    else
                    {
                        targetPath = Path.Combine(_currentPath, folderName);
                    }
                }
                
            }
            else
            {
                targetPath = folderName;
            }

            // if such a folder exists, we display information about the files in it
            if (Directory.Exists(targetPath))
            {
                string[] files;
                files = _scanner.FindAllFilesInDirectory(targetPath);
                FileInfo[] fileInfos = files.Select(path => new FileInfo(path)).ToArray();


                _printInfo.PrintFilesInfo(fileInfos);
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                _currentPath = _HomePath;
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Folder '{folderName}' not found starting from '{_currentPath}'.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                _currentPath = _HomePath;
                return false;
            }
        }

        /// <summary>
        /// Displays information about the specified file or folder, searching recursively if needed.
        /// </summary>
        bool GetInfo(string name)
        {
            string targetPath = null;

            bool simpleName = !name.Contains(Path.DirectorySeparatorChar) && !name.Contains(Path.AltDirectorySeparatorChar);

            if (simpleName)
            {
                string currentFolderName = new DirectoryInfo(_currentPath).Name;
                if (string.Equals(currentFolderName, name, StringComparison.OrdinalIgnoreCase))
                {
                    // the folder name matches the current one, use the current path
                    targetPath = _currentPath;
                }
                else
                {
                    // searching for a folder recursively
                    string foundFolderPath = _scanner.FindFolderRecursive(_currentPath, name);

                    // searching for the file recursively
                    string foundFilePath = _scanner.FindFileByNameRecursive(_currentPath, name);

                    if (foundFilePath != null)
                    {
                        targetPath = foundFilePath;
                    }
                    else if (foundFolderPath != null)
                    {
                        targetPath = foundFolderPath;
                    }
                    else
                    {
                        targetPath = Path.Combine(_currentPath, name);
                    }
                }
                
            }
            else
            {
                // a full or relative path has been entered
                targetPath = name;
            }

            // checking what has been found
            if (Directory.Exists(targetPath))
            {
                _printInfo.PrintFolderInfo(targetPath);
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                _currentPath = _HomePath;
                return true;
            }
            else if (File.Exists(targetPath))
            {
                _printInfo.PrintFilesInfo(new FileInfo[] { new FileInfo(targetPath) });
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                _currentPath = _HomePath;
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file or folder does not exist.");
                Console.ForegroundColor = ConsoleColor.White;
                _currentPath = _HomePath;
                return false;
            }
        }

        /// <summary>
        /// Changes the current directory to the specified folder, with support for relative and absolute paths.
        /// </summary>
        bool ChangeDirectory(string folderName)
        {
            string targetPath; // final path

            if (folderName.ToLower() == "home") // if the user wants to return home
            {
                targetPath = _HomePath;
            }
            else if (Path.IsPathRooted(folderName))
            {
                // the full path has been entered
                targetPath = folderName;
            }
            else if (!folderName.Contains(Path.DirectorySeparatorChar) && !folderName.Contains(Path.AltDirectorySeparatorChar))
            {
                // if only the folder name is entered, search recursively in the current directory
                string foundPath = _scanner.FindFolderRecursive(_currentPath, folderName);
                if (foundPath != null)
                    targetPath = foundPath;
                else
                    targetPath = Path.Combine(_currentPath, folderName); 
            }
            else
            {
                // if the path with subdirectories is entered, build the absolute path
                targetPath = Path.GetFullPath(Path.Combine(_currentPath, folderName));
            }

            // the folder exists, so we move
            if (Directory.Exists(targetPath))
            {
                _currentPath = targetPath;
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Folder '{folderName}' not found in '{_currentPath}'. Tried path: {targetPath}. Press any key");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                return false;
            }
        }


        /// <summary>
        /// Reads and parses the user input command, returning the action and target name.
        /// </summary>
        (string action, string name) GetMenuOption()
        {
            Console.WriteLine("Select an option:");
            while (true)
            {
                string input = Console.ReadLine().Trim();
                var (isValid, action, name) = ParseCommand(input);
                if (isValid)
                    return (action, name);
                Console.WriteLine("Invalid option. Please try again:");
            }
        }

        /// <summary>
        /// Displays the list of available commands to the user.
        /// </summary>
        void DrawAction()
        {
             const int frameInnerWidth = 150;
             
            string header = "ACTION";
            int padding = (frameInnerWidth - (header.Length + 4)) / 2;
            string top = "+" + new string('═', padding) + "[ " + header + " ]" +
                         new string('═', frameInnerWidth - (padding + header.Length + 4)) + "+";
            Console.WriteLine(top);

            foreach (string item in actionItems)
                Console.WriteLine("║ " + item.PadRight(frameInnerWidth - 1) + "║");

            Console.WriteLine("+" + new string('═', frameInnerWidth) + "+");
        }

        /// <summary>
        /// Parses a raw input string and determines if it matches a valid command format.
        /// Returns a tuple indicating validity, the action, and associated name.
        /// </summary>
        (bool isValid, string action, string name) ParseCommand(string input)
        {
            input = input.Trim();

            if (input.StartsWith("all file info in all folders "))
            {
                string namePart = input.Substring("all file info in all folders ".Length).Trim();
                if (!string.IsNullOrEmpty(namePart))
                    return (true, "all file info in all folders", namePart);
            }
            else if (input.StartsWith("all file info in "))
            {
                string namePart = input.Substring("all file info in ".Length).Trim();
                if (!string.IsNullOrEmpty(namePart))
                    return (true, "all file info in", namePart);
            }

            else if (input.StartsWith("info "))
            {
                string namePart = input.Substring("info ".Length).Trim();
                if (!string.IsNullOrEmpty(namePart))
                    return (true, "info", namePart);
            }
            else if (input.StartsWith("to "))
            {
                string namePart = input.Substring("to ".Length).Trim();
                if (!string.IsNullOrEmpty(namePart))
                    return (true, "to", namePart);
            }
            else if (input.StartsWith("back"))
                return (true, "back", "");

                return (false, null, null);
        }   
        

    }
}

