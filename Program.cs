using Nest;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using TelegramSink;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var elasticSettings = new ConnectionSettings(new Uri("http://localhost:9200"));
var elasticClient = new ElasticClient(elasticSettings);

builder.Services.AddSingleton<IElasticClient>(elasticClient);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

ConfigureLogging();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)
        .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
        .Build();
    
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()    
        .Enrich.WithExceptionDetails()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
        .WriteTo.Http("http://localhost:5000/logs", 100, period: TimeSpan.FromSeconds(2))
        .WriteTo.TeleSink("5843621363:AAFiycHoNHeFGUzGmKrczcOWJCd1b82hWJo", "-1001912603201")
        .CreateLogger();

    ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
    {
        var uriString = configuration.GetValue<string>("ElasticUri");

        return new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "test",
            NumberOfReplicas = 1,
            NumberOfShards = 2
        };
    }
}