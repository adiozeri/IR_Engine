using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    class TermDocumentsData
    {
        List<TermInDocumentData> TermDocumentsDataList { get; set; }
        public TermDocumentsData()
        {
            TermDocumentsDataList = new List<TermInDocumentData>();
        }

        public void AddOccurenceDataOfTermToList(string docName, int numbeOccurence)
        {
            TermInDocumentData docData = TermDocumentsDataList.FirstOrDefault(d => d.DocumentName.Equals(docName));
            if (docData != null)
                docData.TermFrequentInDocument++;
            else
                TermDocumentsDataList.Add(new TermInDocumentData (docName));
        }

        public string TermDocumentsDataToString()
        {
            string termDataString = "";
            foreach(TermInDocumentData docTermData in TermDocumentsDataList)
            {
                termDataString = termDataString + docTermData.DataInDocumentToString() + '|';
            }
            return termDataString;
        }
    }
}
