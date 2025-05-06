using Microsoft.EntityFrameworkCore;
using VenueService.Data;
using VenueService.Middleware;
using VenueService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext with Npgsql
builder.Services.AddDbContext<VenueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("VenueDb")));

builder.Services.AddScoped<IVenueService, VenueService.Services.VenueService>();

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


app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<ClaimsFromHeadersMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();