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
                //"S -> A B",
                //"S -> s",
                //"A -> a B",
                //"A -> e",
                //"B -> B b"
                //"S → aS1"
                "S -> a F",
                "F -> A b B F",
                "F -> e",
                "A -> a D",
                "A -> e",
                "D -> b",
                "D -> a",
                "B -> c",
                "B -> e"
            };

            Grammar g = Grammar.Create(grammar);
            PrintFirst(g.First(1));
            //var b = g.FindEpsilonNonterminals();
            //foreach (var item in b)
            //{
            //    Console.WriteLine(item);
            //}

            //Console.WriteLine();

            //var a = g.RecursiveNonterminals(true);
            //foreach (var item in a)
            //{
            //    Console.WriteLine(item);
            //}
        }

        private static void PrintFirst(Dictionary<IToken, List<TokenSequence>> first)
        {
            foreach (var item in first)
            {
                Console.Write("First (" + item.Key + ")" + " = { ");
                foreach (var i in item.Value)
                {
                    Console.Write(i.ToString());
                    Console.Write(" ");
                }

                Console.Write("}");
                Console.WriteLine();
            }

        }
    }
}
