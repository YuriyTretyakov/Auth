    
using System;
using System.Collections.Generic;
using System.Text;
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
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Authorization.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Authorization.ExternalLoginProvider;

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

         //   var fbProvider = new FacebookLoginProvider();
           // fbProvider.Secret = _config["FaceBookSecret"];
           // fbProvider.Appid = _config["FaceBookAppId"];


            services.AddSingleton<FacebookLoginProvider>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                

            }).AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["FaceBookAppId"];
                    facebookOptions.AppSecret = Configuration["FaceBookSecret"];
                })
                .AddCookie((options => { options.Cookie.IsEssential = true; }))
                .AddGoogle(google=>
            {
                google.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                google.ClientId = Configuration["ClientId"];
                google.ClientSecret = Configuration["ClientSecret"];
               // google.CallbackPath = "/signin-google";
                //google.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                //google.ClaimActions.Clear();
                //google.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                //google.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                //google.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                //google.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                //google.ClaimActions.MapJsonKey("urn:google:profile", "link");
                //google.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                //google.ClaimActions.MapJsonKey("urn:google:image", "picture");
                ////google.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
            })
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
                    ValidAudience = Configuration["JwtIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

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