
using QueryTune.Api.Models;
using QueryTune.Core.Rules;
namespace QueryTune.Api.Services
{
    public class QueryAnalyzerService
    {
        private readonly SequentialScanRule _rule = new();

        public QueryAnalysisResult Analyze(string plan)
        {
            var result = new QueryAnalysisResult
            {
                ExplainPlan = plan,
                Score = 100
            };

            if (_rule.IsMatch(plan))
            {
                result.Issues.Add(_rule.Issue());
                result.Suggestions.Add(_rule.Suggestion());
                result.Score -= 30;
            }

            return result;
        }
    }
}
