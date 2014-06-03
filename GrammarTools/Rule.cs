using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    class Rule
    {
        public NonTerminal LeftPart { get; private set; }

        public List<IToken> RightPart { get; private set; }

        public Rule(NonTerminal left, List<IToken> right)
        {
            LeftPart = left;
            LeftPart.Rule = this;
            RightPart = right;
        }
    }
}
