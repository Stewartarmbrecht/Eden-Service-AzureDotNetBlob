[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

Write-EdenBuildInfo "Setting the target url for the features test to: 'http://localhost:7071/api/MyEdenService'." $LoggingPrefix
$Env:FeaturesUrl = "http://localhost:7071/api/MyEdenService"

Write-EdenBuildInfo "Running the tests in the Serivce.Tests/ContentReactor.MyEdenService.Service.Tests.csproj project that are tagged as Features." $LoggingPrefix
dotnet test ./Service.Tests/ContentReactor.MyEdenService.Service.Tests.csproj --filter TestCategory=Features
