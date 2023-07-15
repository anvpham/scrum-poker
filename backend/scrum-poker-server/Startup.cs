using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Tokens;
using scrum_poker_server.Data;
using scrum_poker_server.Data.Caching;
using scrum_poker_server.Hubs;
using scrum_poker_server.Services;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace scrum_poker_server
{
    public class Startup
    {
        public IConfiguration _configuration { get; set; }

        public IWebHostEnvironment _env { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
                  {
                      builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                  }));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                         var accessToken = context.Request.Query["access_token"];
                         var path = context.Request.Path;

                         if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/room"))
                         {
                             context.Token = accessToken;
                         }

                         return Task.CompletedTask;
                     }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OfficialUsers", policyBuilder =>
                {
                      policyBuilder.RequireClaim(ClaimTypes.Email);
                  });

                options.AddPolicy("AllUsers", policyBuilder =>
                {
                      policyBuilder.RequireClaim("UserId");
                  });
            });

            services.AddHttpClient();
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            services.AddControllers();

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));

            services
                .AddSignalR()
                .AddStackExchangeRedis(_configuration.GetConnectionString("Redis"), options =>
                {
                    options.Configuration.ChannelPrefix = "scrum-poker-hubs";
                });

            AddAppServices(services);
            AddCacheServices(services, _configuration);
        }

        public static void AddCacheServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("Redis");
                    options.InstanceName = "scrum-poker_";
                });

            services.AddTransient<ICacheService, CacheService>();
        }

        public static void AddAppServices(IServiceCollection services)
        {
            services.AddTransient<IPokingRoomManager, PokingRoomManager>(); // Used to maintain real-time sessions
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IRoomService, RoomService>();
            services.AddTransient<IJwtService, JwtService>();
            services.AddTransient<IJiraService, JiraService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                    {
                          await context.Response.WriteAsync("Web APIs of scrum poker");
                      });

                endpoints.MapHub<RoomHub>("/room");
                endpoints.MapControllers();
            });
        }
    }
}
