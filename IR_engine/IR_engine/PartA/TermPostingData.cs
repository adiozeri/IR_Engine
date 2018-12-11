using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    /// <summary>
    /// the data of term in the posting/cache list, contain the document the term is in,
    /// frequent of the term in this document, the TF of the term in the document 
    /// </summary>
    [Serializable]
    public class TermPostingData
    {
        public int DocumentId { get; set; }
        public float TF { get; }

        /// <summary>
        /// build new instance of term data in the posting/cache list
        /// </summary>
        /// <param name="frequency">number of instances of the term in the document</param>
        /// <param name="documentId">the document Id</param>
        public TermPostingData(int frequency, int documentId)
        {
            DocumentId = documentId;
            TF = (float)frequency; /// (float)maxWordFreq);
        }
        /// <summary>
        /// build new instance of the term data in posting line from the line in the posting/chace list by split 
        /// </summary>
        /// <param name="line">the line contain the data of the term in the document</param>
        public TermPostingData(string line)
        {
            string [] lineSplit = line.Split(',');
            int docId; float tf;

            if (float.TryParse(lineSplit[1], out tf))
                TF = tf;
            if (int.TryParse(lineSplit[0], out docId))
                DocumentId = docId;

        }

        internal string DisplayForCache()
        {
            return "{ DocID =  " + DocumentId.ToString() + ", TF = " + TF.ToString()+" }";
        }

        /// <summary>
        /// override the ToString method in order to be able to use it when we write the posting data to file
        /// ',' is the delimeter of the fields and the order should be: 1)doc ID 2)Wordfrequency 3)TF
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DocumentId.ToString() + "," + TF.ToString();
        }
    }
}
