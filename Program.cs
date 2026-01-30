using System.Text;
using FileSharing.Services;
using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("https://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
        ),
        ClockSkew = TimeSpan.Zero //domyślnie 5 minut tolerancji??
    };


    //cookies 
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("accessToken"))
            {
                context.Token = context.Request.Cookies["accessToken"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAuthService, AuthService>();

//Singleton - Background Jobs 
builder.Services.AddHostedService<FileCleanupBackgroundService>(); 

// Database
builder.Services.AddDbContext<FileSharingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"🔍 Connection String: '{connectionString}'");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<FileSharingDbContext>();
        Console.WriteLine("Running database migrations...");
        db.Database.Migrate();
        Console.WriteLine("Migrations completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration failed: {ex.Message}");
        throw;
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Debug
app.Use(async (context, next) =>
{
    Console.WriteLine("=== Request Debug ===");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine($"Method: {context.Request.Method}");
    
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        Console.WriteLine($"Auth Header: {context.Request.Headers["Authorization"]}");
    }
    else
    {
        Console.WriteLine("⚠️ NO Authorization header!");
    }
    
    await next();
    
    Console.WriteLine($"Response Status: {context.Response.StatusCode}");
    Console.Write($"Response Time: {DateTime.Now}");
    Console.WriteLine("===================\n");
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; script-src 'self'; object-src 'none';");
    await next();
});


app.UseCors("AllowReactApp");     
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();          
app.UseAuthorization();           

app.MapControllers();

app.Run();
