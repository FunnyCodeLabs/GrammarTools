using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GrammarTools
{
    class Grammar
    {
        private Dictionary<string, NonTerminal> __NonTerminals = new Dictionary<string, NonTerminal>();
        private Dictionary<string, Terminal> __Terminals = new Dictionary<string, Terminal>();
        private List<Rule> __Rules = new List<Rule>();

        public Grammar()
        {

        }


        private void AddRule(string line)
        {
            string[] rightLeft = line.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            if (rightLeft.Length != 2)
                throw new ApplicationException("Error parsing: [" + line + "]");
            
            string termPattern = "[a-z|+|-|*|/|(|)]";
            string nonTermPattern = "[A-Z]";

            string right = rightLeft[0].Trim();
            if (!Regex.IsMatch(right, nonTermPattern)
                throw new ApplicationException();

            NonTerminal nonTerminal;
            if (__NonTerminals.ContainsKey(right))
                nonTerminal = __NonTerminals[right];
            else
            {
                nonTerminal = new NonTerminal(right);
                __NonTerminals.Add(right, nonTerminal);
            }

            string left = rightLeft[1].Trim();
            string[] tokenStrings = left.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var tokenStr in tokenStrings)
            {
                IToken token;
                if (__NonTerminals.ContainsKey(right))
                    nonTerminal = __NonTerminals[right];
                else
                {
                    nonTerminal = new NonTerminal(right);
                    __NonTerminals.Add(right, nonTerminal);
                }
                
            }
        }

        public static Grammar Create(string[] input)
        {
            Grammar grammar = new Grammar();
            foreach (var line in input)
                grammar.AddRule(line);
            


            return grammar;
        }


    }
}
