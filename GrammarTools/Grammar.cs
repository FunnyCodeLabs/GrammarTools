using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GrammarTools
{
    public class Grammar
    {
        private Dictionary<string, NonTerminal> __NonTerminals = new Dictionary<string, NonTerminal>();
        private Dictionary<string, Terminal> __Terminals = new Dictionary<string, Terminal>();
        private List<Rule> __Rules = new List<Rule>();
        private readonly Terminal __Epsilon = new Terminal("e");

        private HashSet<NonTerminal> __EpsilonNonterminals = null;
        private HashSet<NonTerminal> __LeftRecursiveNonterminals = null;

        public Grammar()
        {
            __Terminals.Add("e", __Epsilon);
        }

        public HashSet<NonTerminal> EpsilonNonterminals
        {
            get
            {
                if (__EpsilonNonterminals == null)
                    FindEpsilonNonterminals();
                return __EpsilonNonterminals;
            }
        }

        #region Grammar construction

        private void AddRule(string line)
        {
            Regex rule = new Regex(@"([A-Z])\s+->\s+((([A-Z]|[a-z])\s*)+)");

            var match = rule.Match(line);

            if (!match.Success)
                throw new InvalidOperationException();

            var left = match.Groups[1].Value.Trim();
            var rightSequence = match.Groups[2].Value.Trim();

            NonTerminal leftNonTerminal;
            if (!AddNonTerminal(left, out leftNonTerminal))
                throw new ApplicationException();

            var tokenStrings = (new Regex(@"\s+")).Split(rightSequence).Select(x => x.Trim());

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

            Rule r = new Rule(leftNonTerminal, rightTokens);
            __Rules.Add(r);
        }

        private bool AddNonTerminal(string nonTermString, out NonTerminal nonTerminal)
        {
            string nonTermPattern = "[A-Z]+";
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
            string termPattern = "[a-z]+";

            terminal = null;

            if (!Regex.IsMatch(termString, termPattern))
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

        #endregion

        private bool DirectlyProduces(NonTerminal A, IEnumerable<IToken> alpha)
        {
            return A.Rules.Any(x => x.RightPart.SequenceEqual(alpha));
        }

        private IEnumerable<IToken> AllLeftFrom(Rule rule, IToken token)
        {
            return rule.RightPart.TakeWhile(x => x != token);
        }

        private IEnumerable<IToken> AllRightFrom(Rule rule, IToken token)
        {
            return rule.RightPart.SkipWhile(x => x != token).Skip(1);
        }

        private IEnumerable<IToken> GetSide(bool leftRecursive, Rule rule, NonTerminal nt)
        {
            IEnumerable<IToken> side;
            if (leftRecursive)
                side = AllLeftFrom(rule, nt);
            else
                side = AllRightFrom(rule, nt);
            return side;
        }

        private bool IsEpsilonProductive(IToken token)
        {
            return token.IsTerminal ? token == __Epsilon : (EpsilonNonterminals.Contains(token));
        }

        private IEnumerable<NonTerminal> GetNonterminalsFromRule(Rule rule)
        {
            return rule.RightPart.Where(x => !x.IsTerminal).Cast<NonTerminal>();
        }

        #region Public methods

        public IEnumerable<NonTerminal> FindEpsilonNonterminals()
        {
            if (__EpsilonNonterminals != null)
                return __EpsilonNonterminals;

            List<Terminal> e = new List<Terminal>() { __Epsilon };

            HashSet<NonTerminal> epsilonNonterminals = new HashSet<NonTerminal>();
            HashSet<NonTerminal> currentStage = new HashSet<NonTerminal>();

            //Добавляем все нетерминалы, которые напрямую продуцируют в epsilon
            foreach (var nonterm in __NonTerminals.Values)
            {
                if (DirectlyProduces(nonterm, e.Cast<IToken>()))
                    epsilonNonterminals.Add(nonterm);
            }

            //Последовательно конструируем множество е-нетерминалов
            //Добавляем только те нетерминалы, которые продуцирут в цепочку только из токенов прошлого шага
            do
            {
                currentStage.Clear();
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

                    if (match && !epsilonNonterminals.Contains(rule.LeftPart))
                        currentStage.Add(rule.LeftPart);
                }

                epsilonNonterminals.UnionWith(currentStage);
            }
            while (currentStage.Count != 0);

            return __EpsilonNonterminals = epsilonNonterminals;
        }

        public IEnumerable<NonTerminal> RecursiveNonterminals(bool leftRecursive = true)
        {
            if (__LeftRecursiveNonterminals != null)
                return __LeftRecursiveNonterminals;

            HashSet<NonTerminal> result = new HashSet<NonTerminal>();

            foreach (var nonterm in __NonTerminals.Values)
            {
                bool firstPass = true;
                HashSet<NonTerminal> previousStage = new HashSet<NonTerminal>() { nonterm };
                HashSet<NonTerminal> currentStage = new HashSet<NonTerminal>();

                //Добавляем в множество только нетерминалы вида wAv, где A из предыдущего шага, 
                //а w - посл. токенов, где все e-нетерминалы
                do
                {
                    currentStage.Clear();
                    foreach (var curNonTerm in previousStage)
                    {
                        foreach (var rule in curNonTerm.Rules)
                        {
                            foreach (var nt in GetNonterminalsFromRule(rule))
                            {
                                IEnumerable<IToken> side = GetSide(leftRecursive, rule, nt);

                                if (!previousStage.Contains(nt) && (side.Count() == 0 || side.All(x => IsEpsilonProductive(x))))
                                {
                                    currentStage.Add(nt);
                                }
                                //Покрыть случай, когда рекурсия непосредственная
                                else if (firstPass && (side.Count() == 0 || side.All(x => IsEpsilonProductive(x))))
                                {
                                    currentStage.Add(nt);
                                }
                            }
                        }
                    }

                    previousStage.UnionWith(currentStage);
                    if (firstPass && !currentStage.Contains(nonterm))
                    {
                        previousStage.Remove(nonterm);
                    }

                    firstPass = false;
                }
                while (currentStage.Count != 0 && !previousStage.Contains(nonterm));

                if (previousStage.Contains(nonterm))
                    result.Add(nonterm);
            }

            return result;
        }

        public static Grammar Create(string[] input)
        {
            Grammar grammar = new Grammar();
            foreach (var line in input)
                grammar.AddRule(line);

            return grammar;
        }

        #endregion
    }
}
