using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    class NonTerminal : Token
    {
        public NonTerminal(string value)
            : base(value)
        { }
        
        public override bool IsTerminal
        {
            get { return true; }
        }

        public Rule Rule
        {
            get;
            set;
        }
    }
}
