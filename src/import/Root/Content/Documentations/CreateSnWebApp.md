# OBSOLETE
## Use CreateSnWebAppFromTestProject.md instead

# Create a copied WebApp

A new sensenet-integrated webapp can be created from a copy of the sensenet github repository
under development. This document describes how to start this type of project.
In this case, you have to decide whether to use the latest, currently developed code or
the latest published version.
If the branching strategy does not differ from the one recommended in sensenet
([link to the branching strategy document]), then the latest published version is in
the main branch, and all the packages are available on nuget.org. The latest developed version is in the develop branch, and some of the packages may not be published yet, so they are only available in the company feed.

## Main steps
1. Create a copy from the working development instance
1. Replace project references with package references
1. Configure database and install initial data.

## Detailed task list
You should have a directory on your machine that contains a cloned version of the
sensenet ecosystem from github. Let's call it [sn]

### Create a copy from the working development instance
1. Create a directory that contains your projects. Let's call it `[dev]`
1. CD `[dev]`
1. Create directory: `SnApp1`
1. Ensure that the `master` or `develop` branch of the `[sn]\sensenet` is active
1. Copy `[sn]\sensenet\src\WebApps\SnWebApplication.Api.Sql.TokenAuth` into `[dev]\SnApp1`
1. CD `[dev]\SnApp1`
1. Rename `SnWebApplication.Api.Sql.TokenAuth` directory to `SnApp1.Web`
1. CD `SnApp1.Web`
1. Rename `SnWebApplication.Api.Sql.TokenAuth.csproj` to `SnApp1.Web.csproj`
1. Delete directories: `App_Data`, `bin`, `obj`
1. Start Visual Studion 2022 (referred to as: [VS])
1. Select `Open a project or solution` from the `Get started` column
1. In the Open Project/Solution dialog select the `SnApp1.Web.csproj` file
and click `Open` button. [VS] initializes the UI, the `SnApp.Web` is in
the `Solution Explorer`.
1.  Select the node `Solution 'SnApp.Web' (1 of 1 project)` in the
`Solution Explorer` (this is the root node of this view).
1. Select the `Save SnApp1.Web.sln As...` in the `File` main menu.
1. In the `Save file As` dialog
    1. Move up one (`[dev]\SnApp1`)
    1. Rewrite the solution name in the textbox `File name:` to `SnApp1.sln`
    1. Click `Save` button
1. Close [VS]
1. Delete directory `[dev]\SnApp1\SnApp1.Web\.vs`

At this point, you have a basic project structure that looks as
 if you had created it yourself.

### Replace project references with package references
1. Reopen the  new solution `[dev]\SnApp1\SnApp1.sln`
1. Open the `SnApp.Web.csproj`
1. Do the following sequence for each `ProjectReference` element:
    1. Lookup the project file and open it.
    1. Copy the value of the `Version` element.
    1. Swithc to your project file (`SnApp.Web.csproj`)
    1. Rewrite the `ProjectReference` to the `PackageReference`
    1. Ensure that the `Include` attribute's value is the package name (no path, no csproj)
    1. Insert a `Value` attribute and paste the copied version value into the attribute value.
1. If all `ProjectReference` is rewritten to `PackageReference`, save the project file
1. Build the solution 

Here is an example for the element rewriting:

```
Before
  <ProjectReference Include="..\..\OData\SenseNet.OData.csproj" />

After
  <PackageReference Include="SenseNet.OData" Version="1.0.1.1" />
```

With this, the integrated code base is complete, now it's time to configure and install the initial data.

### Configure project
The application requires a connectionString for an existing empty database. For historical reasons, the installer section is also required, which contains some data repetition based on the connectionstring.

*Warning*: if the application code will be shared publicly (e.g. github), it is strongly recommended that the following configuration elements be placed in UserSecret instead of appSettings.json. 

*Warning*: Due to the copy, the project file contains the original UserSecretsId, which must be deleted and a new one created instead:
1. Delete the `UserSecretsId` element from the `SnApp.Web.csproj` and save the file.
1. Right click the `SnApp.Web` in the `Solution Explorer` and click the `Manage User Secrets` menuitem.
1. An empty `secrets.json` will be opened.

#### Task list
1. Configure authentication server. There is two options
    1. Use identity server. This is in the `sn-identityserver` github repository. By default use these configuration:
``` json
// [dev]\SnApp1.Web\appsettings.json
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

// sn-identityserver\SenseNet.IdentityServer4.Web
?????
```
    1. Use sensenet authentication server.
``` json
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
```
1. Get an empty database's connectionString from your database administrator. If you create the database yourself:
    1. Open the SQL Server Management Studio (`[SSMS]`).
    1. Select the database server.
    1. In `Object Explorer` open the server treenode and right click on the `Databases`.
    1. Select `New Database...`
    1. Let's the `Database name` is `SnApp1`
    1. Click 'Options'
    1. Select `Recovery model` item `Simple`
    1. Click `Ok` button: the new database apeared in the bottom of the `Object Explorer/Databases`
1. Configure the database connection and installation in the `secret.json` or `appsettings.json`. Ensure the connectionString and the installer config based on the following example:
```
{
  "ConnectionStrings": {
    "SnCrMsSql": "Data Source=SNPC016\\SQL2016;Initial Catalog=SnApp1;Integrated Security=SSPI;Persist Security Info=False"
  },
  "sensenet": {
    "install": {
      "mssql": {
        "Server": "SNPC016\\SQL2016",
        "DatabaseName": "SnApp1"
      }
    }
  }
}
```
Note that using SQL server in a development environment sometimes runs into certification issues, so your connectionstring may need to be supplemented with this: `TrustServerCertificate=True`.


        TODO !!!!!!!!!
        SnApp1.Web/Properties/launchSettings.json
            rename profiles/SnWebApplication.Api.Sql.TokenAuth --> SnApp1
            set profiles/SnApp1/applicationUrl": "https://localhost:44991"
            set profiles/Docker/sslPort": 44991



### Install initial data.

After configuration, you are ready for the first startup, which prepares the database for startup. So press F5 with a firm, strong motion.

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
