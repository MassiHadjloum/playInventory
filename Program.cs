using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>()!;

builder.Services.AddMongo().AddMongoRepository<InventoryItem>("inventoryItems");

// add policy to specify timeout duration
builder.Services.AddHttpClient<CatalogClinet>(client => {
    client.BaseAddress = new Uri("http://localhost:5046");
})
.AddTransientHttpErrorPolicy(build => 
    build.Or<TimeoutRejectedException>().WaitAndRetryAsync(5, 
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryAttempt) => {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClinet>>()
            ?.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
        }
    )
)
.AddTransientHttpErrorPolicy(build => 
    build.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3, 
        TimeSpan.FromSeconds(15), 
        onBreak: (outcome, timespan) => {
            var serviceProvider = builder.Services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClinet>>()
                ?.LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds ...");
        },
        onReset: () => {
            var serviceProvider = builder.Services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClinet>>()
                ?.LogWarning($"Closing the circuit ...");
        }
    ))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();