[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

Write-EdenBuildInfo "Building the ContentReactor.MyEdenService.sln Solution continuously." $LoggingPrefix
dotnet watch --project ./ContentReactor.MyEdenService.sln build ./ContentReactor.MyEdenService.sln