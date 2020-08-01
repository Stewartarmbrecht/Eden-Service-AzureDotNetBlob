[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

    Write-EdenBuildInfo "Starting the ContentReactor.MyEdenService.Service.csproj function application." $LoggingPrefix

    $location = Get-Location
    try {
        Set-Location "./Service/"
        func host start -p 7071
    } catch {
        throw $_
    } finally {
        Set-Location $location
    }
