using ActivityService;
using AuthService;
using CookieService;
using DataService;
using FiltersService;
using FunctionalService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ModelService;
using PokerAPI.Extensions;
using System;
using System.Text;
using System.Text.Json;
using UserService;
using PokerAPI.Hubs;
using PokerLogic.Models;
using Microsoft.AspNetCore.SignalR;
using GameLogService;
using StorageService;
using Microsoft.AspNetCore.HttpOverrides;

namespace PokerAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        readonly string MyAllowSpecificOrigins = "cors";
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson(options => {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });
            //.AddControllersAsServices().AddRazorRuntimeCompilation().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddSignalR(o => {
                o.EnableDetailedErrors = true;
            })
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.ConfigureCors();
            services.AddCors(options =>
            {
                options.AddPolicy("localhost:4200", builder => { builder.WithOrigins("http://localhost:4200/").AllowAnyHeader().AllowAnyMethod(); });
                options.AddPolicy(name: MyAllowSpecificOrigins, builder => builder.WithOrigins("http://localhost:4200/").AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed((host) => true));
            });
            services.ConfigureIISIntegration();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("PokerDB_Temp"), x => x.MigrationsAssembly("PokerAPI")), ServiceLifetime.Scoped);

            //options.UseSqlServer(Configuration.GetConnectionString("ProdPoker"), x => x.MigrationsAssembly("PokerAPI")), ServiceLifetime.Scoped);
            //options.UseSqlServer(Configuration.GetConnectionString("TestMigration"), x => x.MigrationsAssembly("PokerAPI")), ServiceLifetime.Scoped);
            //options.UseSqlServer(Configuration.GetConnectionString("PokerRework"), x => x.MigrationsAssembly("PokerAPI")));
            //options.UseSqlServer(Configuration.GetConnectionString("PokerDB"), x => x.MigrationsAssembly("PokerAPI")));

            //sqlServerOptionsAction: sqlOptions =>
            //    {
            //        sqlOptions.EnableRetryOnFailure();
            //    })));

            services.AddDbContext<DataProtectionKeysContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("PokerDPK_Temp"), x => x.MigrationsAssembly("PokerAPI")));

            //options.UseSqlServer(Configuration.GetConnectionString("ProdPokerDPK"), x => x.MigrationsAssembly("PokerAPI")), ServiceLifetime.Scoped);
            //options.UseSqlServer(Configuration.GetConnectionString("TestMgrDPK"), x => x.MigrationsAssembly("PokerAPI")));
            //options.UseSqlServer(Configuration.GetConnectionString("PokerDPK"), x => x.MigrationsAssembly("PokerAPI")));

            services.AddTransient<IFunctionalSvc, FunctionalSvc>();

            services.Configure<AdminUserOptions>(Configuration.GetSection("AdminUserOptions"));
            services.Configure<AppUserOptions>(Configuration.GetSection("AppUserOptions"));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => 
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityDefaultOptions.LockoutDefaultLockoutTimeSpanInMinutes);
                //options.Lockout.MaxFailedAccessAttempts = identityDefaultOptions.LockoutMaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = false;//identityDefaultOptions.LockoutAllowedForNewUsers;

                // User settings
                //options.User.RequireUniqueEmail = identityDefaultOptions.UserRequireUniqueEmail;

                // email confirmation require
                options.SignIn.RequireConfirmedEmail = false;//identityDefaultOptions.SignInRequireConfirmedEmail;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            var dataProtectionSection = Configuration.GetSection("DataProtectionKeys");
            services.Configure<DataProtectionKeys>(dataProtectionSection);
            services.AddDataProtection().PersistKeysToDbContext<DataProtectionKeysContext>();

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(o =>
            {
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = appSettings.ValidateIssuerSigningKey,
                    ValidateIssuer = appSettings.ValidateIssuer,
                    ValidateAudience = appSettings.ValidateAudience,
                    ValidIssuer = appSettings.Site,
                    ValidAudience = appSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddHttpContextAccessor();
            //services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IStorageSvc, StorageSvc>();

            services.AddTransient<IUserSvc, UserSvc>();
            services.AddTransient<IAuthSvc, AuthSvc>();
            services.AddTransient<IActivitySvc, ActivitySvc>();
            //services.AddTransient<GameCtrlInt, PokerRoom>();
            //services.AddTransient<IPokerRoomSvc, PokerRoomSvc>();
            //services.AddSingleton<IGameHandler, GameHandler>();

            services.AddTransient<CookieOptions>();
            services.AddTransient<ICookieSvc, CookieSvc>();

            //GameLog svc
            services.AddScoped<IGameLogSvc, GameLogSvc>();

            services.AddAuthentication("Administrator").AddScheme<AdminAuthenticationOptions, AdminAuthenticationHandler>("Admin", null);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
           
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);
            app.UseCors(builder => builder.WithOrigins("http://localhost:4200/")
                  .AllowAnyMethod()
                  .AllowAnyHeader());

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseAuthorization();

            //CONTINUE WITH MIDDLEWARE - LORIK
            //app.UseMiddleware<IpRevMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<LobbyHub>("/notify/looby");
                //endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");

                endpoints.MapHub<RoomHub>("/notify/rooms");
                //endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}