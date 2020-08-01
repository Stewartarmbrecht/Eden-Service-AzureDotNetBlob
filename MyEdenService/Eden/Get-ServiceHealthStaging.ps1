[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)
try {
    Write-EdenBuildInfo "Calling service health check at 'https://$($EdenEnvConfig.EnvironmentName)-MyEdenService-staging.azurewebsites.net/api/healthcheck?userId=developer98765@test.com'." $LoggingPrefix
    $response = Invoke-RestMethod -URI "https://$($EdenEnvConfig.EnvironmentName)-MyEdenService-staging.azurewebsites.net/api/healthcheck?userId=developer98765@test.com"
    Write-Host ""
    Write-Host $response -ForegroundColor Blue
    Write-Host ""
} catch {
    $message = $_.Exception.Message
    Write-EdenBuildError "Failed to execute health check: '$message'." $LoggingPrefix
    return $FALSE
}