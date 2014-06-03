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
            if (rightLeft.Length < 1)
                throw new ApplicationException("Error parsing: [" + line + "]");


            string right = rightLeft[0].Trim();
            if (AddNonTerminal(right))
                throw new ApplicationException();

            if (rightLeft.Length == 1)
            {

            }
            else
            {
                string left = rightLeft[1].Trim();
                string[] tokenStrings = left.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var tokenStr in tokenStrings)
                {
                    IToken token;
                    if (!AddNonTerminal(tokenStr))
                        AddTerminal(tokenStr);

                }
            }
        }

        private bool AddNonTerminal(string nonTermString)
        {
            string nonTermPattern = "[A-Z]";

            if (!Regex.IsMatch(nonTermString, nonTermPattern))
                return false;

            NonTerminal nonTerminal;
            if (__NonTerminals.ContainsKey(nonTermString))
                nonTerminal = __NonTerminals[nonTermString];
            else
            {
                nonTerminal = new NonTerminal(nonTermString);
                __NonTerminals.Add(nonTermString, nonTerminal);
            }

            return true;
        }

        private bool AddTerminal(string termString)
        {
            string termPattern = "[a-z|+|-|*|/|(|)]";

            if (!Regex.IsMatch(termPattern, termString))
                return false;

            Terminal terminal;
            if (__Terminals.ContainsKey(termString))
                terminal = __Terminals[termString];
            else
            {
                terminal = new Terminal(termString);
                __Terminals.Add(termString, terminal);
            }

            return true;
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
