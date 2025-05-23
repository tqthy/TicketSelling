using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using VenueService.Data;
using VenueService.Helper;
using VenueService.Middleware;
using VenueService.Repositories;
using VenueService.Services.Interfaces;

Console.WriteLine("VENUESERVICE: MAIN METHOD ENTERED - BEFORE ANYTHING ELSE.");
System.Diagnostics.Debug.WriteLine("VENUESERVICE: DEBUG WRITELINE - MAIN METHOD ENTERED.");


SelfLog.Enable(Console.Error);

Log.Logger = new LoggerConfiguration()
    // .ReadFrom.Configuration(ConfigurationHelper.Configuration) // Helper to get config early
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger(); // Use minimal logger until host is built

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for ASP.NET Core host
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

    if (context.HostingEnvironment.IsDevelopment())
    {
        string logstashUrl = context.Configuration.GetValue<string>("SerilogHttpSinkUrl", "http://logstash:5001");
        
        loggerConfiguration.WriteTo.Http(
            requestUri: logstashUrl,
            queueLimitBytes: null, // Explicitly null (default for this param in signature)
            logEventLimitBytes: null, // Explicitly null (default)
            logEventsInBatchLimit: 50, // Your desired value
            batchSizeLimitBytes: null, // Explicitly null (default)
            period: TimeSpan.FromSeconds(2), // Your desired value
            flushOnClose: true, // Explicitly true (default)
            textFormatter: new ElasticsearchJsonFormatter(
                inlineFields: true,
                formatStackTraceAsArray: true,
                renderMessage: true
            ),
            batchFormatter: null, // Explicitly null (default for IBatchFormatter)
            restrictedToMinimumLevel: LogEventLevel.Information, // Your desired value
            levelSwitch: null, // Explicitly null (default)
            httpClient: null, // Explicitly null (default IHttpClient)
            configuration: null // Explicitly null (default IConfiguration for sink)
        );
        Console.WriteLine($"VENUESERVICE: Development mode - HTTP Sink to {logstashUrl} configured programmatically.");
    }
    else
    {
        Console.WriteLine("VENUESERVICE: Not in Development mode, HTTP Sink to Logstash NOT configured.");
    }
});


// Add services to the container.

// Add DbContext with Npgsql
builder.Services.AddDbContext<VenueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("VenueDb")));

builder.Services.AddScoped<IVenueService, VenueService.Services.VenueService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication();

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => Serilog.Events.LogEventLevel.Debug;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
    };
});

app.UseCors();

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<ClaimsFromHeadersMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
