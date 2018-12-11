using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IR_engine.PartA;

namespace IR_engine.PartB
{
    /// <summary>
    /// Ranks the given documents according to given query
    /// </summary>
    public class Ranker
    {
        IRController _controller;
        double Wiq = 0.0;
        double SumOfPowersWiq = 0.0; // sum of all Wiq^2 - part of the cosine similarity denominator, in our case the length of the qaury
        Dictionary<string, double> queryWiq = new Dictionary<string, double>();

        /// <summary>
        /// Create new instance of ranker class that rank the relvent documents according to given query
        /// </summary>
        /// <param name="controller">The controller of the engine include all the data require for ranking the relevent documents</param>
        public Ranker(IRController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// This function gives rank to each document in the potenitalDocs according to given query - 
        /// the rank calculate by cosine similarity measure and the appreance of the term in the query header
        /// </summary>
        /// <param name="potentialDocs">The documents needed to be ranked</param>
        /// <param name="queryPostingsList">All the posting list of the terms in the query</param>
        /// <returns>Dictionary of document and it's rank</returns>
        public Dictionary<Document, double> rankDocumentsForQuery(Dictionary<int, Document> potentialDocs, Dictionary<string, TermPostingList> queryPostingsList, string query)
        {
            Dictionary<Document, double> rankedDocuments = new Dictionary<Document, double>();
            int i = 0;
            queryWiq.Clear();
            SumOfPowersWiq = 0;
            foreach (var term in queryPostingsList.Keys)
            {
                Wiq = query.Split(new string[] { term }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                queryWiq.Add(term, Wiq);
                SumOfPowersWiq += Math.Pow(Wiq, 2);
            }
            //  Wiq = query.Split(new string[] { term }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
            foreach (var docId in potentialDocs.Keys) // calculate the rank for each document
            {
                i++;
 
                double cosSimRank = CosSim(docId, potentialDocs[docId], queryPostingsList, query);
                double headLineRank = HeadLineRank(potentialDocs[docId], queryPostingsList.Keys.ToList());
                double BM25Rank = BM25(docId, potentialDocs[docId], queryPostingsList, query);

                    rankedDocuments.Add(potentialDocs[docId], 0.001 * cosSimRank + 0.099 * headLineRank + 0.9 * BM25Rank);

            }
            return rankedDocuments;
        }
        /// <summary>
        /// Calculate the cosine similarity of the given document with the given query
        /// </summary>
        /// <param name="docId">The id of the document which the rank is caclculate for</param>
        /// <param name="potentialDoc">The doc which the rank is calculate for</param>
        /// <param name="queryPostingsList">All the posting list of the terms in the query </param>
        /// <returns>The cosine similarity rank</returns>
        public double CosSim(int docId, Document potentialDoc, Dictionary<string, TermPostingList> queryPostingsList, string query)
        {
            double Wij; // the weight of term i in doc j - in our case in potentialDoc
            double Wiq; // the weight of term i in query q - in our case in queryPostingsList
            double innerProduct = 0.0; // the numerator of the cosine similarity rank - sum of all Wij*Wiq
     


            foreach (var term in queryPostingsList.Keys) // for each term in the query calculate Wij and update the innerProduct and SumOfPowersWij
            {
                if (queryPostingsList[term] != null)
                {
                    // the Wij is the TF of the term in document, we extract it and normelized by the number of instances of the most frequent term
                    Wij = ((double)queryPostingsList[term].GetTf(docId) / potentialDoc.DocLength) *
                        ((double)_controller.InvertedIndex[term].Idf);
                    Wiq = queryWiq[term];
                    innerProduct += Wij * Wiq;
                  //  SumOfPowersWiq += Math.Pow(Wiq, 2);
                }
            }
            return innerProduct / Math.Pow(((potentialDoc.TotalSquaredTfIdf) / (Math.Pow(potentialDoc.DocLength, 2))) * SumOfPowersWiq, 0.5);
        }

        /// <summary>
        /// Calculate the BM25 measure of the given document with the given query
        /// </summary>
        /// <param name="docId">The id of the document which the rank is caclculate for</param>
        /// <param name="potentialDoc">The doc which the rank is calculate for</param>
        /// <param name="queryPostingsList">All the posting list of the terms in the query </param>
        /// <returns>The cosine similarity rank</returns>
        public double BM25(int docId, Document potentialDoc, Dictionary<string, TermPostingList> queryPostingsList, string query)
        {
            double ni;
            double firstPart;
            double N = _controller.DocumentsDataList.Count;
            double k1 = 1.2;
            double b = 0.75;
            double k2 = 0;
            double averageDocumentLength = 442.2524;
            double K = k1 * ((1 - b) + b * (potentialDoc.DocLength / averageDocumentLength));
            double secondPart;
            double fi;
            double qfi;
            double thirdPart;

            double BM25Result = 0.0;

            foreach (var term in queryPostingsList.Keys)
            {
                ni = _controller.InvertedIndex[term].DocumentsFrequency;
                firstPart = Math.Log((1) / ((ni + 0.5) / (N - ni + 0.5)));

                fi = queryPostingsList[term].GetTf(docId);
                secondPart = ((k1 + 1) * fi) / (K + fi);
                qfi = queryWiq[term];

                thirdPart = ((k2 + 1) * qfi) / (k2 + qfi);
              //  BM25Result += firstPart * secondPart;
                BM25Result += firstPart * secondPart * thirdPart;
            }
            return BM25Result;
        }

        /// <summary>
        /// Calculate the head line rank of the given document with the given query - number of terms in the query appears in the headline - normalized according the query length
        /// </summary>
        /// <param name="potentialDoc">The doc which the rank is calculate for</param>
        /// <param name="termsInQuery">List of all terms in the query</param>
        /// <returns>The head line rank</returns>
        public double HeadLineRank(Document potentialDoc, List<string> termsInQuery)
        {
            double numberOfQueryTermsInHeadLine = 0.0; // counter of the terms in the query which appears in the doc header
            KeyValuePair<string, int> maxTerm;
            Dictionary<string, int> parsedHeadLine = _controller.parser.ParseText(potentialDoc.DocHeadLine, out maxTerm); // parse the title for comparing with the query
            foreach (var term in termsInQuery) // for each term in query check if appers in the header
            {
                if (parsedHeadLine.ContainsKey(term)) // case the term appear increase the counter
                    numberOfQueryTermsInHeadLine++;
            }
            int headLineCount = potentialDoc.DocHeadLine.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return numberOfQueryTermsInHeadLine / (double)headLineCount;
        }
    }
}
