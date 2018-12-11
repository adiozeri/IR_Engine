using SearchEngine.PartA;
using IR_engine.PartA;
using IR_engine.PartB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IR_engine
{
    public delegate void delegateManager(string DoneMessage);

    public class IRController
    {
        #region Index Properties

        #region data
        public string LoadingAndSavingPath { get; set; }
        public string InputPath { get; set; }
        public string CorpusPath { get; set; }
        public string ResultPath { get; set; }
        public string StopWordsPath { get; set; }
        public string TempPostingPath { get; set; }
        public string PostingPath { get; set; }
        public bool Stemming { get; set; }
        public bool documentSummaryOption { get; set; }

        #endregion
        public event delegateManager IndexDone;
        public event delegateManager LoadDone;
        Thread WritingToFileThread; // thread for IO actions
        private Dictionary<string, TermDataInMap> invertedIndex = new Dictionary<string, TermDataInMap>();
        public Dictionary<string, TermDataInMap> InvertedIndex { get { return invertedIndex; } private set { } }
        private Dictionary<string, CacheData> cacheDictionary = new Dictionary<string, CacheData>();
        public Dictionary<string, CacheData> CacheDictionary { get { return cacheDictionary; } private set { } }
        public Dictionary<int, Document> DocumentsDataList = new Dictionary<int, Document>();
        public Parse parser;

        #endregion

        #region Search Properties

        public bool DocumentSummary { get; set; }
        public string SavingQueryResultPath { get; set; }
        public string SavingFileName { get; set; }
        public event delegateManager SearchDone;
        public Searcher searcher;
        public List<string> queryFilesNameResults = new List<string>();
        public List<string> QueryFilesNameResults { get { return queryFilesNameResults; } }

        public Dictionary<string, Dictionary<Document, double>> QueryResults { get; set; }
        public int QueryID { get; set; }
        public string QueriesFilePath { get; set; }
        public string QueryToSearch { get; set; }


        #endregion

        public IRController()
        {
            QueryID = 1;
        }

        #region Index Functionality

        /// <summary>
        /// main function. when the user press index the view call this function
        /// this function represent the whole flow for building the postings files, inverted index and cache 
        /// </summary>
        public void Start()
        {
            invertedIndex.Clear();
            cacheDictionary.Clear();
            DocumentsDataList.Clear();
            OpenFoldersAndUpdateSettings();
            parser = new Parse(StopWordsPath, Stemming);
            ReadFile rf = new ReadFile(CorpusPath, ref DocumentsDataList);
            rf.ResetReadFile();
            Indexer index = new Indexer(this);
            index.ReseIndexData();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            int iteration = 0;
            while (iteration < Properties.Settings.Default.NumberOfGroupsFilesToIndex) // for all group of files do:
            {
                List<string> docs = rf.ReadFodler(Properties.Settings.Default.NumberOfFilesPerItreation); // read from folders
                Dictionary<string, TermPostingList> temporalPostingFile = index.IndexDocumentsToTemporalPostingFile(docs); // create temp posting dictionary
                if (WritingToFileThread != null) // if the writing thread is free write this temp dictionary to file, else wait untill done
                    WritingToFileThread.Join();
                WritingToFileThread = new Thread(() => { index.WriteToTemporalFile(temporalPostingFile); });

                WritingToFileThread.Start();
                iteration++;
            }
            WritingToFileThread.Join(); // wait all threads of writing to finish

            WritingToFileThread = new Thread(() => { WriteDocumentsInfo(); }); // start writing the documents dictionary to file in  another thread

            WritingToFileThread.Start();
            index.MergePostingFiles(); // merging the temporary posting files into final posting files
            WritingToFileThread.Join();
            Directory.Delete(TempPostingPath); // delete the temporal posting dir

            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
            ts.Hours, ts.Minutes, ts.Seconds);
            // return ending message to user
            IndexDone($"Number Of Documents: {DocumentsDataList.Count}, Inverted Index Size: {GetDictionaryBytesSize()} , Cache Size: {GetCacheBytesSize()},Index Run Time: {elapsedTime} ");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>the size of the inverted index in bytes </returns>
        private string GetDictionaryBytesSize()
        {
            long size = 0;
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, InvertedIndex);
                size = s.Length;
            }
            return size.ToString() + "Bytes";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>the size of the cache in bytes</returns>
        private string GetCacheBytesSize()
        {
            long size = 0;
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, CacheDictionary);
                size = s.Length;
            }
            return size.ToString() + "Bytes";
        }
        /// <summary>
        /// write the inverted index into binary file in the result folder
        /// </summary>
        private void WriteDictionaryToFolder()
        {
            FileStream fs = File.Open($@"{LoadingAndSavingPath}\Dictionary", FileMode.Create);
            BinaryFormatter writer = new BinaryFormatter();
            writer.Serialize(fs, invertedIndex);
            fs.Close();
        }
        /// <summary>
        ///  write the cache into binary file in the result folder
        /// </summary>
        private void WriteCacheToFolder()
        {
            FileStream fs = File.Open($@"{LoadingAndSavingPath}\Cache", FileMode.Create);
            BinaryFormatter writer = new BinaryFormatter();
            writer.Serialize(fs, CacheDictionary);
            fs.Close();
        }

        private void WriteDocumentsToFolder()
        {
            FileStream fs = File.Open($@"{LoadingAndSavingPath}\Documents", FileMode.Create);
            BinaryFormatter writer = new BinaryFormatter();
            writer.Serialize(fs, DocumentsDataList);
            fs.Close();
        }

        /// <summary>
        /// load the dictionary from folder into the invertd index 
        /// </summary>
        private void LoadDictionaryFromFolder()
        {
            FileStream fs = File.Open($@"{LoadingAndSavingPath}\Dictionary", FileMode.Open);
            BinaryFormatter reader = new BinaryFormatter();
            invertedIndex.Clear();
            invertedIndex = (Dictionary<string, TermDataInMap>)reader.Deserialize(fs);
            fs.Close();
        }

        private void LoadDocumentsFromFolder()
        {
            FileStream fs = File.Open($@"{LoadingAndSavingPath}\Documents", FileMode.Open);
            BinaryFormatter reader = new BinaryFormatter();
            DocumentsDataList.Clear();
            DocumentsDataList = (Dictionary<int, Document>)reader.Deserialize(fs);
            fs.Close();
        }

        /// <summary>
        ///  load the cache from folder into the cache dictionary 
        /// </summary>
        private void LoadCacheFromFolder()
        {
            FileStream fs = File.Open($@"{LoadingAndSavingPath}\Cache", FileMode.Open);
            BinaryFormatter reader = new BinaryFormatter();
            cacheDictionary.Clear();
            cacheDictionary = (Dictionary<string, CacheData>)reader.Deserialize(fs);
            fs.Close();
        }
        /// <summary>
        /// save all data - inverted index and cache
        /// </summary>
        public void SaveDataToFolder()
        {
            WriteDictionaryToFolder();
            WriteCacheToFolder();
            WriteDocumentsToFolder();
        }
        /// <summary>
        /// load all data - inverted index and cache
        /// </summary>
        public void LoadDataFromFolder()
        {
         
            Thread DictionaryLoad = new Thread(() => { LoadDictionaryFromFolder(); });
            DictionaryLoad.Start();
            Thread CacheLoad = new Thread(() => { LoadCacheFromFolder(); });
            CacheLoad.Start();
            Thread DocumentLoad = new Thread(() => { LoadDocumentsFromFolder(); });
            DocumentLoad.Start();
            DictionaryLoad.Join();
            CacheLoad.Join();
            DocumentLoad.Join();
            // AddTfIdfToDocuments();
            LoadDone("Load Done");

        }
        /// <summary>
        /// delete all files created
        /// </summary>
        internal void Reset()
        {
            if (Directory.Exists($@"{ResultPath}\Posting With Stemming"))
            {
                Directory.Delete($@"{ResultPath}\Posting With Stemming", true);
            }
            if (Directory.Exists($@"{ResultPath}\Posting"))
            {
                Directory.Delete($@"{ResultPath}\Posting", true);
            }
            if (File.Exists($@"{LoadingAndSavingPath}\Dictionary"))
            {
                File.Delete($@"{LoadingAndSavingPath}\Dictionary");
            }
            if (File.Exists($@"{LoadingAndSavingPath}\Cache"))
            {
                File.Delete($@"{LoadingAndSavingPath}\Cache");
            }
            InvertedIndex.Clear();
            CacheDictionary.Clear();
            DocumentsDataList.Clear();
        }
        /// <summary>
        /// before starting the process update all needed paths
        /// </summary>
        private void OpenFoldersAndUpdateSettings()
        {
            if (Stemming)
            {
                Directory.CreateDirectory($@"{ResultPath}\TempPostingWithStem");
                TempPostingPath = $@"{ResultPath}\TempPostingWithStem";
                Directory.CreateDirectory($@"{ResultPath}\Posting With Stemming");
                PostingPath = $@"{ResultPath}\Posting With Stemming";
            }
            else
            {
                Directory.CreateDirectory($@"{ResultPath}\TempPosting");
                TempPostingPath = $@"{ResultPath}\TempPosting";
                Directory.CreateDirectory($@"{ResultPath}\Posting");
                PostingPath = $@"{ResultPath}\Posting";
            }
            StopWordsPath = $@"{InputPath}\stop_words.txt";
            CorpusPath = $@"{InputPath}";
        }
        /// <summary>
        /// updae the path for saving and loading the data
        /// </summary>
        /// <param name="selectedPath">the path that user insert for saving and looading data</param>
        internal void UpdateSavingAndLoadingPath(string selectedPath)
        {
            LoadingAndSavingPath = selectedPath;
        }
        /// <summary>
        ///  updae the input path with the corpus data
        /// </summary>
        /// <param name="selectedPath">the path that user insert for index the corpus</param>
        internal void UpdateCorpusPath(string selectedPath)
        {
            if (!Directory.GetFiles(selectedPath).Any(f => f == $@"{selectedPath}\stop_words.txt"))
            {
                throw new FileNotFoundException("Folder must contain the text file name stop_word");
            }

            InputPath = selectedPath;
        }
        /// <summary>
        /// write documents info into txt file
        /// </summary>
        public void WriteDocumentsInfo()
        {
            using (StreamWriter file = new StreamWriter($@"{ResultPath}/DocumentsReaderInfo", true))
            {
                foreach (KeyValuePair<int, Document> item in DocumentsDataList)
                {
                    file.WriteLine(item.Key + ":L-" + item.Value.DocLength + ",MaxTF-" + item.Value.MostFrequentTerm + "," + item.Value.FrequentTermNumberOfInstances.ToString());
                }
            }
        }

        #endregion

        #region Search Functionality

        public void Search()
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                parser = new Parse(LoadingAndSavingPath + "\\stop_words.txt", Stemming);
                searcher = new Searcher(this);
                var documentsRank = searcher.Search(QueryToSearch);

                queryFilesNameResults = documentsRank.Keys.ToList().Select(d => d.DocName).ToList();
                QueryResults = new Dictionary<string, Dictionary<Document, double>>();
                QueryResults.Add(QueryID.ToString(), documentsRank);

                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                ts.Hours, ts.Minutes, ts.Seconds);
                // return ending message to user
                SearchDone($"Number of relevent documents: {documentsRank.Count} ,Search Run Time: {elapsedTime} ");
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public void WriteResultsInfo()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(SavingQueryResultPath + "\\" + SavingFileName + ".txt"))
                {
                    foreach (var queryNumber in QueryResults.Keys)
                    {
                        foreach (var docRank in QueryResults[queryNumber])
                        {
                            file.WriteLine(queryNumber + " 0 " + docRank.Key.DocName.Trim(' ') + " 1 42.38 mt");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                SearchDone($"Something wrong {e.Message}");
            }
        }
        public Dictionary<int, Tuple<string, float>> Search5SignificantSentences(string docName)
        {
            try
            {
                parser = new Parse(LoadingAndSavingPath + "\\stop_words.txt", Stemming);
                searcher = new Searcher(this);
                return searcher.Find5SignificantSentencesInDocument(docName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void SearchQueriesFromFile()
        {
 
            try
            {
                parser = new Parse(LoadingAndSavingPath + "\\stop_words.txt", Stemming);
                searcher = new Searcher(this);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Dictionary<string, string> queriesFromFile = new Dictionary<string, string>();
                QueryResults = new Dictionary<string, Dictionary<Document, double>>();
                QueryResults.Clear();
                queryFilesNameResults = new List<string>();
                queryFilesNameResults.Clear();

                using (StreamReader sr = new StreamReader(QueriesFilePath))
                {
                    string queriesFile = sr.ReadToEnd();
                    string[] queriesFileList = queriesFile.Split(new string[] { "<top>", "</top>" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var queryFile in queriesFileList)
                    {
                        if (queryFile == "\r\n\r\n\r\n")
                            continue;
                        string[] queryFileSplit = queryFile.Split(new string[] { "<num> Number:", "<title>", "<desc> Description:", "<narr> Narrative:" }, StringSplitOptions.RemoveEmptyEntries);
                        string queryNumber = queryFileSplit[1].Trim('\r', '\n', ' ');
                        string title = queryFileSplit[2].Trim('\r', '\n', ' ');
                        string description = CleanDescriptionAndNarrative(queryFileSplit[3].Trim('\r', '\n', ' '));
                        queriesFromFile.Add(queryNumber, title + " " + description);
                    }
                    int resultsCount = 0;
                    foreach (var queryNumber in queriesFromFile.Keys)
                    {
                        var documentsRank = searcher.Search(queriesFromFile[queryNumber]);
                        QueryResults.Add(queryNumber, documentsRank);
                        QueryFilesNameResults.AddRange(documentsRank.Keys.ToList().Select(d => d.DocName).ToList());
                        resultsCount += documentsRank.Count;
                    }
                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                    ts.Hours, ts.Minutes, ts.Seconds);
                    SearchDone($"Number of relevent documents for all {queriesFromFile.Count} queries is {resultsCount} ,Total search Run Time: {elapsedTime} ");
                }
            }
            catch (Exception e)
            {
                SearchDone($"Something wrong {e.Message}");
            }
        }
        public string CleanDescriptionAndNarrative(string text)
        {
            text = text.ToLower();
            text = text.Replace("documents", String.Empty);
            text = text.Replace("document", String.Empty);
            text = text.Replace("discussing", String.Empty);
            text = text.Replace("discuss", String.Empty);
            text = text.Replace("relevant", String.Empty);
            text = text.Replace("contain", String.Empty);
            text = text.Replace("following", String.Empty);
            text = text.Replace("information", String.Empty);
            text = text.Replace("identify", string.Empty);
            return text;
        }
        public void AddTfIdfToDocuments()
        {
            foreach (var term in invertedIndex.Keys)
            {
                double idf = invertedIndex[term].Idf;
                TermPostingList postingLine = GetPostingLineFromPostingFile(term, invertedIndex[term].PointerLine);
                foreach (var termData in postingLine.PostingList)
                {
                    DocumentsDataList[termData.DocumentId].TotalSquaredTfIdf += Math.Pow((termData.TF / DocumentsDataList[termData.DocumentId].FrequentTermNumberOfInstances) * idf, 2);
                }
            }
        }
        private TermPostingList GetPostingLineFromPostingFile(string term, int pointerLine)
        {
            string fileName = term[0].ToString();
            if (!char.IsLetter(term[0]))
                fileName = "special";
            string postingPath = LoadingAndSavingPath + "\\Posting\\" + fileName; // the posting path - according to the first letter of the term
            string postingLine = File.ReadLines(postingPath).Skip(pointerLine - 1).Take(1).First(); // take the posting list in the pointer line
            return new TermPostingList(postingLine, int.MaxValue); // return new instance of term posting list from posting line with only 50 most significant documents of the term
        }
        /// <summary>
        /// delete all files and structure related to search process
        /// </summary>
        internal void SearchReset()
        {
            try
            {
                if (File.Exists($@"{SavingQueryResultPath}\{SavingFileName}"))
                {
                    File.Delete($@"{SavingQueryResultPath}\{SavingFileName}");
                }
                InvertedIndex.Clear();
                CacheDictionary.Clear();
                DocumentsDataList.Clear();
                queryFilesNameResults = new List<string>();
                queryFilesNameResults.Clear();
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        #endregion
    }
}
