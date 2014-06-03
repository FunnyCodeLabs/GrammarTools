using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrammarTools
{
    public class TokenSequence : List<IToken>
    {
        public TokenSequence(IEnumerable<IToken> tokenCollection)
            :base(tokenCollection)
        {
        }

        public TokenSequence()
            :base()
        {
        }

        public override string ToString()
        {
            string s = String.Concat(this.Select(t => t.Value));
            return s;
        }

        public override bool Equals(object obj)
        {
            TokenSequence t = obj as TokenSequence;
            if (t == null)
                return false;

            if (Count != t.Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (!this[i].Equals(t[i]))
                    return false;

            return true;
        }
    }
}
