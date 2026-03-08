using Microsoft.AspNetCore.Mvc;
using QueryTune.Api.Models;
using QueryTune.Api.Services;
namespace QueryTune.Api.Controllers;

[ApiController]
[Route("api/query")]
public class QueryAnalysisController : ControllerBase
{
    private readonly ExplainPlanService _explainService;
    private readonly QueryAnalyzerService _analyzer;

    public QueryAnalysisController(
        ExplainPlanService explainService,
        QueryAnalyzerService analyzer)
    {
        _explainService = explainService;
        _analyzer = analyzer;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] QueryRequest request)
    {
        var result = await _explainService.AnalyzeQuery(request.Query);

        return Ok(result);
    }
}
