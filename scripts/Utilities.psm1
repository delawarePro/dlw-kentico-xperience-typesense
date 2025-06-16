# Utilities

<#
    .DESCRIPTION
        Returns the web application folder path from the workspace root
#>
function Get-WebProjectPath {
    param(
        [string] $WorkspaceFolder = ".."
    )
    # return Join-Path $WorkspaceFolder "Dlw.Kentico.Xperience.Pierret.AppHost"
    return Join-Path $WorkspaceFolder "examples/DancingGoat"
}

<#
.DESCRIPTION
   Gets the database connection string from the user secrets or appsettings.json file
#>
function Get-ConnectionString {
    param(
        [string] $WorkspaceFolder = ".."
    )

    $projectPath = Get-WebProjectPath ($WorkspaceFolder)

    # Try to get the connection string from user secrets first
    Write-Host "Checking for a connection string user secrets for project: $projectPath"

    $connectionString = dotnet user-secrets list --project $projectPath `
    | Select-String -Pattern "ConnectionStrings:" `
    | ForEach-Object { $_.Line -replace '^ConnectionStrings:CMSConnectionString \= ','' }

    if (-not [string]::IsNullOrEmpty($connectionString)) {
        Write-Host 'Using ConnectionString from user-secrets'

        return $connectionString
    }

    Write-Host 'Unable to find connection string in user secrets.'

    $appSettingsFileNames = 'appsettings.json'

    foreach ($appSettingFileName in $appSettingsFileNames)
    {
        $jsonFilePath = Join-Path $projectPath $appSettingFileName
        Write-Host "Trying to use connectionString in $jsonFilePath"
        if (Test-Path $jsonFilePath)
        {
            $appSettingsJson = Get-Content $jsonFilePath | Out-String | ConvertFrom-Json
            Write-Host "connectionString in $appSettingsJson"
            $connectionString = $appSettingsJson.ConnectionStrings.CMSConnectionString;

            if ($connectionString)
            {
                Write-Host "Using ConnectionString from $appSettingFileName"
                return $connectionString;
            }
        }
    }
    Write-Error "Connection string not found."
    exit 1
}

function Invoke-ExpressionWithRetry {
    param(
        [string]$command,
        [int]$maxRetries = 3,
        [int]$retryDelaySeconds = 2
    )

    for ($i = 1; $i -le $maxRetries; $i++) {
        try {
            Write-Host "Attempting execution (try $i of $maxRetries)..."
            $result = Invoke-ExpressionWithException $command
            return $result
        }
        catch {
            if ($i -eq $maxRetries) {
                throw
            }
            Write-Host "Attempt $i failed with error: $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Verbose "Error details: $($_ | Format-List -Force | Out-String)"
            Write-Host "Waiting $retryDelaySeconds seconds before retry..."
            Start-Sleep -Seconds $retryDelaySeconds
        }
    }
}

<#
.DESCRIPTION
   Ensures the expression successfully exits and throws an exception
   with the failed expression if it does not.
#>
function Invoke-ExpressionWithException {
    param(
        [string]$expression
    )

    Write-Host "$expression"

    Invoke-Expression -Command $expression

    if ($LASTEXITCODE -ne 0) {
        $errorMessage = "[ $expression ] failed`n`n"

        throw $errorMessage
    }
}


#Query that executes a command without returning a dataset.
function Execute-SQL-Command {
    param(
        [string] $ConnectionString,
        [string] $CommandText
    )
    $connection = New-Object system.data.SqlClient.SQLConnection($ConnectionString)

    $connection.Open()
    $command = new-object system.data.sqlclient.sqlcommand($CommandText,$connection)
    $transaction = $connection.BeginTransaction()
    $command.Transaction = $transaction

    try {
        $rowsAffected = $command.ExecuteNonQuery()
        Write-Host 'Command: '$CommandText
        Write-Host 'Rows affected: '$rowsAffected
        $transaction.Commit()
    }
    catch {
        Write-Error $_.Exception.Message
        return $FALSE
    }

    $connection.Close()

    return $TRUE
}

#Query that retrieves a data set
function Execute-SQL-Data-Query {
    param(
        [string] $ConnectionString,
        [string] $CommandText
    )
    $connection = New-Object System.Data.SqlClient.SQLConnection($ConnectionString)

    $connection.Open()

    $command = New-Object System.Data.SqlClient.SqlCommand($CommandText,$connection)
    $dataAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = new-object System.Data.Dataset
    $dataAdapter.Fill($dataset)

    $connection.Close()

    return $dataset
}
function Handle-Error {
    param(
        [string] $Message
    )
    Set-Location -Path $originalLocation
    Write-Error $Message
    Read-Host -Prompt "Press Enter to exit"
    exit 1
}