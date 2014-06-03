﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrammarTools
{
    class Token : IToken
    {
        private string __Value;

        public Token(string value)
        {

        }

        public string Value
        {
            get
            {
                return __Value;
            }
        }

        public bool IsTerminal
        {
            abstract get;
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