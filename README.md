IMPORTANT HINT: THIS IS JUST FOR TESTING PURPOSES
Use this setup at your own risk!

For more information please visit the [PnP.Core.SDK website](https://pnp.github.io/pnpcore/using-the-sdk/readme.html)

Setup this project
1. Create Azure App Registration
   
   a. For Application permissions:
   ![image](https://github.com/user-attachments/assets/06195e73-4c48-4416-acb1-d527e8dd95ab)

    - Certificate instead of user secret (not supported anymore)
    - Id Tokens
    - Redirect Url ends with /signin-oidc
    

   b. For Delegate permissions:
   ![image](https://github.com/user-attachments/assets/7d70e772-babc-4889-93e3-7cd7413ee03a)

    - User secret
    - Id Tokens
    - Redirect Url ends with /signin-oidc


2. Local secrets example - This should be app settings in Azure App Service with Key Vault references in production

    - Delegate settings:
    ```json
    "AzureAdDelegate": {
      "Instance": "https://login.microsoftonline.com/",
      "Domain": "{domainName}.onmicrosoft.com",
      "TenantId": "{tenantId}",
      "ClientId": "{clientId}",
      "ClientSecret": "{secret}",
      "Scopes": "{spSiteUrl}/.default",
      "CallbackPath": "/signin-oidc",
      "SiteUrl": "{spSiteUrl}"
    }
    ```
  
    - Application settings:
    ```json
    "AzureAdApplication": {
      "ClientId": "{clientId}",
      "TenantId": "{tenantId}",
      "SiteUrl": "{spSiteUrl}",
      "CertificatePath": "{pathToYourCert.pfx}",
      "CertificatePassword": "{pw}"
    }
    ```
