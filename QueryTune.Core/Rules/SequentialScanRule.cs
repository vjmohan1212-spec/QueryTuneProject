namespace QueryTune.Core.Rules;

public class SequentialScanRule
{
    public bool IsMatch(string explainPlan)
    {
        return explainPlan.Contains("Seq Scan");
    }

    public string Issue()
    {
        return "Sequential Scan detected.";
    }

    public string Suggestion()
    {
        return "Consider creating an index on the filtered column.";
    }
}