[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

    Write-EdenBuildInfo "Building the ContentReactor.MyEdenService.sln Solution." $LoggingPrefix
    dotnet build ./ContentReactor.MyEdenService.sln
    Write-EdenBuildInfo "Finished building the ContentReactor.MyEdenService.sln Solution." $LoggingPrefix
