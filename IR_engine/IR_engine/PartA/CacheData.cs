using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    /// <summary>
    /// Cache Data Class - contain all data object in the cache dictionary have ( represent a line in the cache dictionary )
    /// </summary>
    [Serializable]
    public class CacheData
    {
        public TermPostingList PostingList { get; set; } // the posting list of the term
        public int PostingPointer { get; set; } // the line in the posting file

        /// <summary>
        /// build new Chace data from the final posting line
        /// </summary>
        /// <param name="line">posting line</param>
        /// <param name="pointer">number of row in the posting file</param>
        public CacheData(string line, int pointer,int limit )
        {
            PostingList = new TermPostingList(line,limit);
            PostingPointer = pointer;
        }
        public string DisplayForCache()
        {
            return PostingList.DisplayForCache();
        }
    }
}
