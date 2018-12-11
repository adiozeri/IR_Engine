using SearchEngine.PartA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    /// <summary>
    /// index the documents, 
    /// </summary>
    class Indexer
    {
        IRController controller;
        static int DocID = 1;
        static int TemporalPostingFileID = 1;
        static int MergeIteration = 1;
        public bool FinalMerge = false;
        static int FinalPostingName = 97;
        Dictionary<string, int> termsTFs;
        public Indexer(IRController controller)
        {
            this.controller = controller;
            termsTFs = new Dictionary<string, int>((StringComparer.InvariantCultureIgnoreCase));
        }
        public void ReseIndexData()
        {
            DocID = 1;
            TemporalPostingFileID = 1;
            MergeIteration = 1;
            FinalMerge = false;
            FinalPostingName = 97;
            termsTFs = new Dictionary<string, int>((StringComparer.InvariantCultureIgnoreCase));
        }
        /// <summary>
        /// Index list of texts into temporal posting dictionary
        /// </summary>
        /// <param name="docs">List of texts</param>
        /// <returns>Dictionary of terms and posting data</returns>
        public Dictionary<string, TermPostingList> IndexDocumentsToTemporalPostingFile(List<string> docs)
        {
            Dictionary<string, TermPostingList> TemporalPostingFile = new Dictionary<string, TermPostingList>((StringComparer.InvariantCultureIgnoreCase));

            foreach (string doc in docs) // for each document text in docs 
            {
                // parse the document text into dictionary of terms and it's frequency and update the max term in the document
                var maxTerm = new KeyValuePair<string, int>();
                Dictionary<string, int> termsInDocumentDictionary = controller.parser.ParseText(doc, out maxTerm);
                // update the document data in the documents list
                controller.DocumentsDataList[DocID].DocLength = termsInDocumentDictionary.Count;
                controller.DocumentsDataList[DocID].MostFrequentTerm = maxTerm.Key;
                controller.DocumentsDataList[DocID].FrequentTermNumberOfInstances = maxTerm.Value;
                // for each entry in the terms dictionary update the data in the temporal posting dictionary
                foreach (KeyValuePair<string, int> entery in termsInDocumentDictionary)
                {
                    if (!TemporalPostingFile.ContainsKey(entery.Key)) // if the key not exist in the temporal posting dictionary create new instance
                        TemporalPostingFile[entery.Key] = new TermPostingList();

                    if (!termsTFs.ContainsKey(entery.Key)) // if the key not exist in the termsTFs dictionary create new instance
                        termsTFs[entery.Key] = entery.Value;
                    else
                        termsTFs[entery.Key] += entery.Value; // add the TF to total TF of the term in the termsTFs dictionary

                    TermPostingData termData = new TermPostingData(entery.Value, DocID); // create term posting data
                    TemporalPostingFile[entery.Key].AddToTermPostingList(termData); // update the data in the dictionary
                }
                DocID++; // move to the next doc in the courpus
            }
            return TemporalPostingFile;
        }
        /// <summary>
        /// Write the temporal dictionary into file
        /// </summary>
        /// <param name="temporalPosting">refernce to the temporal dictionary that needs to be writen</param>
        public void WriteToTemporalFile(Dictionary<string, TermPostingList> temporalPosting)
        {
            // sort the keys list of the dictionary in order to write the dictionary in a sorted way
            var list = temporalPosting.Keys.ToList();
            list.Sort();
            // open new file - the name file is the mergeIteration and the number of temporal posting file.
            using (StreamWriter file = new StreamWriter($@"{controller.TempPostingPath}\{MergeIteration}-{TemporalPostingFileID.ToString()}"))
            {
                TemporalPostingFileID++; // for the next temporal file
                foreach (string entery in list) // write every entery in the dictionary in a new word
                {
                    file.WriteLine(entery + "*" + temporalPosting[entery].ToString()); // write the term, then '*' delimeter and then all the posting data
                }
            }
        }
        /// <summary>
        /// merge all the posting file into 27 merged posting file - each for every letter and another for numbers and symbols 
        /// </summary>
        public void MergePostingFiles()
        {
            CountdownEvent countDown; // using safe thread - we want that each iteration ends the merge and only in the end move to the next iteration
            int remainingPostFiles = Directory.GetFiles(controller.TempPostingPath).Length; // each iteration save the number of files left to merge

            while (remainingPostFiles > 1) // while there is more then 1 temp posting to merge
            {
                int docNameLevelIteration = 1; // the name of the file , starts from 1 in each level
                countDown = new CountdownEvent((int)remainingPostFiles / 2); // this is the number of merges needed for the current level, also be the number of threads of merge needs to be finished till the next level starts
                int i = 1;

                // case odd - if there is odd number of files we move the first file to the next level
                if (remainingPostFiles % 2 == 1)
                {
                    // change the name of the file to be the first file in the next level of merge
                    File.Move($@"{controller.TempPostingPath}\{MergeIteration}-1", $@"{controller.TempPostingPath}\{MergeIteration + 1}-{docNameLevelIteration}");
                    docNameLevelIteration++; // if we added one file to the next level, the next name file need to grow
                    i++;
                }
                // for the current level, merge every 2 file
                for (; i <= remainingPostFiles; i += 2)
                {
                    // sets the name of the files needed to merge and the name of the merged file
                    string file1 = $@"{controller.TempPostingPath}\{MergeIteration}-{i}";
                    string file2 = $@"{controller.TempPostingPath}\{MergeIteration}-{i + 1}";
                    string tempMergeFile = $@"{controller.TempPostingPath}\{MergeIteration + 1}-{docNameLevelIteration}"; // the file will belong to the next level
                    // call to mergeTowPostingFiles 
                    ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
                    {
                        if (remainingPostFiles != 2) // check that we not int the last level - the last level will be that there is only 2 files in the folder needs to be merged 
                            MergeTwoPostingFiles(file1, file2, tempMergeFile);

                        else // case we in the last merge - update the FinalMerge flag and call to merge tow posting file with the special posting name
                        {
                            FinalMerge = true;
                            MergeTwoPostingFiles(file1, file2, $@"{controller.PostingPath}\Special");
                        }

                        countDown.Signal(); // sign the thread to the the count in order to update that it finished merge (decreade number of threads needed to be wait)
                    }));
                    docNameLevelIteration++; // now we go to the next 2 files in this level, increase the number of merge doc
                }
                countDown.Wait(); // wait that all merges in this level finished
                MergeIteration++; // increase the merge iteration to the next merge level
                remainingPostFiles = Directory.GetFiles($"{controller.TempPostingPath}").Length; // update the number of files left to be merged
            }
        }
        /// <summary>
        /// get 2 posting file and merge to the new file
        /// </summary>
        /// <param name="posting1">the name of the first file</param>
        /// <param name="posting2">the name of the second file</param>
        /// <param name="mergeFile">the name of the new merged file</param>
        public void MergeTwoPostingFiles(string posting1, string posting2, string mergeFile)
        {
            int rowNumber = 1;
            int bufferlen = 3; // every time we loaded 3 lines from file

            // Open the files
            StreamReader readers1 = new StreamReader(posting1);
            StreamReader readers2 = new StreamReader(posting2);

            // Make the queues
            Queue<string> q1 = new Queue<string>(bufferlen);
            Queue<string> q2 = new Queue<string>(bufferlen);

            // Load the queues
            for (int i = 0; i < bufferlen; i++)
            {
                if (readers1.Peek() < 0) break;
                q1.Enqueue(readers1.ReadLine());
                if (readers2.Peek() < 0) break;
                q2.Enqueue(readers2.ReadLine());
            }

            // Merge!
            StreamWriter sw = new StreamWriter(mergeFile);
            bool done = false;
            string lowest_value;
            while (!done)
            {
                // Find the chunk with the lowest value
                int lowest_index = 1;
                if (q1 != null)
                {
                    lowest_value = q1.Peek();
                    if (q2 != null)
                    {            
                        int compareValue = string.Compare((q2.Peek()).Split('*')[0], (q1.Peek()).Split('*')[0]);
                        if (compareValue < 0) // case the term in the second file needs to be before the line in the first file
                        {
                            lowest_value = q2.Peek();
                            lowest_index = 2;
                        }
                        else if (compareValue == 0) // case both line describe the same term
                        {
                            lowest_value += "~" + q2.Peek().Split('*')[1]; // merge the two lines
                            lowest_index = 3; // special case
                        }
                    }
                }
                else if (q2 != null)
                {
                    lowest_value = q2.Peek();
                    lowest_index = 2;
                }
                else
                {
                    // Was nothing found in any queue means we done.
                    done = true;
                    break;
                }

                // Output it
                if (FinalMerge) // case this is the final merge
                {
                    if (lowest_value[0] == FinalPostingName) // case this is the first term that start with new letter
                    {
                        sw.Close(); // close the current posting file
                        string newPath = $@"{controller.PostingPath}\" + ((char)FinalPostingName).ToString(); // create new posting for the current letter
                        sw = new StreamWriter(newPath); // create file with the new path
                        FinalPostingName++; // increace the name - to be match the next letter
                        rowNumber = 1; // reset the row number to the first row
                    }
                    if (UpdatePostingLineInTheInvertedIndexAndCache(lowest_value, rowNumber)) // add the data to the inverted index
                    {
                        sw.WriteLine(lowest_value); // write the choosen line to the file                
                        rowNumber++; // incrase the row number in each iteration of writing
                    }
                }
                else
                {
                    sw.WriteLine(lowest_value); // write the choosen line to the file                
                }

                if (lowest_index == 1 || lowest_index == 3) // case the line writen is from file 1
                {
                    q1.Dequeue();
                    if (q1.Count == 0) // case the queue is empth get 3 more lines
                    {
                        for (int i = 0; i < bufferlen; i++)
                        {
                            if (readers1.Peek() < 0) break;
                            q1.Enqueue(readers1.ReadLine());
                        }

                        // case there was nothing left to read
                        if (q1.Count == 0)
                        {
                            q1 = null; // EOF
                        }
                    }
                }
                if (lowest_index == 2 || lowest_index == 3) // case the line writen is from file 2 
                {
                    q2.Dequeue();
                    if (q2.Count == 0)
                    {
                        for (int i = 0; i < bufferlen; i++)
                        {
                            if (readers2.Peek() < 0) break;
                            q2.Enqueue(readers2.ReadLine());
                        }

                        // case there was nothing left to read
                        if (q2.Count == 0)
                        {
                            q2 = null; // EOF
                        }
                    }
                }
            }
            sw.Close(); // close the file

            // Close and delete the files
            readers1.Close();
            readers2.Close();
            File.Delete(posting1);
            File.Delete(posting2);
        }
        /// <summary>
        /// update posting data in the inverted index
        /// </summary>
        /// <param name="postingLine">the line from the posting line</param>
        /// <param name="numberOfLineInPostingFile">number of pointer row</param>
        public bool UpdatePostingLineInTheInvertedIndexAndCache(string postingLine, int numberOfLineInPostingFile)
        {
            string[] postingList = postingLine.Split('*');// the term needs to be update
            string[] postingLinks = postingList[1].Split('~');
            int docFrequnecy = postingLinks.Length; // split the term data to get the number of documents the term appers in
            if (docFrequnecy < 2 && postingLinks[0].Substring(postingLinks[0].IndexOf(',') + 1).Equals("1"))
                return false;
            controller.InvertedIndex.Add(postingList[0], new TermDataInMap(docFrequnecy, numberOfLineInPostingFile, DocID, termsTFs[postingList[0]])); //termsTFs[postingList[0]]
            if (controller.InvertedIndex[postingList[0]].TotalTF * controller.InvertedIndex[postingList[0]].Idf > Properties.Settings.Default.CacheThreshold)
            {
                CacheData cacheData = new CacheData(postingLine, numberOfLineInPostingFile, Properties.Settings.Default.CachePostingListLimit);
                controller.CacheDictionary.Add(postingList[0], cacheData);
                controller.InvertedIndex[postingList[0]].TermDictionaryPointer = TermDictionaryPointer.Cache;
            }
            return true;
        }
       
    }
}
