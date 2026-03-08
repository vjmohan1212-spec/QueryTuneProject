namespace QueryTune.Api.Models
{
    public class IndexInfo
    {
        public string IndexName { get; set; }

        public List<string> Columns { get; set; } = new();
    }
}
