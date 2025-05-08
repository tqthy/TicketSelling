using Microsoft.EntityFrameworkCore;
using Serilog;
using VenueService.Data;
using VenueService.Helper;
using VenueService.Middleware;
using VenueService.Repositories;
using VenueService.Services.Interfaces;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(ConfigurationHelper.Configuration) // Helper to get config early
    .Enrich.FromLogContext()
    .CreateBootstrapLogger(); // Use minimal logger until host is built

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for ASP.NET Core host
builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration) // Also requires the package
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
    // ... other Serilog config ...
);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<ClaimsFromHeadersMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
