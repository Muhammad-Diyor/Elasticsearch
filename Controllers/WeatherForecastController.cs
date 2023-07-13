using ElasticSearchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Serilog;

namespace ElasticSearchAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IElasticClient _elasticClient;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IElasticClient elasticClient)
    {
        _logger = logger;
        _elasticClient = elasticClient;
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        _logger.LogInformation("Hi, it's the get endpoint");
        var product = new Product { Id = Guid.NewGuid(), Name = "Sample Product" };
        // save data in pg
        var response = await _elasticClient.IndexAsync(product, idx => idx.Index("products"));
        if (response.IsValid)
        {
            Log.Information("Indexing completed successfully");
        }
        else
        {
            Log.Error("Indexing failed");
        }

        return Ok(response);
    }

    [HttpGet( "GetAllProducts")]
    public async Task<IActionResult> CheckElasticSearch()
    {
        _logger.LogInformation("Hi, it's the checkElastic endpoint");

        var elasticHealth = _elasticClient.Search<Product>(s =>
            s.Index("products"));
        return Ok(elasticHealth.Documents);
    }

    [HttpGet("GetCount")]
    public async Task<IActionResult> GetCount()
    {
        _logger.LogInformation("Hi, it's the checkElastic endpoint");

        var countOfEntries = (await _elasticClient.CountAsync<Product>(s =>
            s.Index("products"))).Count;
        return Ok(countOfEntries);
    }

    [HttpPost]
    public async Task<IActionResult> PostProduct(string name, int quantity, int loopTimes)
    {
        for (int i = 0; i < loopTimes; i++)
        {
            var product = new Product { Id = Guid.NewGuid(), Name = name, Quantity = quantity, };
            await _elasticClient.IndexAsync(product, indexDescriptor => indexDescriptor.Index("products"));
        }
        
        return Ok();
    }
}
