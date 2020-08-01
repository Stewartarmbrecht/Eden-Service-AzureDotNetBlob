[CmdletBinding()]
param(
    $EdenEnvConfig,
    [String] $LoggingPrefix
)
    Write-EdenBuildInfo "Testing the ContentReactor.MyEdenService.sln Solution continuously using the 'Service.Tests/ContentReactor.MyEdenService.Service.Tests'." $LoggingPrefix

    $testingActions = {
        $VerbosePreference = "Continue"
        Write-Host ""
        Write-EdenBuildInfo "Testing the ContentReactor.MyEdenService.sln Solution using the 'Service.Tests/ContentReactor.MyEdenService.Service.Tests'." $LoggingPrefix
        dotnet test ./Service.Tests/ContentReactor.MyEdenService.Service.Tests.csproj `
            --logger "trx;logFileName=testResults.trx" `
            --filter TestCategory!=Features `
            /p:CollectCoverage=true `
            /p:CoverletOutput=TestResults/ `
            /p:CoverletOutputFormat=lcov `
            /p:Include=`"[ContentReactor.MyEdenService.Service*]*`" `
            /p:Threshold=80 `
            /p:ThresholdType=line `
            /p:ThresholdStat=total | Write-Host
        Write-EdenBuildInfo "Finished testing the ContentReactor.MyEdenService.sln Solution using the 'Service.Tests/ContentReactor.MyEdenService.Service.Tests'." $LoggingPrefix
    
        Write-EdenBuildInfo "Generating test results report to './Service.Tests/TestResults/allure'" $LoggingPrefix
        allure generate ./Service.Tests/TestResults/ -o ./Service.Tests/TestResults/Allure --clean | Write-Host
        Write-EdenBuildInfo "Finished generating test results report to './Service.Tests/TestResults/allure'" $LoggingPrefix
    
        Write-EdenBuildInfo "Generating code coverage reports to './Service.Tests/TestResults/coveragereport'" $LoggingPrefix
        reportgenerator "-reports:./Service.Tests/TestResults/Coverage.info" "-targetdir:Service.Tests/TestResults/coveragereport" -reporttypes:Html | Write-Host
        Write-EdenBuildInfo "Finished generating code coverage reports to './Service.Tests/TestResults/coveragereport'" $LoggingPrefix    

        # Write-EdenBuildInfo "Sleeping err working..." $LoggingPrefix    
        # Start-Sleep 2
        Write-EdenBuildInfo "" $LoggingPrefix    
        Write-EdenBuildInfo "Back to watching for changes..." $LoggingPrefix    
        Write-EdenBuildInfo "" $LoggingPrefix    
    }

    Watch-EdenFolder `
        -Folder "." `
        -Filter "*.cs" `
        -Action $testingActions `
        -LoggingPrefix $LoggingPrefix `
        -Verbose