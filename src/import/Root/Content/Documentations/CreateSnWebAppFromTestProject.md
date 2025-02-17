# Create a sensenet WebApp
This document describes how to integrate sensenet into an ASP.NET Core web app created from scratch. During the integration, we use some source files of a selected demo application. The `sensenet` has the following demo application 
([see on Github](https://github.com/SenseNet/sensenet/tree/master/src/WebApps "Demo apps in the master branch")):

1. SnWebApplication.Api.InMem.Admin
2. SnWebApplication.Api.InMem.TokenAuth
3. SnWebApplication.Api.Sql.Admin
4. SnWebApplication.Api.Sql.SearchService.Admin
5. *SnWebApplication.Api.Sql.SearchService.TokenAuth*
6. *SnWebApplication.Api.Sql.SearchService.TokenAuth.Preview*
7. **SnWebApplication.Api.Sql.TokenAuth**
8. *SnWebApplication.Api.Sql.TokenAuth.Preview*

Italicized elements are recommended for integration purposes. In this document, we describe the use of bold element: `SnWebApplication.Api.Sql.TokenAuth`. We will use the following source files from this application:
- [Proram.cs](https://github.com/SenseNet/sensenet/blob/master/src/WebApps/SnWebApplication.Api.Sql.TokenAuth/Program.cs "Program.cs")
- [Startup.cs](https://github.com/SenseNet/sensenet/blob/master/src/WebApps/SnWebApplication.Api.Sql.TokenAuth/Startup.cs "Startup.cs")
- [appsettings.json](https://github.com/SenseNet/sensenet/blob/master/src/WebApps/SnWebApplication.Api.Sql.TokenAuth/appsettings.json "appsettings.json")

## Main steps
1. Create a webapp with Visual Studio 2022 (hereinafter: VS).
1. Add sensenet references.
1. Build Services
1. Configure database and install initial data.

## Detailed task list
You should have a directory on your machine that contains a cloned version of the
sensenet ecosystem from github. Let's call it [sn]

### Create a webapp with Visual Studio
1. Open VS
1. Click `Create a new project`
1. Select C#, All platforms, Web
1. Select ASP.NET Core Web API
1. Click Next
1. Type `Project Name`: your project name (hereinafter: `SnWebApp1`)
1. Select `Location`
1. Click Next
1. Set project generation properties on the `Additional information` page:
    1. `Framework`: .NET 8.0 (Long term support)
    1. `Authentication type`: None
    1. ON `Configure for HTTPS`
    1. OFF `Enable container support`
    1. OFF `Enable OpenAPI support`
    1. ON `Do not use top-level statements`
    1. OFF `Use controllers`
    1. OFF `Enlist in .NET Aspire orchestration`
1. Click Create

At this point, you have a webapp skeleton.

### Add sensenet references
- Open a powershell console and navigate to the project directory (the "dir" shows the csproj file)
- Execute the following lines
``` powershell
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.11
dotnet add package Microsoft.IdentityModel.Protocols.OpenIdConnect
dotnet add package Microsoft.VisualStudio.Azure.Containers.Tools.Targets
dotnet add package SenseNet.AI.Text.SemanticKernel
dotnet add package SenseNet.AI.Vision.Azure
dotnet add package SenseNet.Search.Lucene29.Local
dotnet add package SenseNet.Security.EFCSecurityStore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Graylog
dotnet add package SenseNet.ContentRepository.MsSql
dotnet add package SenseNet.OData
dotnet add package SenseNet.OpenApi
dotnet add package SenseNet.Services.Core.Install
dotnet add package SenseNet.Services.Core
dotnet add package SenseNet.Services.Wopi
dotnet add package SenseNet.WebHooks
```
- Build the solution 
- If there is a problem with ` SenseNet.TaskManagement.Core` package
(in case of sensenet github version `dotnet8`) add the following line, and build the solution again:
``` powershell
dotnet add package SenseNet.TaskManagement.Core --version 2.3.0
```
### .gitignore
When sharing source code on GitHub, it is highly recommended to add this section to the .gitignore file:
```
# sensenet
install-services-core.zip
**/install-services-core.zip
**/install-services-core
**/App_Data/LocalIndex
**/App_Data/Logs
```
### Build services and startup
In the VS
1. Add a class `Startup` class to the webapp project (the `Startup.cs` file opens)
1. Replace the entire `public class Startup { ... }` code to the demo app's entire Startup class copied from [here](https://github.com/SenseNet/sensenet/blob/master/src/WebApps/SnWebApplication.Api.Sql.TokenAuth/Startup.cs "Startup { ... }")
1. Open the existing `Program.cs`
1. Replace the entire `public class Program { ... }` code to the demo app's entire Program class copied from [here](https://github.com/SenseNet/sensenet/blob/master/src/WebApps/SnWebApplication.Api.Sql.TokenAuth/Program.cs "Program { ... }")

With this, the integrated code base is complete, now it's time to configure and install the initial data.

### Prepare configuration
1. Open the existing `appsettings.json`
1. Replace the entire code to the demo app's appSettings copied from [here](https://github.com/SenseNet/sensenet/blob/master/src/WebApps/SnWebApplication.Api.Sql.TokenAuth/appsettings.json "appsettings.json")

### Configure logging
1. Replace the `Serilog.Properties.Application` value to your app name ("`SnWebApp1`")
1. Open the existing `appsettings.Development.json` (under the `appsettings.json`)
1. Replace the entire code to the demo app's settings for development copied from [here](https://github.com/SenseNet/sensenet/blob/master/src/WebApps/SnWebApplication.Api.Sql.TokenAuth/appsettings.Development.json "appsettings.Development.json")

### Configure authentication server
Choose one of the two alternatives: use identity server or sensenet authentication server.

#### Use identity server.
This is in the `sn-identityserver` github repository. By default run this on the local machine and use this configuration:
``` json
// ...\SnWebApp1\appsettings.json
{
  "sensenet": {
    ...
    "authentication": {
      "authServerType": "IdentityServer",
      "authority": "https://localhost:44311",
      "repositoryUrl": "https://localhost:44991",
      "AddJwtCookie": true
    },
    ...
  }
}
```
The identityserver needs to be aware of the new web application, so it needs to be told that it needs to recognize new clients.
By default, `launchSettings.json` on identityserver web contains a `"SelfHost"` branch with the following content:

// sn-identityserver\SenseNet.IdentityServer4.Web
``` json
// sn-identityserver/src/SenseNet.IdentityServer4.Web/Properties/launchSettings.json
"SelfHost": {
  "commandName": "Project",
  "launchBrowser": true,
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "sensenet__Clients__spa__RepositoryHosts__0__PublicHost": "https://localhost:44362",
    "sensenet__Clients__adminui__RepositoryHosts__0__PublicHost": "https://localhost:44362"
  },
  "applicationUrl": "https://localhost:44311"
},
```
This needs to be replaced with the URLs of the new application's clients (in this example, the your new app listens on port 7025):
``` json
"sensenet__Clients__spa__RepositoryHosts__0__PublicHost": "https://localhost:7025",
"sensenet__Clients__adminui__RepositoryHosts__0__PublicHost": "https://localhost:7025"
```
It is also possible for the identityserver to listen to multiple applications at the same time, in which case the original clients do not need to be deleted (look at the numbers in the keys):
``` json
"sensenet__Clients__spa__RepositoryHosts__0__PublicHost": "https://localhost:44362",
"sensenet__Clients__adminui__RepositoryHosts__0__PublicHost": "https://localhost:44362",
"sensenet__Clients__spa__RepositoryHosts__1__PublicHost": "https://localhost:7025",
"sensenet__Clients__adminui__RepositoryHosts__1__PublicHost": "https://localhost:7025"
```
#### Use sensenet authentication server.
``` json
// ...\SnWebApp1\appsettings.json
{
  "sensenet": {
    ...
    "authentication": {
      "authServerType": "SNAuth",
      "authority": "https://localhost:7088",
      "repositoryUrl": "https://localhost:44991",
      "AddJwtCookie": false
    },
    ...
  }
}
// sn-auth/???
?????
```
### Configure launchSettings.json
?????

### Configure database
The application requires a connectionString for an existing empty database. For historical reasons, the installer section is also required, which contains some data repetition based on the connectionstring.

*Warning*: if the application code will be shared publicly (e.g. github), it is strongly recommended that the following configuration elements be placed in UserSecret instead of appSettings.json. 

To create user secret file right click the project node in the `Solution Explorer` and click the `Manage User Secrets` menuitem. After that an empty `secrets.json` will be opened.

1. Get an empty database's connectionString from your database administrator. If you create the database yourself:
    1. Open the SQL Server Management Studio (`[SSMS]`).
    1. Select the database server.
    1. In `Object Explorer` open the server treenode and right click on the `Databases`.
    1. Select `New Database...`
    1. Let's the `Database name` is `SnWebApp1`
    1. Click 'Options'
    1. Select `Recovery model` item `Simple`
    1. Click `Ok` button: the new database apeared in the bottom of the `Object Explorer/Databases`
1. Configure the database connection and installation in the `secret.json` or `appsettings.json`. Ensure the connectionString and the installer config based on the following example:
```
{
  "ConnectionStrings": {
    "SnCrMsSql": "Data Source=Computer\\MSSQLInstance;Initial Catalog=SnWebApp1;Integrated Security=SSPI;Persist Security Info=False"
  },
  "sensenet": {
    "install": {
      "mssql": {
        "Server": "Computer\\MSSQLInstance",
        "DatabaseName": "SnWebApp1"
      }
    }
  }
}
```
Note that using SQL server in a development environment sometimes runs into certification issues, so your connectionstring may need to be supplemented with this: `TrustServerCertificate=True`.

### Install initial data.

After configuration, you are ready for the first launch, which prepares the database for further use. So, press F5 in a quick but strong motion. This will launch a console application that will start printing out the installation information, which will look something like this:
```
Accessing sensenet database...
Database installed.
Unpacking embedded package to: install-services-core
Extracting ...
Unpacking finished.
Executing package install-services-core...
ComponentId: SenseNet.Services
PackageType:   Install
Package version: 7.7.41
SYSTEM INSTALL
===============================================================================
                              Executing phase 1/1
===============================================================================
Executing steps
================================================== #1/3 StartRepository
startIndexingEngine: False
indexPath: D:\projects\Markdown_WebApplication\Markdown_WebApplication\bin\Debug\net8.0\App_Data\LocalIndex
-------------------------------------------------------------
Time: 00:00:02.8706496
================================================== #2/3 Import
```
and so on. The entire process and additional information can be traced back in the log files: `/App_Data/Logs`

.

.

.

.

.

.

.

.

.

.

.

.

.





-------------------------------------------- latest dev
CD D:\projects\_documented\
MD NewSnApp
CD NewSnApp
dotnet new webapi --framework net8.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.11
dotnet add package Microsoft.IdentityModel.Protocols.OpenIdConnect
dotnet add package Microsoft.VisualStudio.Azure.Containers.Tools.Targets
dotnet add package SenseNet.AI.Text.SemanticKernel
dotnet add package SenseNet.AI.Vision.Azure
dotnet add package SenseNet.Search.Lucene29.Local
dotnet add package SenseNet.Security.EFCSecurityStore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Graylog
dotnet add package SenseNet.ContentRepository.MsSql
dotnet add package SenseNet.OData
dotnet add package SenseNet.OpenApi
dotnet add package SenseNet.Services.Core.Install
dotnet add package SenseNet.Services.Core
dotnet add package SenseNet.Services.Wopi
dotnet add package SenseNet.WebHooks

-------------------------------------------- latest released
CD D:\projects\_documented\
MD NewSnApp
CD NewSnApp
dotnet new webapi --framework net8.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.11
dotnet add package Microsoft.IdentityModel.Protocols.OpenIdConnect
dotnet add package Microsoft.VisualStudio.Azure.Containers.Tools.Targets
dotnet add package SenseNet.AI.Text.SemanticKernel -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.AI.Vision.Azure -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.Search.Lucene29.Local -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.Security.EFCSecurityStore -s https://api.nuget.org/v3/index.json
dotnet add package Serilog.AspNetCore -s https://api.nuget.org/v3/index.json
dotnet add package Serilog.Sinks.Graylog -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.ContentRepository.MsSql -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.OData -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.OpenApi -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.Services.Core.Install -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.Services.Core -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.Services.Wopi -s https://api.nuget.org/v3/index.json
dotnet add package SenseNet.WebHooks -s https://api.nuget.org/v3/index.json
