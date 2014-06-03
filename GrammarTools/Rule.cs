using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    public class Rule
    {
        public NonTerminal LeftPart { get; private set; }

        public List<IToken> RightPart { get; private set; }

        public Rule(NonTerminal left, List<IToken> right)
        {
            LeftPart = left;

            if (LeftPart.Rules == null)
                LeftPart.Rules = new List<Rule>() { this };
            else
                LeftPart.Rules.Add(this);

            RightPart = right;
        }

        public bool ContainsOnlyTerminals()
        {
            return RightPart.Where(token => token.IsTerminal).Count() == RightPart.Count;
        }

        public bool ContainsOnlyNonTerminals()
        {
            return RightPart.Where(token => !token.IsTerminal).Count() == RightPart.Count;
        }

        public override string ToString()
        {
            return LeftPart.ToString() + " -> " + String.Concat(RightPart.Select(t => t.ToString() + " ")).Trim();
        } 
    }
}
