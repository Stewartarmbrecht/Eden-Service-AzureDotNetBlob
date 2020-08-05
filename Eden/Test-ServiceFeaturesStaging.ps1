[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

$apiName = "$($EdenEnvConfig.EnvironmentName)-MyEdenService".ToLower()
Write-EdenBuildInfo "Setting the target url for the features test to: 'https://$apiName-staging.azurewebsites.net/api/MyEdenService'." $LoggingPrefix
$Env:FeaturesUrl = "https://$apiName-staging.azurewebsites.net/api/MyEdenService"

Write-EdenBuildInfo "Running the tests in the Serivce.Tests/MyEdenSolution.MyEdenService.Service.Tests.csproj project that are tagged as Features." $LoggingPrefix
dotnet test ./Service.Tests/MyEdenSolution.MyEdenService.Service.Tests.csproj --filter TestCategory=Features
