
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Authorization.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Swagger;
using Authorization.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Authorization.ExternalLoginProvider.FaceBook;
using Authorization.ExternalLoginProvider.Google;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Authorization.Helpers.RefreshToken;

namespace Authorization
{
    public class Startup
    {
        private readonly IConfigurationRoot _config;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _config = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            const string authName = "Authorization";
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
               
            });
            services.AddAuthorization(options => {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddSingleton(new FacebookLoginProvider());
            services.AddSingleton(new GoogleLoginProvider(_config["ClientId"],_config["ClientSecret"]));
            services.AddSingleton(new TokenStorage());
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                

            }).AddCookie((options => { options.Cookie.IsEssential = true; }))
            .AddJwtBearer(cfg =>
            {
               
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = Configuration["JwtIssuer"],
                    ValidAudience = Configuration["JwtAudince"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
                cfg.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }

                        return Task.CompletedTask;
                    }
                };

            });
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddMvc()
                .AddJsonOptions(
                config =>
                {
                    config.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }
                );

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My cool swagger", Version = "1.0" });
                c.AddSecurityDefinition(authName, new ApiKeyScheme() { In = "header", Name = authName, Type = "apiKey" });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                    {
                        {authName, new string[] { }}
                    });
            }
            );

            services.AddSingleton<IConfiguration>(_config);
            services.AddDbContext<AuthDbContext>();
            services.AddTransient<AuthDataSeeder>();
            

            services.AddIdentity<User, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;

            }).AddDefaultTokenProviders()
              .AddEntityFrameworkStores<AuthDbContext>();

            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, AuthDataSeeder seedData)
        {
            app.UseCors("CorsPolicy");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();


            

            app.UseMiddleware<Middleware>();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger Sample");

            });

            app.UseHttpsRedirection();

            
            
            app.UseMvc();
            seedData.SeedUsers().Wait();
        }
    }
}