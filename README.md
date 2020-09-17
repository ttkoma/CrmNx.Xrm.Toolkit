![.NET Core](https://github.com/ttkoma/CrmNx.Xrm.Toolkit/workflows/.NET%20Core/badge.svg)
[![Version](https://img.shields.io/nuget/vpre/CrmNx.Xrm.Toolkit.svg)](https://www.nuget.org/packages/CrmNx.Xrm.Toolkit)
[![Downloads](https://img.shields.io/nuget/dt/CrmNx.Xrm.Toolkit.svg)](https://www.nuget.org/packages/CrmNx.Xrm.Toolkit)
# CrmNx.Xrm.Toolkit 

Dynamics 365 WebApi Toolkit library for .Net.

Currently, support only on-premise with AD authentication.

## Getting started

Add connection string with a name "Crm".
```javascript
// appsettings.json

"ConnectionStrings": {
    "Crm": "Url=https://host.local;Username=;Password=;Domain=DMNAME;Authtype=AD"
  }
```
ConfigureServices.
```c#
// Startup.cs 

public void ConfigureServices(IServiceCollection services) 
{
    ...
    services.AddCrmWebApiClient();
    ...
}
```
Inject ICrmWebApiClient into ApiController or another class
```c#
// AccountController.cs 
...
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit;
using CrmNx.Xrm.Toolkit.Query;
...

[ApiController]
public class AccountController : ControllerBase
{
    private readonly ICrmWebApiClient _crmClient;

    public AccountController(ICrmWebApiClient crmClient)
    {
        _crmClient = crmClient ?? throw new ArgumentNullException(nameof(crmClient));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        // await _crmClient.RetrieveMultipleAsync("account") - Retrieved all fields
        var collection = await _crmClient.RetrieveMultipleAsync("account", new QueryOptions("fullname"), cancellationToken);
        
        return Ok(collection.Entities);
    }
}
```

