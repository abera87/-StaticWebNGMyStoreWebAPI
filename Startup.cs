using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace StaticWebNGMyStoreWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
            .AddJwtBearer("AzureAD", options =>
            {
                options.Audience = Configuration.GetValue<string>("AzureAD:Audience");
                options.Authority = Configuration.GetValue<string>("AzureAD:Instance") + Configuration.GetValue<string>("AzureAD:TenantId");
                

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = Configuration.GetValue<string>("AzureAD:Issuer"),
                    // for single Audience
                    //ValidAudience = Configuration.GetValue<string>("AzureAD:Audience")
                    // for multiple hard coded Audiences
                    // ValidAudiences=new List<string>{
                    //     "123",
                    //     "456"
                    // },

                    // for Audiences from Appsettings
                    ValidAudiences=Configuration.GetSection("AzureAD:Audiences").Get<string[]>()
                };

                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        //Get the user's unique identifier
                        string oid = ctx.Principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
                        //Get the Azure AD tenant identifier
                        string tid = ctx.Principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
                        string userPrincipalName = ctx.Principal.FindFirstValue(ClaimTypes.Name);



                        //If the user is an admin, add them the role
                        var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Role, "Admin")
                            };
                        var appIdentity = new ClaimsIdentity(claims);

                        ctx.Principal.AddIdentity(appIdentity);

                        return Task.CompletedTask;
                    }
                };
            });


            // services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
            // .AddAzureAD(options => Configuration.Bind("AzureAd", options));

            // services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            // {
            //     options.Authority = options.Authority + "/v2.0/";

            //     options.TokenValidationParameters.ValidateIssuer = false;
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
