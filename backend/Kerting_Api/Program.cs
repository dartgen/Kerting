
using Kerting_Api.Interface;
using Kerting_Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Text;

namespace Kerting_Api
{
    /// <summary>
    /// A Kerting API indulási pontja.
    /// Itt állítjuk össze a teljes alkalmazás konténerét (DI),
    /// a hitelesítési szabályokat, valamint a HTTP pipeline futási sorrendjét.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // A builder gyűjti össze az összes szolgáltatás-regisztrációt és konfigurációt.
            var builder = WebApplication.CreateBuilder(args);

            // A JSON ciklikus referencia problémák elkerülése:
            // egyes EF navigációs kapcsolatok körkörös objektumgráfot adhatnak vissza.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            // Swagger/OpenAPI alap szolgÃ¡ltatÃ¡sok.
            builder.Services.AddEndpointsApiExplorer();

            // Swagger dokumentáció + JWT "Bearer" gomb beállítása.
            // A cél, hogy fejlesztés közben közvetlenül a Swagger felületről
            // lehessen hitelesített végpontokat tesztelni.
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Kerting API", Version = "v1" });

                // 1) A "Bearer" séma leírása.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Kérlek, írd be a tokent!",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                // 2) A fenti séma kötelezővé tétele a védett végpontoknál.
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
                        new string[] {}
                    }
                });
            });

            // CORS szabály: a frontend fejlesztői környezetből érkező hívások engedélyezése.
            // Jelenleg nyitott policy, mert fejlesztési sebességre optimalizál.
            // Produkcióban domain-szűkítés javasolt.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("VueCorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Adatbázis kapcsolat (EF Core + SQL Server).
            builder.Services.AddDbContext<Libary.KertingDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            // Függőség-injektálás: interfész -> konkrét implementáció map.
            // Ez biztosítja, hogy a controllerek a szerződésre (interface) támaszkodjanak,
            // ne közvetlenül az adatbázisra vagy konkrét osztályokra.
            builder.Services.AddScoped(typeof(GenericInterface<>), typeof(GenericService<>));
            builder.Services.AddScoped<IGalleryService, GalleryService>();
            builder.Services.AddScoped<IForumService, ForumService>();
            builder.Services.AddScoped<IWorkService, WorkService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<ICalendarService, CalendarService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IActivityTagService, ActivityTagService>();
            builder.Services.AddScoped<IUserReviewService, UserReviewService>();
            builder.Services.AddScoped<IFeaturedUsersService, FeaturedUsersService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // JWT konfiguráció:
            // a bejelentkezéskor kiadott tokenek ellenőrzési szabályait itt definiáljuk.
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            builder.Services.AddAuthentication(item =>
            {
                item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(item =>
            {
                item.RequireHttpsMetadata = false;
                item.SaveToken = true;
                item.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    // Aláírás ellenőrzése a közös titkos kulccsal.
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    // Kibocsátó és célközönség ellenőrzése a token-hamisítás ellen.
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],

                    // Nincs extra türelmi idő: lejárat után azonnal érvénytelen token.
                    ClockSkew = TimeSpan.Zero
                };
            });

            var app = builder.Build();

            // A HTTP kérésfeldolgozási pipeline konfigurálása.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // A middleware sorrend kritikus:
            // routing -> CORS -> HTTPS -> statikus fájlok -> auth -> authorization -> controller mapping.
            app.UseRouting();
            app.UseCors("VueCorsPolicy");
            
            app.UseHttpsRedirection();

            // A feltöltött médiafájlok publikálása /resources útvonal alatt.
            // Így a frontend közvetlenül hivatkozhat backend által mentett képekre.
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(builder.Environment.ContentRootPath, "Resources")),
                RequestPath = "/resources"
            });

            // Hitelesítés: tokenből felhasználó azonosítás.
            app.UseAuthentication();

            // Jogosultságkezelés: [Authorize] és szerepkör alapú szabályok érvényesítése.
            app.UseAuthorization();
            
            // Controller végpontok felkötése a routing táblába.
            app.MapControllers();

            // Alkalmazás indítása (blokkoló hívás).
            app.Run();
        }
    }
}
