using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    /// <summary>
    /// Document class, contain all meta data that needs to be saved on the document
    /// </summary>
    [Serializable]
    public class Document
    {
        public string DocName { get; set; }
        public int DocLength { get; set; }
        public string MostFrequentTerm { get; set; }
        public int FrequentTermNumberOfInstances { get; set; } // number of instances of the most frequent term in the document
        public string FolderName { get; set; }
        public string DocHeadLine { get; set; }
        public int DocLocationAtFolder { get; set; }
        public double TotalSquaredTfIdf { get; set; }
        public int UniqueWordsLength { get; set; }

        public Document(string folderName, string docName, string docHeadLine, int docLocationAtFolder)
        {
            FolderName = folderName;
            DocName = docName;
            DocHeadLine = docHeadLine;
            DocLocationAtFolder = docLocationAtFolder;
            TotalSquaredTfIdf = 0.0;
            DocLength = 0;
        }

    }
}
