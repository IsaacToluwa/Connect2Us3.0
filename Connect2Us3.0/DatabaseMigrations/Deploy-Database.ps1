# Book2Us Database Deployment Script
# This PowerShell script helps deploy the database to different environments
# Usage: .\Deploy-Database.ps1 -Environment "Production" -Server "YOUR_SERVER" -Database "Book2UsDB" -Username "YOUR_USERNAME" -Password "YOUR_PASSWORD"

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [string]$Server,
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "Book2UsDB",
    
    [Parameter(Mandatory=$false)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [string]$Password,
    
    [Parameter(Mandatory=$false)]
    [switch]$CreateDatabase = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SeedData = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$ScriptPath = ".\DatabaseMigrations"
)

# Function to write colored output
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Function to test SQL connection
function Test-SqlConnection {
    param([string]$ConnectionString)
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        $connection.Close()
        return $true
    }
    catch {
        Write-ColorOutput "Failed to connect to SQL Server: $($_.Exception.Message)" "Red"
        return $false
    }
}

# Function to execute SQL script
function Invoke-SqlScript {
    param(
        [string]$ConnectionString,
        [string]$ScriptFile
    )
    
    try {
        Write-ColorOutput "Executing script: $ScriptFile" "Yellow"
        
        $scriptContent = Get-Content $ScriptFile -Raw
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $command = New-Object System.Data.SqlClient.SqlCommand($scriptContent, $connection)
        
        $connection.Open()
        $command.ExecuteNonQuery()
        $connection.Close()
        
        Write-ColorOutput "Script executed successfully: $ScriptFile" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "Error executing script $ScriptFile : $($_.Exception.Message)" "Red"
        return $false
    }
}

# Main deployment function
function Deploy-Database {
    Write-ColorOutput "=== Book2Us Database Deployment ===" "Cyan"
    Write-ColorOutput "Environment: $Environment" "Cyan"
    Write-ColorOutput "Server: $Server" "Cyan"
    Write-ColorOutput "Database: $Database" "Cyan"
    Write-ColorOutput "=================================" "Cyan"
    
    # Build connection string
    if ($Username -and $Password) {
        $ConnectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;MultipleActiveResultSets=True;"
    }
    else {
        $ConnectionString = "Server=$Server;Database=$Database;Integrated Security=True;MultipleActiveResultSets=True;"
    }
    
    # Test connection
    Write-ColorOutput "Testing SQL Server connection..." "Yellow"
    if (-not (Test-SqlConnection $ConnectionString)) {
        Write-ColorOutput "Cannot connect to SQL Server. Deployment aborted." "Red"
        return $false
    }
    
    Write-ColorOutput "Connection successful!" "Green"
    
    # Create database if requested
    if ($CreateDatabase) {
        Write-ColorOutput "Creating database..." "Yellow"
        $masterConnectionString = $ConnectionString -replace "Database=$Database", "Database=master"
        
        $createDbScript = @"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$Database')
BEGIN
    CREATE DATABASE [$Database]
    PRINT 'Database $Database created successfully'
END
ELSE
BEGIN
    PRINT 'Database $Database already exists'
END
"@
        
        try {
            $connection = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
            $command = New-Object System.Data.SqlClient.SqlCommand($createDbScript, $connection)
            $connection.Open()
            $command.ExecuteNonQuery()
            $connection.Close()
            Write-ColorOutput "Database created/verified successfully" "Green"
        }
        catch {
            Write-ColorOutput "Error creating database: $($_.Exception.Message)" "Red"
            return $false
        }
    }
    
    # Execute schema creation script
    $schemaScript = Join-Path $ScriptPath "01_CreateDatabaseSchema.sql"
    if (Test-Path $schemaScript) {
        Write-ColorOutput "Creating database schema..." "Yellow"
        if (-not (Invoke-SqlScript $ConnectionString $schemaScript)) {
            return $false
        }
    }
    else {
        Write-ColorOutput "Schema script not found: $schemaScript" "Red"
        return $false
    }
    
    # Execute seed data script if requested
    if ($SeedData) {
        $seedScript = Join-Path $ScriptPath "02_SeedData.sql"
        if (Test-Path $seedScript) {
            Write-ColorOutput "Inserting seed data..." "Yellow"
            if (-not (Invoke-SqlScript $ConnectionString $seedScript)) {
                Write-ColorOutput "Warning: Seed data script failed, but continuing deployment" "Yellow"
            }
        }
        else {
            Write-ColorOutput "Seed data script not found: $seedScript" "Yellow"
        }
    }
    
    # Verify deployment
    Write-ColorOutput "Verifying deployment..." "Yellow"
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $command = New-Object System.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM [ApplicationUsers]", $connection)
        $connection.Open()
        $userCount = $command.ExecuteScalar()
        $connection.Close()
        
        Write-ColorOutput "Deployment verification successful! Found $userCount users in database." "Green"
    }
    catch {
        Write-ColorOutput "Error verifying deployment: $($_.Exception.Message)" "Red"
        return $false
    }
    
    Write-ColorOutput "=================================" "Cyan"
    Write-ColorOutput "Database deployment completed successfully!" "Green"
    Write-ColorOutput "Environment: $Environment" "Green"
    Write-ColorOutput "Database: $Database on $Server" "Green"
    Write-ColorOutput "=================================" "Cyan"
    
    return $true
}

# Environment-specific configurations
switch ($Environment) {
    "Development" {
        Write-ColorOutput "Using Development configuration" "Green"
        # Development-specific settings
        if (-not $Username) { $Username = "dev_user" }
        if (-not $Database) { $Database = "Book2UsDB_Dev" }
    }
    
    "Staging" {
        Write-ColorOutput "Using Staging configuration" "Green"
        # Staging-specific settings
        if (-not $Username) { $Username = "staging_user" }
        if (-not $Database) { $Database = "Book2UsDB_Staging" }
    }
    
    "Production" {
        Write-ColorOutput "Using Production configuration" "Green"
        # Production-specific settings
        if (-not $Username) { 
            Write-ColorOutput "Username is required for Production environment" "Red"
            exit 1
        }
        if (-not $Password) { 
            Write-ColorOutput "Password is required for Production environment" "Red"
            exit 1
        }
        if (-not $Database) { $Database = "Book2UsDB_Prod" }
    }
}

# Run deployment
$success = Deploy-Database

if ($success) {
    exit 0
}
else {
    exit 1
}