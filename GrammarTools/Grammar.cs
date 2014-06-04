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

        private Rule __StartRule;

        private HashSet<NonTerminal> __EpsilonNonterminals = null;
        private HashSet<NonTerminal> __LeftRecursiveNonterminals = null;
        private HashSet<NonTerminal> __RightRecursiveNonterminals = null;

        private static string __TerminalPattern = "[a-z+*()]+";
        private static string __NonTerminalPattern = "[A-Z]+";
        private static readonly IToken __Epsilon = new Terminal("e");
        private static readonly IToken __EOF = new Terminal("$");

        public Grammar()
        {
            __Terminals.Add("e", (Terminal) __Epsilon);
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

        public HashSet<NonTerminal> LeftRecursiveNonterminals
        {
            get
            {
                if (__LeftRecursiveNonterminals == null)
                    RecursiveNonterminals();
                return __LeftRecursiveNonterminals;
            }
        }

        public HashSet<NonTerminal> RightRecursiveNonterminals
        {
            get
            {
                if (__RightRecursiveNonterminals == null)
                    RecursiveNonterminals(false);
                return __RightRecursiveNonterminals;
            }
        }

        #region Grammar construction

        private void AddRule(string line)
        {
            Regex rule = new Regex(String.Format(@"({0})\s+->\s+((({0}|{1})\s*)+)", __NonTerminalPattern, __TerminalPattern));

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
            if (__StartRule == null)
                __StartRule = r;
            __Rules.Add(r);
        }

        private bool AddNonTerminal(string nonTermString, out NonTerminal nonTerminal)
        {
            nonTerminal = null;
            if (!Regex.IsMatch(nonTermString, __NonTerminalPattern))
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
            terminal = null;

            if (!Regex.IsMatch(termString, __TerminalPattern))
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

            List<Terminal> e = new List<Terminal>() { (Terminal)__Epsilon };

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
            if (leftRecursive)
            {
                if (__LeftRecursiveNonterminals != null)
                    return __LeftRecursiveNonterminals;
            }
            else
            {
                if (__RightRecursiveNonterminals != null)
                    return __RightRecursiveNonterminals;
            }

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

            if (leftRecursive)
                __LeftRecursiveNonterminals = result;
            else
                __RightRecursiveNonterminals = result;

            return result;
        }

        private HashSet<IToken> First(IToken token)
        {
            if (token.IsTerminal)
                return new HashSet<IToken>() { token };

            NonTerminal nonterm = token as NonTerminal;

            if (LeftRecursiveNonterminals.Contains(nonterm))
                throw new InvalidOperationException("Nonterminal can't be left recursive!");

            HashSet<IToken> firstSet = new HashSet<IToken>();
            HashSet<IToken> eps = new HashSet<IToken>(){__Epsilon};
            bool containsEps = true;

            foreach (var rule in nonterm.Rules)
            {
                containsEps = true;
                foreach (var t in rule.RightPart)
                {
                    if (containsEps)
                    {
                        var first_yi = First(t);
                        containsEps = first_yi.Contains(__Epsilon);
                        first_yi.ExceptWith(eps);
                        firstSet.UnionWith(first_yi);
                    }
                    else
                        break;
                }
            }

            if (containsEps || EpsilonNonterminals.Contains(nonterm))
                firstSet.Add(__Epsilon);

            return firstSet;
        }

        private HashSet<IToken> First(IEnumerable<IToken> sequence)
        {
            HashSet<IToken> firstSet = new HashSet<IToken>();
            HashSet<IToken> eps = new HashSet<IToken>(){__Epsilon};
            bool containsEps = true;

            List<HashSet<IToken>> firsts = new List<HashSet<IToken>>();

            foreach (var token in sequence)
            {
                if (containsEps)
                {
                    var first_i = First(token);
                    firsts.Add(first_i);
                    containsEps = first_i.Contains(__Epsilon);
                    firstSet.UnionWith(first_i);
                }
                else
                    break;
            }

            if (firsts.Count == sequence.Count() && firsts.All(x => x.Contains(__Epsilon)))
                firstSet.Add(__Epsilon);

            return firstSet;
        }

        private HashSet<IToken> Follow(NonTerminal nonterm)
        {
            HashSet<IToken> followSet = new HashSet<IToken>();

            if (__StartRule.LeftPart == nonterm)
                followSet.Add(__EOF);

            foreach (var rule in __Rules)
            {
                if (rule.RightPart.Contains(nonterm))
                {
                    var right = AllRightFrom(rule, nonterm);
                    if (right.Count() == 0)
                        followSet.UnionWith(Follow(rule.LeftPart));
                    else
                    {
                        var firstRight = First(right);
                        followSet.UnionWith(firstRight);

                        if (firstRight.Contains(__Epsilon))
                            followSet.UnionWith(Follow(rule.LeftPart));
                    }
                }
            }

            throw new NotImplementedException();
        }

        public Dictionary<IToken, HashSet<IToken>> First()
        {
            Dictionary<IToken, HashSet<IToken>> res = new Dictionary<IToken, HashSet<IToken>>();
            foreach (var item in __NonTerminals.Values)
                res.Add(item, First(item));

            return res;
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
