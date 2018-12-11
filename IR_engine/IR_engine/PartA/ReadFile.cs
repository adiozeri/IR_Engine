using IR_engine.PartA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SearchEngine.PartA
{
    /// <summary>
    /// Reads folders contain files with documents and returns the documents texts
    /// </summary>
    class ReadFile
    {
        static int DocID = 1;
        private string DocStart { get; }
        private string DocEnd { get; }
        private string[] FoldersList { get; set; }
        private List<string> DocumentsTextList { get; set; }
        private int LastReadIndex { get; set; }
        Dictionary<int, Document> documentsList;
        private string DocIidentifierEnd;
        private string DocIidentifierStart;
        private string FbTitleIidentifierStart;
        private string FbTitleIidentifierEnd;
        private string FtTitleIidentifierStart;
        private string FtTitleIidentifierEnd;

        public void ResetReadFile()
        {
            DocID = 1;
        }
        /// <summary>
        /// create new instance of ReadFile.
        /// </summary>
        /// <param name="path">The root path of the sub-folders</param>
        /// <param name="documentsList">reference to documentsList in order to add new document with DocID and 
        /// the meta data in case we will need it in part 2 </param>
        public ReadFile(string path, ref Dictionary<int, Document> documentsList)
        {
            this.documentsList = documentsList;
            DocStart = "<DOC>";// the deliemter of start document
            DocEnd = "</DOC>"; // the delimeter of end document
            DocIidentifierStart = "<DOCNO>";// the deliemter of start header
            DocIidentifierEnd = "</DOCNO>";// the deliemter of end header
            FbTitleIidentifierStart = "<TI>";// the deliemter of start header
            FbTitleIidentifierEnd = "</TI>";
            FtTitleIidentifierStart = "<HEADLINE>";
            FtTitleIidentifierEnd = "</HEADLINE>";
            LastReadIndex = 0; // in order to know when we stop reading
            FoldersList = Directory.GetDirectories(path); // build the folders list - gets all the sub-folders from the root path
            DocID = 1;
        }
        /// <summary>
        ///  Reading the number of folders given
        /// </summary>
        /// <param name="numberOfFoldersToRead">the number of folders needs to be read</param>
        /// <returns>The texts in the documents of the folders</returns>
        public List<string> ReadFodler(int numberOfFoldersToRead)
        {

            DocumentsTextList = new List<string>();
            // move on the folderList form the last cursor to the last cursor + the number of folders needs to be read
            for (int i = LastReadIndex; i < LastReadIndex + numberOfFoldersToRead && i < FoldersList.Length; i++)
            {
                string file = Directory.GetFiles(FoldersList[i])[0]; // get the file in the folder
                ReadDocumentsInFiles(file, FoldersList[i]); // read all documents in the file
            }
            // update the curosr to the current position we  stop
            LastReadIndex = LastReadIndex + numberOfFoldersToRead;
            return DocumentsTextList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        private void ReadDocumentsInFiles(string file, string FolderName)
        {
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    string fileOfDocuments = sr.ReadToEnd();
                    AddToDocumentsList(fileOfDocuments, FolderName);
                }
            }
            catch (Exception e)
            {
                throw new Exception("The file could not be read:", e);
            }
        }

        private void AddToDocumentsList(string fileOfDocuments, string FolderName)
        {

            string[] documentsInFile = fileOfDocuments.Split(new string[] { DocStart }, StringSplitOptions.RemoveEmptyEntries);
            int docLocationAtFolder = 0;
            foreach (string doc in documentsInFile)
            {
                if (!doc.Equals("\n"))
                {
                    try
                    {

                        string[] splitDoc = doc.Split(new string[] { "<TEXT>" }, StringSplitOptions.RemoveEmptyEntries);
                        string text = splitDoc[1].Split(new string[] { "</TEXT>" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (text != "\n")
                        {
                            DocumentsTextList.Add(text);
                            string splitDocNo = "";
                            string splitTitle = "";
                            try
                            {

                                splitDocNo = doc.Split(new string[] { DocIidentifierStart, DocIidentifierEnd }, StringSplitOptions.RemoveEmptyEntries)[1];
                                splitTitle = doc.Split(new string[] { FbTitleIidentifierStart, FbTitleIidentifierEnd, FtTitleIidentifierStart, FtTitleIidentifierEnd }, StringSplitOptions.RemoveEmptyEntries)[1];
                            }
                            catch(Exception){}
                            documentsList[DocID] = new Document(FolderName, splitDocNo, splitTitle, docLocationAtFolder);
                            DocID++;
                        }
                    }
                    catch // doc not contain text, continue to the next document
                    {
                        continue;
                    }
                }
                docLocationAtFolder++;
            }
        }
    }
}