using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    /// <summary>
    /// the type of the list the pointer number leads to 
    /// </summary>
    public enum TermDictionaryPointer
    {
        Cache,
        Posting
    }
    /// <summary>
    /// Data of the term in the inverted index dictionary - contain the # of documents the term appears in, the pointer line,
    /// the type of the list the pointer leads to, the idf of the term
    /// </summary>
    [Serializable]
    public class TermDataInMap
    {
        public int DocumentsFrequency { get; set;}
        public int PointerLine { get; set; }
        TermDictionaryPointer termDictionaryPointer = TermDictionaryPointer.Posting;
        public TermDictionaryPointer TermDictionaryPointer { get { return termDictionaryPointer; }  set { termDictionaryPointer = value; } }
        public float Idf { get; set; }
        public int TotalTF { get; set; }

        public TermDataInMap() { }

        /// <summary>
        /// build new instance of term data
        /// </summary>
        /// <param name="docFrequnecy"> the number of documents the term appears in</param>
        /// <param name="numberOfLineInPostingFile">the # of line in the chache/posting list </param>
        /// <param name="TotalOfDocs">the number of total docs for creating the idf</param>
        public TermDataInMap(int docFrequnecy, int numberOfLineInPostingFile, int TotalOfDocs, int totalTF)
        {
            DocumentsFrequency = docFrequnecy;
            PointerLine = numberOfLineInPostingFile;
            Idf =(float) Math.Log((TotalOfDocs/DocumentsFrequency), 2);
            TotalTF = totalTF;
        }

        internal void AddToTotalTF(int value)
        {
            TotalTF += value;
        }
    }
}
