using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Data;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKeyPerFile(directoryPath: "/run/secrets", optional: true, reloadOnChange: true);
// DbContext configuration
var connectionString = builder.Configuration.GetConnectionString("UserServiceDb");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'UserServiceDb' not found.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


// Identity configuration
// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
    {
        // Configure password settings if needed (optional)
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedAccount = false; // Good for demos, usually true for production
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Needed for things like password reset tokens



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not found."))),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IUserService, UserService.Services.UserService>(); 



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeRoles(services); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the roles.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
