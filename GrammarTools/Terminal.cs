using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    class Terminal : IToken
    {
        
        public Terminal(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public bool IsTerminal
        {
            get { return false; }
        }
    }
}
