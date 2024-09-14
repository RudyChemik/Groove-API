using ElGatoAPI.Data;
using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PayPalCheckoutSdk.Core;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MSC API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});


//DB CONN sq
var ConnectionString = builder.Configuration.GetConnectionString("DeafultConnectionString");
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(ConnectionString));

//SERVICES
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IStudioService, StudioService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddScoped<IShoppingService, ShoppingService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IJwtService, JwtService>();

//IDENTITY
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var roleManager = builder.Services.BuildServiceProvider().GetRequiredService<RoleManager<IdentityRole>>();
var roles = new[] { "admin", "artist", "studiohead", "user", "studioadmin" };

foreach (var role in roles)
{
    if (!roleManager.RoleExistsAsync(role).Result)
    {
        roleManager.CreateAsync(new IdentityRole(role)).Wait();
    }
}

//JWT 
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var configuration = builder.Configuration;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudience = configuration["AuthSettings:Audience"],
        ValidIssuer = configuration["AuthSettings:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthSettings:Key"])),
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
    options.AddPolicy("Artist", policy => policy.RequireRole("artist"));
    options.AddPolicy("StudioHead", policy => policy.RequireRole("studiohead"));
    options.AddPolicy("StudioAdmin", policy => policy.RequireRole("studioadmin"));
    options.AddPolicy("User", policy => policy.RequireRole("user"));
});


//PASS REQ
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8; 
});


builder.Services.AddSingleton<PayPalEnvironment>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var payPalClientId = configuration["PayPalSettings:ClientId"];
    var payPalClientSecret = configuration["PayPalSettings:ClientSecret"];
    return new SandboxEnvironment(payPalClientId, payPalClientSecret);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

AppDbInitializer.Initialize(app);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
