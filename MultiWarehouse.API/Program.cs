using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MultiWarehouse.API.Middlewares;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Extensions;
using MultiWarehouse.Service.Repositories.Implementations;
using MultiWarehouse.Service.Repositories.Interfaces;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. FRAMEWORK AYARLARI
// =========================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Frontend'in çalıştığı tam adres
              .AllowAnyHeader()  // Her türlü Header'a (Authorization vb.) izin ver
              .AllowAnyMethod()  // GET, POST, PUT, DELETE hepsine izin ver
              .AllowCredentials(); // (Opsiyonel) İleride Cookie tabanlı yetkilendirme yaparsan şarttır
    });
});

// =========================================================
// 2. VERİTABANI VE KONFİGÜRASYON (OPTIONS) AYARLARI
// =========================================================
// PostgreSQL Veritabanı Bağlantısı (DbContext) Ayarı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// OPTIONS PATTERN UYGULAMASI:
// "TokenOptions" adındaki JSON bloğunu okur ve CustomTokenOption sınıfına doldurur.
// Sistemin neresinde (IOptions<CustomTokenOption>) istenirse, bu dolu sınıfı otomatik olarak verir.
builder.Services.Configure<MultiWarehouse.Shared.Configurations.CustomTokenOption>(
    builder.Configuration.GetSection("TokenOptions")
);

// =========================================================
// 3. BAĞIMLILIK ENJEKSİYONU (DEPENDENCY INJECTION)
// =========================================================
// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//servis ekleme
builder.Services.AddServiceLayer();



// =========================================================
// 4. SWAGGER VE GÜVENLİK (API DOKÜMANTASYONU)
// =========================================================
// Swagger Configuration (XML Docs + JWT Auth Support)
builder.Services.AddSwaggerGen(options =>
{
    // 1. XML Dokümantasyonunu Bağlama
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // 2. Swagger UI üzerine JWT "Authorize" Butonu Ekleme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Lütfen JWT Token bilginizi 'Bearer {token}' formatında giriniz."
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

// Token ayarlarını appsettings.json'dan çekiyoruz
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<MultiWarehouse.Shared.Configurations.CustomTokenOption>();

// JWT Doğrulama Kuralları
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),

        ValidateIssuerSigningKey = true, // Şifreyi doğrula
        ValidateAudience = true,         // Audience doğrula
        ValidateIssuer = true,           // Issuer doğrula
        ValidateLifetime = true,         // Süreyi kontrol et
        ClockSkew = TimeSpan.Zero        // Süre bitiminde ekstra 5dk tolerans verme, anında kes
    };
});

builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// =========================================================
// 5. HTTP İSTEK HATTI (PIPELINE / MIDDLEWARES)
// =========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Multi Warehouse API v1");
    });
}

app.UseCustomException();
app.UseHttpsRedirection();

app.UseCors("AllowNextJs");

// Kimlik Doğrulama ve Yetkilendirme Middleware'leri
app.UseAuthentication(); // JWT ile giriş yapıldığını doğrular
app.UseAuthorization();  // Kullanıcının o sayfaya yetkisi var mı diye bakar

app.MapControllers();
app.Run();