using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfToCsv
{
    internal abstract class AbstractMatchNode
    {
        private readonly string _label;
        protected string Value;

        protected AbstractMatchNode(string label)
        {
            _label = label;
        }

        public MatchType Type;
        public abstract List<ScoreResult> GetScores(string[] input);

        public string GetLabel()
        {
            return _label;
        }

        public string GetValue()
        {
            return Value;
        }
    }

    enum MatchType
    {
        EXACT_STRING,
        REGEX,
        OR,
        FUZZY_STRING,
        LIST
    }

    class RegexNode : AbstractMatchNode
    {
        private readonly Regex _pattern;
        private readonly AbstractMatchNode _next;
        private readonly double _score;

        public RegexNode(string label, double score, Regex pattern, AbstractMatchNode next): base(label)
        {
            _pattern = pattern;
            _next = next;
            _score = score;
        }

        public override List<ScoreResult> GetScores(string[] input)
        {
            List<ScoreResult> scoreResults = new List<ScoreResult>();
            if (_next != null)
            {
                scoreResults = _next.GetScores(input.Skip(1).ToArray());
            }
            double score;
            if (_pattern.IsMatch(input[0]))
            {
                score = 1;
                Match match = _pattern.Match(input[0]);
                Value = match.Value;
            }
            else
            {
                score = 0;
                Value = "";
            }
            foreach (ScoreResult result in scoreResults)
            {
                result.Score += score * _score;
                result.Nodes.Add(this);
            }
            return scoreResults;
        }
    }

    class OrNode : AbstractMatchNode
    {
        private readonly AbstractMatchNode[] _nodes;
        public readonly MatchType MatchType = MatchType.OR;

        public OrNode(string label, params AbstractMatchNode[] nodes) : base(label)
        {
            _nodes = nodes;
        }

        public override List<ScoreResult> GetScores(string[] inputs)
        {
            List<ScoreResult> scoreResults = new List<ScoreResult>();
            foreach (AbstractMatchNode node in _nodes)
            {
                scoreResults.AddRange(node.GetScores(inputs));
            }
            return scoreResults;
        }
    }

    class ScoreResult
    {
        public List<AbstractMatchNode> Nodes;
        public double Score;
    }

    //class FuzzyStringNode : AbstractMatchNode
    //{
    //    public override MatchType GetMatchType()
    //    {
    //        return MatchType.FUZZY_STRING;
    //    }

    //    public override double GetScore(string[] input)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
