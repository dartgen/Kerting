
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Kerting_Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen(); => aut könnyű használat miat ezt cseréljük!

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Kerting API", Version = "v1" });

                // 1. A "lakat" gomb definiálása (Security Definition)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Kérlek, írd be a tokent!",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                // 2. A biztonsági szabály alkalmazása minden végpontra
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


            // Engedélyezzük, hogy a Vue (vagy bármilyen más frontend) hívhassa az API-t
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("VueCorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin()    // Bármilyen címről jöhet a kérés (pl. localhost:5173)
                          .AllowAnyHeader()    // Bármilyen fejlécet küldhet (pl. Authorization, Content-Type)
                          .AllowAnyMethod();   // Bármilyen metódust használhat (GET, POST, PUT, DELETE)
                });
            });

            builder.Services.AddDbContext<Libary.KertingDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddScoped(typeof(Interface.GenericInterface<>), typeof(Service.GenericService<>));
            builder.Services.AddScoped<Interface.IGalleryService, Service.GalleryService>();

            // 2. JWT KONFIGURÁCIÓ
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
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("VueCorsPolicy");

            // 2. UTÁNA jöhetnek a statikus fájlok (kisbetűs RequestPath-el!)
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(builder.Environment.ContentRootPath, "Resources")),
                RequestPath = "/resources"
            });

            app.UseAuthentication();
            app.UseAuthorization();
            

            app.MapControllers();

            app.Run();
        }
    }
}
