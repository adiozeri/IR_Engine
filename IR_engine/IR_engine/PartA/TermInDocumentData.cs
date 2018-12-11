using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    class TermInDocumentData
    {
        public string DocumentName { get; set; }
        public int TermFrequentInDocument { get; set; }

        public TermInDocumentData(string documentName)
        {
            DocumentName = documentName;
            TermFrequentInDocument = 1;
        }
        public string DataInDocumentToString()
        {
            return DocumentName + ',' + TermFrequentInDocument;
        }
    }
}
