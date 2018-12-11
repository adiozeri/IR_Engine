using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_engine.PartA
{
    public class TermData
    {
        public int NumberOfDocumentsTheTermAppearsAt { get; set; }
        public int NumberOfRowInPostingFile { get; set; }

        public TermData()
        {
            NumberOfDocumentsTheTermAppearsAt = 0;
        }

    }
}
