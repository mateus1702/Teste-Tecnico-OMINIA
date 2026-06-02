param(
    [string]$Configuration = "Release"
)

Write-Host "Running tests with coverage..."
Push-Location $PSScriptRoot
try {
    dotnet test .\Ambev.DeveloperEvaluation.sln `
        --configuration $Configuration `
        /p:CollectCoverage=true `
        /p:CoverletOutputFormat=cobertura `
        /p:CoverletOutput=./TestResults/coverage.cobertura.xml `
        /p:Exclude="[*]*.Program,[*]*.Startup,[*]*.Migrations.*"

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "Coverage report generated at template/backend/TestResults/"
}
finally {
    Pop-Location
}
