using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    class Terminal : Token
    {
        public Terminal(string value)
            : base(value)
        { }

        public override bool IsTerminal
        {
            get { return true; }
        }
    }
}
