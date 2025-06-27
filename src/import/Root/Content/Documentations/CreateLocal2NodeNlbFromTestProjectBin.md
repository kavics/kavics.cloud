# Create two node NLB from a sensenet WebApp's binaries
This doc describes how to make a **debuggable** two node NLB simulation from the test app prepared for this purpose.

## Prepare base app
Make sure the `SnWebApplication.Api.Sql.TokenAuth.NLB` app has launched successfully at least once. This guarantees that the database and local index are ready to work. This app will be the first node of the NLB simulation.

## Create the second NLB node
### Create the second node's skeleton
1. Create a folder for the NLB e.g. `D:\_NlbTest\sensenet-bin` will now be called `<nlbroot>` or `\`
1. Create a subfolder under `<nlbroot>`: `node-2`

### Copy
1. Navigate to the source webapp's directory: 
`...\sensenet\src\WebApps\SnWebApplication.Api.Sql.TokenAuth.NLB`
1. Copy the content of the the `bin\Debug\net8.0` into the `<nlbroot>\node-2`.
`D:\dev\github\sensenet\src\WebApps\SnWebApplication.Api.Sql.TokenAuth.NLB\bin\Debug\net8.0`. 
1. Copy the `App_Data` into the `<nlbroot>\node-2`.
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
$host.ui.RawUI.WindowTitle = "SnWebApp.Api.Sql.TokenAuth NODE 2 https://localhost:44373"
cd node-2
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="https://localhost:44373"
dotnet SnWebApplication.Api.Sql.TokenAuth.NLB.dll
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
