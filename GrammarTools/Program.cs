﻿using System;
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
                "S -> B A",

                "A -> + B A",
                "A -> e",

                "B -> D C",

                "C -> * D C",
                "C -> e",

                "D -> ( S )",
                "D -> a",
            };

            Grammar g = Grammar.Create(grammar);
            //var a = g.Follow(g.NonTerminals[3]);
            Print(g.Follow());

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

        private static void Print(Dictionary<IToken, HashSet<IToken>> follow)
        {
            foreach (var item in follow)
            {
                Console.Write("Follow (" + item.Key + ")" + " = { ");
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
