using IR_engine.PartA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IR_engine.PartB
{
    public class Searcher
    {
        IRController _controller; //Engine controller includes all the data require for searching the relevent documents
        /// <summary>
        /// Create new instance of searcher class that search for relvent documents according to given query
        /// </summary>
        /// <param name="controller">The controller of the engine include all the data require for searching the relevent documents</param>
        public Searcher(IRController controller)
        {
            _controller = controller;
        }
    
        /// <summary>
        /// Search for the 50 most relevent documents retreval for the given query - the 50 documents with the highest rank
        /// </summary>
        /// <param name="query">The given query needs to find the documents according</param>
        /// <returns>Dictionary of Documents and their rank according the query order by rank</returns>
        public Dictionary<Document, double> Search(string query)
        {
            // parse the query according to documents parsing
            KeyValuePair<string,int> maxTerm;
            Dictionary<string, int> parsedTermsInQuery = _controller.parser.ParseText(query, out maxTerm); 
            // gets all posting lines of the terms in the query
            Dictionary<string, TermPostingList> querysPostingList = GetQueryPostingList(parsedTermsInQuery.Keys.ToList());
            // gets all the documents may match the given query
            Dictionary<int, Document> potentialDocs = GetPotentialDocs(querysPostingList);

            Ranker ranker = new Ranker(_controller);
            // gets the ranked documents from ranker
            Dictionary<Document, double> rankedDocuments = ranker.rankDocumentsForQuery(potentialDocs, querysPostingList, query);
          //  int x = 0;
           // Dictionary<Document, double> rankedDocuments11 = (rankedDocuments.OrderByDescending(pair => pair.Value)).Take(50).ToDictionary(pair => pair.Key, pair => pair.Value);
            //rankedDocuments = (rankedDocuments.OrderByDescending(pair => pair.Value)).Take(50).ToDictionary(pair => pair.Key, pair => pair.Value);
            return (rankedDocuments.OrderByDescending(pair => pair.Value)).Take(50).ToDictionary(pair => pair.Key, pair=>pair.Value);    
        }

        /// <summary>
        /// Gets all the posting list of the terms in the query
        /// </summary>
        /// <param name="termsInQuery">List of terms in the query</param>
        /// <returns>Dictionary of the term and it's posting list</returns>
        private Dictionary<string, TermPostingList> GetQueryPostingList(List<string> termsInQuery)
        {
            Dictionary<string, TermPostingList> queryPostingsList = new Dictionary<string, TermPostingList>();
            foreach (var term in termsInQuery) // for each term in the query
            {
                TermPostingList termPostingList = null;
                if (_controller.InvertedIndex.ContainsKey(term)) // case the term appearce in the inverted index
                {
                    //switch (_controller.InvertedIndex[term].TermDictionaryPointer) // check the location of the posting list
                    //{
                        //case TermDictionaryPointer.Cache: // case the posting line is in the cache, extract from the cacheDictionary
                        //    termPostingList = _controller.CacheDictionary[term].PostingList;
                        //    break;
                        //case TermDictionaryPointer.Posting:// case the posting line is in the posting file, extract from the file
                            termPostingList = GetPostingLineFromPostingFile(term, _controller.InvertedIndex[term].PointerLine);
                            //break;
                   // }
                    queryPostingsList.Add(term, termPostingList); // add the posting list to the dictionary
                }
            }
            return queryPostingsList;
        }
        /// <summary>
        /// Gets all the potential documents may match the query results 
        /// </summary>
        /// <param name="queryPostingList">All the posting list of the terms in the query</param>
        /// <returns>Dictionary of doc id and the document instance of all potential documents match the query</returns>
        private Dictionary<int, Document> GetPotentialDocs(Dictionary<string, TermPostingList> queryPostingList)
        {
            Dictionary<int, Document> potentialDocs = new Dictionary<int, Document>();
            foreach (var term in queryPostingList.Keys) //  for each term in the query 
            {
                foreach(var termPostingData in queryPostingList[term].PostingList) // adds all documents from posting list to the potentialDocs dictionary
                {
                    if (!potentialDocs.ContainsKey(termPostingData.DocumentId))
                    {
                        potentialDocs.Add(termPostingData.DocumentId, _controller.DocumentsDataList[termPostingData.DocumentId]);
                    }
                }
            }
            return potentialDocs;
        }
        /// <summary>
        /// Retreve the posting line from the posting file 
        /// </summary>
        /// <param name="term">The term which needed to get it's posting list</param>
        /// <param name="pointerLine">The line of the posting list in the posting file</param>
        /// <returns>the posting list of the term</returns>
        private TermPostingList GetPostingLineFromPostingFile(string term, int pointerLine)
        {
            string fileName = term[0].ToString();
            if (!char.IsLetter(term[0]))
                fileName = "special";
            string postingPath = _controller.LoadingAndSavingPath + "\\Posting\\" + fileName; // the posting path - according to the first letter of the term
            string postingLine = File.ReadLines(postingPath).Skip(pointerLine - 1).Take(1).First(); // take the posting list in the pointer line
            return new TermPostingList(postingLine, 7000); // return new instance of term posting list from posting line with only 50 most significant documents of the term
        }

        #region documentSearch

        public Dictionary<int, Tuple<string, float>> Find5SignificantSentencesInDocument(string docName)
        {
            Dictionary<int, Tuple<string, float>> fiveSignificantSentences = null;
            Document document = _controller.DocumentsDataList.FirstOrDefault(docKeyValue => docKeyValue.Value.DocName == docName).Value;
            string documentPath = _controller.LoadingAndSavingPath + "\\corpus\\" + document.FolderName + "\\" + document.FolderName;
            using (StreamReader sr = new StreamReader(documentPath))
            {
                string documentsInFolder = sr.ReadToEnd();
                string[] documentsInFolderSplit = documentsInFolder.Split(new string[] { "<DOC>" }, StringSplitOptions.RemoveEmptyEntries);
                string searchedDocument = documentsInFolderSplit[document.DocLocationAtFolder];
                string[] splitDoc = searchedDocument.Split(new string[] { "<TEXT>" }, StringSplitOptions.RemoveEmptyEntries);
                string searchedDocumentText = splitDoc[1].Split(new string[] { "</TEXT>" }, StringSplitOptions.RemoveEmptyEntries)[0];
                Dictionary<int, string> sentencesInText = SplitTextToSentences(searchedDocumentText);
                fiveSignificantSentences = IndexSentencesInDocument(sentencesInText, document.DocHeadLine);
            }
            return fiveSignificantSentences;
        }

        public Dictionary<int, string> SplitTextToSentences(string searchedDocumentText)
        {
            Dictionary<int,string> sentencesInText = new Dictionary<int, string>();
            string currentSentence = "";
            int sentenceIndex = 0;
            for (int i = 0; i < searchedDocumentText.Length; i++)
            {
                if (i + 1 < searchedDocumentText.Length - 1 && searchedDocumentText[i] == '.' &&
                    (searchedDocumentText[i + 1] == ' ' || searchedDocumentText[i + 1] == '\n'))
                {
                    currentSentence = Regex.Replace(currentSentence, @"\s+", " ");
                    sentencesInText.Add(sentenceIndex, currentSentence);
                    currentSentence = "";
                    sentenceIndex++;
                    continue;
                }
                currentSentence += searchedDocumentText[i];
            }
            return sentencesInText;
        }

        public Dictionary<int, Tuple<string, float>> IndexSentencesInDocument(Dictionary<int, string> sentencesInText, string docHeadLine)
        {
            Dictionary<int, Tuple<string, float>> sentencesRank = new Dictionary<int, Tuple<string, float>>();
            Dictionary<string, int> termsInDocumentWithTheirSentencesFrequency = new Dictionary<string, int>();
            Dictionary<int, Dictionary<string, int>> DictionaryOfSentencesTotermsInSentenceDictionary = new Dictionary<int, Dictionary<string, int>>();
            var maxTerm = new KeyValuePair<string, int>();
            Dictionary<string, int> termsInDocumentHeadLine = _controller.parser.ParseText(docHeadLine, out maxTerm);

            foreach (var sentenceKeyValue in sentencesInText) // for each sentence text in document 
            {
                // parse the sentence text into dictionary of terms and it's frequency and update the max term in the document
                maxTerm = new KeyValuePair<string, int>();
                Dictionary<string, int> termsInSentenceDictionary = _controller.parser.ParseText(sentenceKeyValue.Value, out maxTerm);
                // for each entry in the terms dictionary update the data in the temporal posting dictionary
                foreach (KeyValuePair<string, int> entery in termsInSentenceDictionary)
                {
                    if (!termsInDocumentWithTheirSentencesFrequency.ContainsKey(entery.Key)) // if the key not exist in the temporal posting dictionary create new instance
                        termsInDocumentWithTheirSentencesFrequency[entery.Key] = 1;

                    else
                        termsInDocumentWithTheirSentencesFrequency[entery.Key]++; // add the TF to total TF of the term in the termsTFs dictionary
                }
                DictionaryOfSentencesTotermsInSentenceDictionary.Add(sentenceKeyValue.Key, termsInSentenceDictionary);
            }

            foreach (var sentenceLocationAndTermsDictionary in DictionaryOfSentencesTotermsInSentenceDictionary)
            {
                float sentenceTFIdf = 0;
                float numberOfTermsAppearncesInDocumentHeadLine = 0;
                foreach (var termKeyValueInSentence in sentenceLocationAndTermsDictionary.Value)
                {
                    float Idf = (float)Math.Log((DictionaryOfSentencesTotermsInSentenceDictionary.Count /
                            termsInDocumentWithTheirSentencesFrequency[termKeyValueInSentence.Key]), 2);
                    float Tf = (float)termKeyValueInSentence.Value;
                    sentenceTFIdf += Idf * Tf;
                    if (termsInDocumentHeadLine.ContainsKey(termKeyValueInSentence.Key))
                        numberOfTermsAppearncesInDocumentHeadLine++;
                }
                float sentenceRank = (float)(0.7) * sentenceTFIdf + (float)(0.3) * numberOfTermsAppearncesInDocumentHeadLine;
                sentencesRank.Add(sentenceLocationAndTermsDictionary.Key, new Tuple<string, float> (sentencesInText[sentenceLocationAndTermsDictionary.Key], sentenceRank) );
            }
            return (sentencesRank.OrderByDescending(pair => pair.Value.Item2)).Take(5).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        #endregion
    }
}
