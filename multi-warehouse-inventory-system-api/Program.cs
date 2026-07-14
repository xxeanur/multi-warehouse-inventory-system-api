using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using multi_warehouse_inventory_system_api.Middlewares;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Repositories.Implementations;
using MultiWarehouse.Service.Repositories.Interfaces;
using System.IO;
using System.Reflection;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Service.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OPTIONS PATTERN UYGULAMASI:
// "TokenOptions" adındaki JSON bloğunu okur ve CustomTokenOption sınıfına doldurur.
// Sistemin neresinde (IOptions<CustomTokenOption>) istenirse, bu dolu sınıfı otomatik olarak verir.
builder.Services.Configure<MultiWarehouse.Shared.Configurations.CustomTokenOption>(
    builder.Configuration.GetSection("TokenOptions")
);
// PostgreSQL Veritabanı Bağlantısı (DbContext) Ayarı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//servisler
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Swagger Configuration (XML Docs + JWT Auth Support)
builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Kimlik Doğrulama ve Yetkilendirme Middleware'leri
app.UseAuthentication(); // JWT ile giriş yapıldığını doğrular
app.UseAuthorization();  // Kullanıcının o sayfaya yetkisi var mı diye bakar

app.MapControllers();
app.Run();