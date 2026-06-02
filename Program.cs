using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpiderStock.Application;
using SpiderStock.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// 記憶體快取（供 SectorService 使用）
builder.Services.AddMemoryCache();

// Alltick API（既有功能）
builder.Services.AddHttpClient<IStockApiClient, AlltickApiClient>();
builder.Services.AddSingleton<IQuoteService, QuoteService>();
builder.Services.AddHostedService<QuoteService>();

// 玉山 eSun API + 產業 Dashboard
builder.Services.AddHttpClient<IEsunApiClient, EsunApiClient>();
builder.Services.AddScoped<ISectorService, SectorService>();

var app = builder.Build();

// 靜態檔案（wwwroot/index.html → 產業 Dashboard）
app.UseDefaultFiles();
app.UseStaticFiles();

// ── 產業 Dashboard API ──────────────────────────────────────────────
var sectorApi = app.MapGroup("/api/sector");

// GET /api/sector/summaries → 全產業漲跌彙總
sectorApi.MapGet("/summaries", async (ISectorService svc) =>
    Results.Ok(await svc.GetSectorSummariesAsync()));

// GET /api/sector/stocks/{industryCode} → 特定產業下各股交易量
sectorApi.MapGet("/stocks/{industryCode}", async (string industryCode, ISectorService svc) =>
    Results.Ok(await svc.GetStocksInSectorAsync(industryCode)));

// ── 既有 TODO 範例 API（保留）──────────────────────────────────────
var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

await app.RunAsync();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
