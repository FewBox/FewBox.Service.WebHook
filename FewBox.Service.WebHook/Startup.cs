using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FewBox.Core.Persistence.Orm;
using FewBox.Core.Utility.Net;
using FewBox.Core.Utility.Formatter;
using FewBox.Core.Web.Config;
using FewBox.Core.Web.Filter;
using FewBox.Core.Web.Orm;
using FewBox.Core.Web.Security;
using FewBox.Core.Web.Token;
using FewBox.Service.WebHook.Domain;
using FewBox.Service.WebHook.Model.Configs;
using FewBox.Service.WebHook.Model.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NSwag;
using NSwag.SwaggerGeneration.Processors.Security;

namespace FewBox.Service.WebHook
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.Configuration = configuration;
            this.HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RestfulUtility.IsCertificateNeedValidate = false;
            RestfulUtility.IsLogging = true; // Todo: Need to remove.
            JsonUtility.IsCamelCase = true;
            JsonUtility.IsNullIgnore = true;
            services.AddMvc(options=>{
                if (this.HostingEnvironment.EnvironmentName != "Test")
                {
                    options.Filters.Add<ExceptionAsyncFilter>();
                }
                options.Filters.Add<TraceAsyncFilter>();
            })
            .AddJsonOptions(options=>{
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .ConfigureApplicationPartManager(apm =>
            {
                var dependentLibrary = apm.ApplicationParts
                    .FirstOrDefault(part => part.Name == "FewBox.Core.Web");
                if (dependentLibrary != null)
                {
                    apm.ApplicationParts.Remove(dependentLibrary);
                }
            }); // Note: Remove AuthenticationController.
            services.AddCors();
            services.AddAutoMapper();
            services.AddMemoryCache();
            services.AddRouting(options => options.LowercaseUrls = true);
            // Used for [Authorize(Policy="JWTRole_ControllerAction")].
            var jwtConfig = this.Configuration.GetSection("JWTConfig").Get<JWTConfig>();
            services.AddSingleton(jwtConfig);
            var securityConfig = this.Configuration.GetSection("SecurityConfig").Get<SecurityConfig>();
            services.AddSingleton(securityConfig);
            // Used for Config.
            var healthyConfig = this.Configuration.GetSection("HealthyConfig").Get<HealthyConfig>();
            services.AddSingleton(healthyConfig);
            var rabbitMQConfig = this.Configuration.GetSection("RabbitMQConfig").Get<RabbitMQConfig>();
            services.AddSingleton(rabbitMQConfig);
            // Used for RBAC AOP.
            services.AddScoped<IAuthorizationHandler, RoleHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, RoleAuthorizationPolicyProvider>();
            services.AddScoped<IAuthenticationService, RemoteAuthenticationService>();
            // Used for Application.
            services.AddScoped<IAppService, AppService>();
            // Used for Exception&Log AOP.
            services.AddScoped<IExceptionHandler, ConsoleExceptionHandler>();
            services.AddScoped<ITraceHandler, ConsoleTraceHandler>();
            // Used for IHttpContextAccessor&IActionContextAccessor context.
            services.AddHttpContextAccessor();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            // Used for JWT.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key))
                };
            });
            // Used for Swagger Open Api Document.
            services.AddOpenApiDocument(config => {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "FewBox webhook Api";
                    document.Info.Description = "FewBox webhook, for more information please visit the 'https://fewbox.com'";
                    document.Info.TermsOfService = "https://fewbox.com/terms";
                    document.Info.Contact = new NSwag.SwaggerContact
                    {
                        Name = "FewBox",
                        Email = "support@fewbox.com",
                        Url = "https://fewbox.com/support"
                    };
                    document.Info.License = new NSwag.SwaggerLicense
                    {
                        Name = "Use under license",
                        Url = "https://raw.githubusercontent.com/FewBox/FewBox.Service.WebHook/master/LICENSE"
                    };
                };
                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
                config.DocumentProcessors.Add(
                    new SecurityDefinitionAppender("JWT", new List<string>{"API"}, new SwaggerSecurityScheme
                    {
                        Type = SwaggerSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        Description = "Bearer [Token]",
                        In = SwaggerSecurityApiKeyLocation.Header
                    })
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            //app.UseHttpsRedirection();
            app.UseMvc();
            app.UseStaticFiles();
            app.UseSwagger();
            if (env.IsDevelopment() || env.IsStaging())  
            {
                app.UseSwagger();  
                app.UseSwaggerUi3();  
            }
            else
            {
                app.UseReDoc();
            }
            app.UseCors();
        }
    }
}