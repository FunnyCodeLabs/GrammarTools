using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    class NonTerminal : IToken
    {
        public NonTerminal(string value)
        {
            Value = value;
        }
        
        public string Value { get; private set; }

        public bool IsTerminal
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
