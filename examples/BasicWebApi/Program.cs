using EasyHealth.HealthChecks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add EasyHealth health checks with custom configuration
builder.Services.AddEasyHealthChecks(options =>
{
    // Configure memory health check
    options.Memory.ThresholdBytes = 1_073_741_824; // 1GB
    options.Memory.DegradedThresholdBytes = 858_993_459; // 800MB
    options.Memory.Tags = new[] { "memory", "system" };
    
    // Add HTTP health checks for external dependencies
    options.Http.Add(new HttpHealthCheckOptions
    {
        Name = "jsonplaceholder_api",
        Url = new Uri("https://jsonplaceholder.typicode.com/posts/1"),
        Timeout = TimeSpan.FromSeconds(10),
        SlowResponseThresholdMs = 2000,
        Tags = new[] { "external", "api" }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map health check endpoints
app.MapEasyHealthChecks();

// Sample API endpoint
app.MapGet("/api/sample", () => new { Message = "Hello from EasyHealth API!", Timestamp = DateTime.UtcNow });

app.MapControllers();

app.Run();