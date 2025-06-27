# -- under construction --

# CASE STUDY: Create 2 node NLB from a sensenet WebApp
## Source test app: **SnWebApplication.Api.Sql.TokenAuth**

This doc describes...

## Create NLB skeleton
1. Create a folder for the NLB e.g. `D:\_NlbTest\sensenet` will now be called `nlb-root` or `\`
1. Create 2 subfolders under `nlb-root`: `node-1` and `node-2`

## Prepare the first NLB node
### Copy
1. Copy the source app webfolder's content into the `\node-1`. My source is `D:\dev\github\sensenet\src\WebApps\SnWebApplication.Api.Sql.TokenAuth`. 
2. The App_Data need to contain the `LocalIndex` with the valid lucene structure. The `IndexBackup` and `Logs` need to be empty.
3. Delete the `bin`, `obj` and `install-services-core` directories

### Modify the project file
Open the `SnWebApplication.Api.Sql.TokenAuth.csproj`, and remove all project references. Do not leave empty `<ItemGroup>` element.
```xml
<ProjectReference Include="..\..\ContentRepository.MsSql\SenseNet.ContentRepository.MsSql.csproj" />
<ProjectReference Include="..\..\OData\SenseNet.OData.csproj" />
<ProjectReference Include="..\..\OpenApi\SenseNet.OpenApi.csproj" />
<ProjectReference Include="..\..\Services.Core.Install\SenseNet.Services.Core.Install.csproj" />
<ProjectReference Include="..\..\Services.Core\SenseNet.Services.Core.csproj" />
<ProjectReference Include="..\..\Services.Wopi\SenseNet.Services.Wopi.csproj" />
<ProjectReference Include="..\..\WebHooks\SenseNet.WebHooks.csproj" />
```
After that add the removed packages from nuget.
```powershell
dotnet add package SenseNet.ContentRepository.MsSql
dotnet add package SenseNet.OData
dotnet add package SenseNet.OpenApi
dotnet add package SenseNet.Services.Core.Install
dotnet add package SenseNet.Services.Core
dotnet add package SenseNet.Services.Wopi
dotnet add package SenseNet.WebHooks
```
## Extend the project file
Add the required packages to the project file
```powershell
CD <nlb-root>\node-1
dotnet add package SenseNet.Messaging.RabbitMQ
dotnet add package SenseNet.Security.Messaging.RabbitMQ
```
### Extend Startup.cs
Add this line to the *usings*
```csharp
using SenseNet.Security.Messaging.RabbitMQ;
```
Insert the followings before the `.AddSenseNetOData()` line.

```csharp
.Configure<RabbitMqOptions>(Configuration.GetSection("sensenet:security:rabbitmq"))
.AddRabbitMqSecurityMessageProvider()
.AddRabbitMqMessageProvider(configureRabbitMq: options =>
{
    Configuration.GetSection("sensenet:rabbitmq").Bind(options);
})
```

## Extend configuration
Insert the followings into the `sensenet` section of the `appsettings.json`
```json
  "sensenet": {
    "rabbitmq": {
      "ServiceUrl": "amqp://guest:guest@localhost:5672",
      "MessageExchange": "local-nlb-general"
    },
    "security": {
      "rabbitmq": {
        "ServiceUrl": "amqp://guest:guest@localhost:5672",
        "MessageExchange": "local-nlb-security"
      }
    },
```
Copy values from the secret and write directly into these values
```json
  "ConnectionStrings": {
    "SnCrMsSql": ""

  "sensenet": {
    "ApiKeys": {
      "HealthCheckerUser": ""
```
## Modify launchSettings.json
- Open the `nlb-root\node-1\Properties\launchSettings.json`
- Rewrite the start url of the webapp
  - `"applicationUrl": "https://localhost:45101"`

## Copy nlb node
Copy the content of the  `nlb-root\node-1\` under the `nlb-root\node-2\`

## Modify launchSettings.json on the second nlb node
- Open the `nlb-root\node-2\Properties\launchSettings.json`
- Rewrite the start url of the webapp
  - `"applicationUrl": "https://localhost:45102"`

## Start every nodes
Start powershell #1
```powershell
CD nlb-root\node-1
dotnet run
```
Start powershell #2
```powershell
CD nlb-root\node-2
dotnet run
```
If AdminUI is needed
```powershell
CD D:\dev\github\sn-identityserver\src\SenseNet.IdentityServer4.Web
dotnet run
```

`\sn-identityserver\src\SenseNet.IdentityServer4.Web\Properties\launchSettings.json`
https://admin.test.sensenet.com/?repoUrl=https%3A%2F%2Flocalhost%3A45101
