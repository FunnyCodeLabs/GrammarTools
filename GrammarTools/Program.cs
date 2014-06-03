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
            string[] grammar = new string[] { 
                "S -> a B D", 
                "S -> a D", 
                "S -> A C", 
                "S -> b",
                "A -> S C B",
                "A -> S A B C",
                "A -> C b D",
                "A -> e",
                "B -> C A", 
                "B -> d", 
                "C -> A D C", 
                "C -> a",
                "C -> e",
                "D -> E a C", 
                "D -> S C", 
                "E -> B C S", 
                "E -> a", 
            };
            Grammar g = Grammar.Create(grammar);

            var a = g.FindEpsilonNonterminals();
            foreach (var item in a)
            {
                Console.WriteLine(item);
            }
        }
    }
}
