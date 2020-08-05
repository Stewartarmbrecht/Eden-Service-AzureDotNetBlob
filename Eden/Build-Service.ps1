[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

    Write-EdenBuildInfo "Building the MyEdenSolution.MyEdenService.sln Solution." $LoggingPrefix
    dotnet build ./MyEdenSolution.MyEdenService.sln
    Write-EdenBuildInfo "Finished building the MyEdenSolution.MyEdenService.sln Solution." $LoggingPrefix
