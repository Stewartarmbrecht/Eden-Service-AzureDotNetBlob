[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

Write-EdenBuildInfo "Building the MyEdenSolution.MyEdenService.sln Solution continuously." $LoggingPrefix
dotnet watch --project ./MyEdenSolution.MyEdenService.sln build ./MyEdenSolution.MyEdenService.sln