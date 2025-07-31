using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFolderAnalyzer
{
    /// <summary>
    /// Represents a folder node in the directory structure.
    /// Contains the folder name, list of files, and child folders.
    /// </summary>
    internal class NodeFolder 
    {
        public string nameFolder {get; set; }
        public List<string> filesInFolder {get; set; }
        public List<NodeFolder> childFolder {get; set; }



    }
}
