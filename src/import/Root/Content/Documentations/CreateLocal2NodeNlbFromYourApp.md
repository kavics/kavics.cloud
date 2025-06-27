# -- under construction --

# Create two node NLB from your WebApp's binaries
This doc describes how to make a debuggable two node NLB simulation from a sensenet based webapplication.

## Prepare base app
Make sure that the your webapp has launched successfully at least once. This guarantees that the database and local index are ready to work. This app will be the first node of the NLB simulation.
## Extend the project file
Add the required packages to the project file
```powershell
CD <yourapp>
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

### Extend configuration
Insert the followings into the `sensenet` section of the `appsettings.json`
```
"rabbitmq": {
  "ServiceUrl": ""
},
"security": {
  "rabbitmq": {
    "ServiceUrl": ""
  }
},
"tracing": {
  "StartupTraceCategories": "Messaging"
}
```
### secrets.json
Complete the configuration with the user-specific data in the user secrets
```
"rabbitmq": {
  "ServiceUrl": "amqp://guest:guest@localhost:5672",
  "MessageExchange": "local-nlb-general"
},
"security": {
  "rabbitmq": {
    "ServiceUrl": "amqp://guest:guest@localhost:5672",
    "MessageExchange": "local-nlb-security"
  }
}
```

## Create the second NLB node
### Create the second node's skeleton
1. Create a folder for the NLB e.g. `D:\_NlbTest\yourapp-bin` will now be called `<nlbroot>` or `\`
1. Create a subfolder under `<nlbroot>`: `node-2`

### Copy
1. Navigate to the your webapp's directory that contains the `bin`, `App_Data`and more subdirectories. 
1. Copy the content of the the `<yourapp>\bin\Debug\net8.0` into the `<nlbroot>\node-2`.
1. Copy the `<yourapp>\App_Data` into the `<nlbroot>\node-2`.
1. Empty the `IndexBackup` and `Logs` directories under the `<nlbroot>\node-2\App_Data`
1. Check if `<nlbroot>\node-2\App_Data\LocalIndex` contains a subdirectory that contains a folder like this: 20250627004259 with a valid Lucene index. If not, start over, because the original app has never run properly.

### Complete the configuration
By default, the `appsettings.json` does not contain user-specific values, this data is located in the project's `secret.json` file.

Copy values from the original app's `secret.json` and write directly into the `<nlbroot>\node-2\appsettings.json`.
```json
  "ConnectionStrings": {
    "SnCrMsSql": ""

  "sensenet": {
    "ApiKeys": {
      "HealthCheckerUser": ""

    "rabbitmq": {
      "ServiceUrl": "",
      "MessageExchange": ""

    "security": {
      "rabbitmq": {
        "ServiceUrl": "",
        "MessageExchange": ""
```

## Create a start script
To make it easier to run, create a startup script for node 2:
`<nlbroot>\RUN-Node-2.ps1` that contains something like this:
``` powershell
$host.ui.RawUI.WindowTitle = "<yourapp> NODE 2 https://localhost:35323"
cd node-2
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="https://localhost:35323"
dotnet <yourapp>.dll
```
Note that the app-url, and location of the script and start directory are arbitrary.

## Start every nodes
- Start the original app as **node-1** from the Visual Studio in debug mode (F5)
- Run the **node-2** starter script: double click on the `<nlbroot>\RUN-Node-2.ps1`
- If **AdminUI** is needed run the following script
```powershell
CD <yoursources>\sn-identityserver\src\SenseNet.IdentityServer4.Web
dotnet run
```
