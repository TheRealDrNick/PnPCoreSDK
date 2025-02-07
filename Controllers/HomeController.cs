using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnPCoreSDK.Models;
using PnPCoreSDK.Options;

namespace PnPCoreSDK.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPnPContextFactory _pnpContextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly IOptions<AzureAdDelegate> _delegateOptions;

    public HomeController(
        ILogger<HomeController> logger, 
        IPnPContextFactory pnPContextFactory,
        IHttpContextAccessor httpContextAccessor, 
        IOptions<AzureAdDelegate> delegateOptions
        )
    {
        _logger = logger;
        _pnpContextFactory = pnPContextFactory;
        _httpContextAccessor = httpContextAccessor;
        _delegateOptions = delegateOptions;
    }

    // just for testing purposes
    public async Task<IActionResult> GetAccessToken()
    {
        ViewBag.IdToken = await _httpContextAccessor.HttpContext?.GetTokenAsync("id_token");
        //var accessToken = await _httpContextAccessor.HttpContext?.GetTokenAsync("access_token");
        //  ViewBag.AccessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
        return View();
    }

    public async Task<IActionResult> Index()
    {
        var idToken = await _httpContextAccessor.HttpContext?.GetTokenAsync("id_token");
        var tenantId = _delegateOptions.Value.TenantId;
        var clientId = _delegateOptions.Value.ClientId;
        var clientSecret = _delegateOptions.Value.ClientSecret;
        var siteUrl = _delegateOptions.Value.SiteUrl;
        var onBehalfAuthProvider = new OnBehalfOfAuthenticationProvider(clientId,
            tenantId, new PnPCoreAuthenticationOnBehalfOfOptions(){ ClientSecret = clientSecret },
        () => idToken);
        
        // delegate access to sharepoint
        using (var context = await _pnpContextFactory.CreateAsync(new Uri(siteUrl), onBehalfAuthProvider))
        {
            try
            {
                var sharedDocuments = context.Web.Lists.GetByTitle("Documents", l => l.RootFolder);
                // local file for testing purposes ... needs to be a stream
                IFile addedFile = await sharedDocuments.RootFolder.Files.AddAsync("Test123.docx",
                    System.IO.File.OpenRead(@"C:\\Temp\\Test123.docx"), true);
                ViewBag.Id = addedFile.Title;
                return View();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { message = ex.Message });
            }
        }
    }


    // application access to sharepoint -> refers to program.cs alias for site
    public async Task<IActionResult> SPRequest()
    {
        using (var context = await _pnpContextFactory.CreateAsync("SuretyDocuments"))
        {
            try
            {
                var sharedDocuments = context.Web.Lists.GetByTitle("Documents", l => l.RootFolder);
                // local file for testing purposes ... needs to be a stream
                IFile addedFile = await sharedDocuments.RootFolder.Files.AddAsync("Test123.docx",
                    System.IO.File.OpenRead(@"C:\\Temp\\Test123.docx"), true);
                ViewBag.Id = addedFile.Title;
                return View();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { message = ex.Message });
            }
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}