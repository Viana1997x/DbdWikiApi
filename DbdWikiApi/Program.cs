using DbdWikiApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configura��es do Banco de Dados
builder.Services.Configure<DbdWikiDatabaseSettings>(
    builder.Configuration.GetSection("DbdWikiDatabaseSettings"));

// Registrar Servi�os
builder.Services.AddSingleton<DbdWikiService>();
builder.Services.AddScoped<UserService>();

// Servi�o de CORS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configura��o do JWT e Swagger (sem altera��es)
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Dead by Daylight Wiki API",
        Description = "Uma API para consultar informa��es e gerenciar usu�rios.",
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer ' seguido do seu token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- ORDEM DO PIPELINE CORRIGIDA E ROBUSTA ---

// 1. Redireciona para HTTPS
app.UseHttpsRedirection();

// --- ADICIONE ESTA LINHA ---
// Habilita o uso de arquivos est�ticos, como imagens salvas na pasta wwwroot
app.UseStaticFiles();
// -------------------------

// 2. ADICIONADO: Garante que a rota seja identificada
app.UseRouting();

// 3. Aplica a pol�tica de CORS
app.UseCors(myAllowSpecificOrigins);

// 4. Aplica autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization();

// 5. Mapeia os controllers para os endpoints
app.MapControllers();

app.Run();