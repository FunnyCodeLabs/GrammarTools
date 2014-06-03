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
                "S -> A b S", 
                "S -> A C", 

                "A -> B D",

                "B -> B C", 
                "B -> e", 

                "C -> S a", 
                "C -> e",

                "D -> a B", 
                "D -> B A", 
            };
            Grammar g = Grammar.Create(grammar);

            var b = g.FindEpsilonNonterminals();
            foreach (var item in b)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine();

            var a = g.RecursiveNonterminals(true);
            foreach (var item in a)
            {
                Console.WriteLine(item);
            }
        }
    }
}
