using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrammarTools
{
    public abstract class Token : IToken
    {
        private string __Value;

        public Token(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            __Value = value;
        }

        public string Value
        {
            get { return __Value; }
        }

        public abstract bool IsTerminal
        {
            get;
        }

        public override bool Equals(object obj)
        {
            Token t = obj as Token;
            if (t == null)
                return false;

            return __Value.Equals(t.__Value);
        }
    }
}
