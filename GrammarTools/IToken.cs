using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    public interface IToken
    {
        bool IsTerminal { get; }
        string Value { get; }
    }
}
