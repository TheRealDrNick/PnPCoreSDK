using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using PnPCoreSDK.Options;

namespace PnPCoreSDK;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInMemoryTokenCaches();
        
        // this settings section is used for user authentication and user delegate permissions for SharePoint
        builder.Services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdDelegate"));

        // make id token available in controller
        // if not set, no token is in the http response header available
        // is different for jwt tokens (access tokens)
        // needs to be done due to auth provider for sharepoint delegate permission access
        builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = options.Authority;
            options.SaveTokens = true;
            options.TokenValidationParameters.ValidateIssuer = false;
        });
        
        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        builder.Services.AddOptions()
            .Configure<AzureAdApplication>(builder.Configuration.GetSection("AzureAdApplication"))
            .Configure<AzureAdDelegate>(builder.Configuration.GetSection("AzureAdDelegate"));
        
        // app registration configuration for application permission
        var certificatePath = builder.Configuration["AzureAdApplication:CertificatePath"];
        var certificatePassword = builder.Configuration["AzureAdApplication:CertificatePassword"];;
        var certificate = new X509Certificate2(certificatePath, certificatePassword, X509KeyStorageFlags.Exportable);
        builder.Services.AddPnPCore(options =>
        {
            options.PnPContext.GraphFirst = true;
            // assign alias for specific site url
            options.Sites.Add("SuretyDocuments", new PnPCoreSiteOptions
            {
                SiteUrl = builder.Configuration["AzureAdApplication:SiteUrl"]
            });
        });
        
        builder.Services.AddPnPCoreAuthentication(options =>
        {
            options.Credentials.Configurations.Add("x509certificate",
                new PnPCoreAuthenticationCredentialConfigurationOptions
                {
                    ClientId = builder.Configuration["AzureAdApplication:ClientId"],
                    TenantId = builder.Configuration["AzureAdApplication:TenantId"],
                    X509Certificate = new PnPCoreAuthenticationX509CertificateOptions
                    {
                        Certificate = certificate
                    }
                });
            // Configure the default authentication provider
            options.Credentials.DefaultConfiguration = "x509certificate";

            // Map the site defined in AddPnPCore with the 
            // Authentication Provider configured in this action
            options.Sites.Add("SuretyDocuments",
                new PnPCoreAuthenticationSiteOptions
                {
                    AuthenticationProviderName = "x509certificate"
                });
        });

        builder.Services.AddHttpContextAccessor();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}
