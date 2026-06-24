using System.Security.Claims;
using System.Text;
using EventosVivos_Api.Data;
using EventosVivos_Api.Models.Security;
using EventosVivos_Api.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Agrega los servicios al contenedor.

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentityCore<User>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var sessionId = context.Principal?.FindFirstValue(JwtTokenStore.SessionIdClaim);

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId))
                {
                    context.Fail("Invalid access token.");
                    return;
                }

                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var user = await userManager.FindByIdAsync(userId);
                if (user is null || !user.IsActive)
                {
                    context.Fail("Invalid access token.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var sessionExists = await dbContext.UserTokens.AnyAsync(userToken =>
                    userToken.UserId == userId
                    && userToken.LoginProvider == JwtTokenStore.LoginProvider
                    && userToken.Value == sessionId);

                if (!sessionExists)
                {
                    context.Fail("Token has been revoked.");
                }
            }
        };
    });

builder.Services.AddControllers();
// Configura Swagger/OpenAPI para documentar la API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT de la sesion."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

await DatabaseSeeder.SeedAsync(app.Services);
if (args.Contains("--seed-only", StringComparer.OrdinalIgnoreCase))
{
    return;
}

// Configura el pipeline HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
