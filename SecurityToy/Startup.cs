using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecurityToy.Models;
using SecurityToy.Repositories;
using SecurityToy.Services;

namespace SecurityToy
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();
            Configuration = builder.Build();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(Configuration["ConnectionString:SecurityDb"]));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<IVerificationTokenService, VerificationTokenService>();
            services.AddScoped<ISmsService, TwilioSmsService>();

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("Audience");
            services.Configure<Audience>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<Audience>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ClockSkew = TimeSpan.Zero,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = appSettings.Iss,
                   ValidAudience = appSettings.Iss,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Secret))
               };
               options.SaveToken = true;
           });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(options => {

                options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();

            });
            app.UseAuthentication();

            app.UseMvc();
        }

    }
}
