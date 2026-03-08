namespace QueryTune.Api.Models;

public class QueryAnalysisResult
{
    public int Score { get; set; }

    public List<string> Issues { get; set; } = new();

    public List<string> Suggestions { get; set; } = new();

    public string ExplainPlan { get; set; }
}