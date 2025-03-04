﻿namespace PnPCoreSDK.Options;

public class AzureAdDelegate
{
    public string Instance { get; set; }
    public string Domain { get; set; }
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scopes { get; set; }
    public string CallbackPath { get; set; }
    public string SiteUrl { get; set; }
}