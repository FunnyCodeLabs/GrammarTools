using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    public class NonTerminal : Token
    {
        public NonTerminal(string value)
            : base(value)
        { }
        
        public override bool IsTerminal
        {
            get { return false; }
        }

        public List<Rule> Rules
        {
            get;
            set;
        }
    }
}
