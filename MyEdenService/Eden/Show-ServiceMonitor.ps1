[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)

Write-EdenBuildInfo "Connecting to azure tenant." $loggingPrefix

$pscredential = New-Object System.Management.Automation.PSCredential($EdenEnvConfig.ServicePrincipalId, (ConvertTo-SecureString $EdenEnvConfig.ServicePrincipalPassword))
Connect-AzAccount -ServicePrincipal -Credential $pscredential -Tenant $EdenEnvConfig.TenantId | Write-Verbose

Write-EdenBuildInfo "Generating link to the service monitoring dashboard in Azure." $loggingPrefix
Write-Host "" -ForegroundColor Blue
Write-Host "https://portal.azure.com/#@boundbybetter.com/resource/subscriptions/$((Get-AzSubscription).Id)/resourceGroups/$($EdenEnvConfig.EnvironmentName)-MyEdenService/providers/Microsoft.Insights/components/$($EdenEnvConfig.EnvironmentName)-MyEdenService-ai/applicationMap" -ForegroundColor Blue
Write-Host "" -ForegroundColor Blue
