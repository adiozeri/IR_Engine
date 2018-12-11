using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    /// <summary>
    /// A line in the posting list, each line contain list of term posting data (for each document the term in it)
    /// </summary>
    [Serializable]
    public class TermPostingList
    {
        public List<TermPostingData> PostingList { get; set; }

        /// <summary>
        /// empty constructoer - init the posting list
        /// </summary>
        public TermPostingList()
        {
            PostingList = new List<TermPostingData>();
        }
        /// <summary>
        /// create new instance of the postingList from string line, we know the delimeter of the term from
        /// the data is '*' so first we split according it and then split the data part with '~' in order
        /// to ceate each instance of termPostingData
        /// </summary>
        /// <param name="line"></param>
        public TermPostingList(string line, int limit, bool ordered = true)
        {
            PostingList = new List<TermPostingData>();
            line = line.Split('*')[1];
            string[] termPostingDataList = line.Split('~');
            if (!ordered)
            {
                foreach (string termPostingData in termPostingDataList)
                    PostingList.Add(new TermPostingData(termPostingData));
            }
            else
            {
                foreach (string termPostingData in termPostingDataList)
                    PostingList.Add(new TermPostingData(termPostingData));
                PostingList = PostingList.OrderByDescending(t => t.TF).ToList();
                if (limit < PostingList.Count)
                    PostingList.RemoveRange(limit, PostingList.Count - limit - 1);
            }

        }
        internal string DisplayForCache()
        {
            string postingListString = "";
            foreach (TermPostingData termData in PostingList)
            {
                postingListString += termData.DisplayForCache() + " -> ";
            }
            postingListString = postingListString.Substring(0, postingListString.Length - 2);
            return postingListString;
        }
        /// <summary>
        /// add termPostingData to the end of the list
        /// </summary>
        /// <param name="termData"></param>
        public void AddToTermPostingList(TermPostingData termData)
        {
            PostingList.Add(termData);
        }
        /// <summary>
        /// ovveride the ToString method in order to use it when we write the posting line to file
        /// we concatenate the termData with '~' delimeter between
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string postingListString = "";
            foreach (TermPostingData termData in PostingList)
            {
                postingListString += termData.ToString() + "~";
            }
            postingListString = postingListString.Substring(0, postingListString.Length - 1);
            return postingListString;
        }

        public float GetTf(int docId)
        {
            float tf = 0;
            foreach (TermPostingData termData in PostingList)
            {
                if (termData.DocumentId == docId)
                {
                    tf = termData.TF;
                    break;
                }
            }
            return tf;
        }


    }
}
