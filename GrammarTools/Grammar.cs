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
        private readonly Terminal __Epsilon = new Terminal("e");
        private List<NonTerminal> __EpsilonNonterminals = null;

        public Grammar()
        {
            __Terminals.Add("e", __Epsilon);
        }

        private void AddRule(string line)
        {
            string[] rightLeft = line.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            if (rightLeft.Length < 2)
                throw new ApplicationException("Error parsing: [" + line + "]");


            string left = rightLeft[0].Trim();
            NonTerminal leftNonTerminal;
            if (!AddNonTerminal(left, out leftNonTerminal))
                throw new ApplicationException();

            string right = rightLeft[1].Trim();
            string[] tokenStrings = right.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            List<IToken> rightTokens = new List<IToken>();
            foreach (var tokenStr in tokenStrings)
            {
                NonTerminal nonTerm;
                if (!AddNonTerminal(tokenStr, out nonTerm))
                {
                    Terminal term;
                    if (AddTerminal(tokenStr, out term))
                        rightTokens.Add(term);
                    else
                        throw new ApplicationException();
                }
                else
                    rightTokens.Add(nonTerm);
            }

            Rule rule = new Rule(leftNonTerminal, rightTokens);
            __Rules.Add(rule);
        }

        private bool AddNonTerminal(string nonTermString, out NonTerminal nonTerminal)
        {
            string nonTermPattern = "[A-Z]";

            nonTerminal = null;
            if (!Regex.IsMatch(nonTermString, nonTermPattern))
                return false;

            if (__NonTerminals.ContainsKey(nonTermString))
                nonTerminal = __NonTerminals[nonTermString];
            else
            {
                nonTerminal = new NonTerminal(nonTermString);
                __NonTerminals.Add(nonTermString, nonTerminal);
            }

            return true;
        }

        private bool AddTerminal(string termString, out Terminal terminal)
        {
            string termPattern = "[a-z|+|-|*|/|(|)]";

            terminal = null;

            if (!Regex.IsMatch(termPattern, termString))
                return false;

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

        public IEnumerable<NonTerminal> FindEpsilonNonterminals()
        {
            if (__EpsilonNonterminals != null)
                return __EpsilonNonterminals;

            List<Terminal> epsilonRightPart = new List<Terminal>() { __Epsilon };

            HashSet<NonTerminal> epsilonNonterminals = new HashSet<NonTerminal>();
            HashSet<NonTerminal> currentStage = new HashSet<NonTerminal>();

            //Добавляем все нетерминалы, которые напрямую продуцируют в epsilon
            foreach (var nonterm in __NonTerminals.Values)
            {
                if (DirectlyProduces(nonterm, epsilonRightPart.Cast<IToken>()))
                    epsilonNonterminals.Add(nonterm);
            }

            //Последовательно конструируем множество е-нетерминалов
            //Добавляем только те нетерминалы, которые продуцирут в цепочку только из токенов прошлого шага
            do
            {
                foreach (var rule in __Rules)
                {
                    bool match = true;
                    foreach (var token in rule.RightPart)
                    {
                        if (!epsilonNonterminals.Contains(token))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        currentStage.Add(rule.LeftPart);
                }

                epsilonNonterminals.UnionWith(currentStage);
            }
            while (currentStage.Count != 0);

            return epsilonNonterminals;
        }

        private bool DirectlyProduces(NonTerminal A, IEnumerable<IToken> alpha)
        {
            return A.Rules.Any(x => x.RightPart.SequenceEqual(alpha));
        }
    }
}
