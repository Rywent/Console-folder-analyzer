using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ConsoleFolderAnalyzer
{
    /// <summary>
    /// Provides methods for scanning directories, including recursive traversal and searching for folders and files.
    /// </summary>
    internal class DirectoryScanner
    {
        /// <summary>
        /// Recursively traverses the directory at the specified path and builds a <see cref="NodeFolder"/> representing its structure.
        /// </summary>
        public NodeFolder RecursionDirectory(string path) 
        {
            NodeFolder directory = new NodeFolder();
            directory.nameFolder = Path.GetFileName(path);
            directory.filesInFolder = new List<string>();
            directory.childFolder = new List<NodeFolder>();

            try
            {
                // getting the list of files in the current folder
                directory.filesInFolder = Directory.GetFiles(path).ToList();

                // получаем список подпапок и рекурсивно обходим их
                foreach (string subDirPath in Directory.GetDirectories(path))
                {
                    try
                    {
                        var childNode = RecursionDirectory(subDirPath);
                        directory.childFolder.Add(childNode);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // obtain the list of subfolders and recursively traverse them
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Access denied to folder: {subDirPath}");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        // other possible errors when traversing the subfolder
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error accessing folder {subDirPath}: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Access denied to folder: {path}");
                Console.ResetColor();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Directory not found: {path}");
                Console.ResetColor();
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"IO error accessing folder {path}: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unexpected error accessing folder {path}: {ex.Message}");
                Console.ResetColor();
            }

            return directory;
        }

        /// <summary>
        /// Recursively searches for a folder with a specified name starting from the root path.
        /// </summary>
        public string FindFolderRecursive(string rootPath, string folderName) 
        {
            foreach (var dir in Directory.GetDirectories(rootPath))
            {
                if (string.Equals(Path.GetFileName(dir), folderName, StringComparison.OrdinalIgnoreCase))
                    return dir;

                string found = FindFolderRecursive(dir, folderName);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Recursively searches for a file with the specified name starting from the given directory.
        /// </summary>
        public string FindFileByNameRecursive(string directory, string fileName)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }

                foreach (var dir in Directory.GetDirectories(directory))
                {
                    string found = FindFileByNameRecursive(dir, fileName);
                    if (found != null)
                        return found;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during searching files: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Retrieves all files in the specified folder without searching subdirectories.
        /// </summary>
        public string[] FindAllFilesInDirectory(string pathToFolder)
        {
            try
            {
                return Directory.GetFiles(pathToFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new string[0];
            }

        }
        /// <summary>
        /// Retrieves all files in the specified folder and all its subdirectories.
        /// </summary>
        public string[] FindAllFiles(string pathToFolder)
        {
            try
            {
                return Directory.GetFiles(pathToFolder, "*.*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new string[0];
            }

        }

        /// <summary>
        /// Calculates the total size, in bytes, of the specified directory including all its subdirectories.
        /// </summary>
        public long GetFolderSize(DirectoryInfo dir)
        {
            long size = 0;
            // file sizes in the current folder
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }
            // recursively add the size of subfolders
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                size += GetFolderSize(subDir);
            }
            return size;
        }

    }
}
