using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarTools
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] grammar = new string[] { "S -> B C", "S -> e", "B -> D A", "A -> a" };
            Grammar g = Grammar.Create(grammar);
        }
    }
}
