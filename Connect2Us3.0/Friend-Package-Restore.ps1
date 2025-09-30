# Book2Us Package Restore - Portable Solution for Friend's PC
# This script fixes the EntityFramework package missing issue

Write-Host "=== Book2Us Package Restore Tool ===" -ForegroundColor Cyan
Write-Host "This will fix the EntityFramework 6.5.1 missing package issue" -ForegroundColor Yellow
Write-Host ""

# Step 1: Check current directory
Write-Host "Current directory: $(Get-Location)" -ForegroundColor Green

# Step 2: Look for the project files
Write-Host "Looking for Book2Us project files..." -ForegroundColor Yellow

$projectFiles = @()
$solutionFiles = @()

# Search for project files in current and subdirectories
Get-ChildItem -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $projectFiles += $_
    Write-Host "Found project: $($_.Name) in $($_.DirectoryName)" -ForegroundColor Green
}

# Search for solution files
Get-ChildItem -Filter "*.sln" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $solutionFiles += $_
    Write-Host "Found solution: $($_.Name) in $($_.DirectoryName)" -ForegroundColor Green
}

if ($projectFiles.Count -eq 0) {
    Write-Host "No project files found in current directory!" -ForegroundColor Red
    Write-Host "Please run this script from the Book2Us project folder." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 3: Navigate to the main project directory
$mainProject = $projectFiles[0]
$projectDir = $mainProject.DirectoryName
Set-Location $projectDir

Write-Host "Working in project directory: $projectDir" -ForegroundColor Green

# Step 4: Check for packages folder and EntityFramework
Write-Host "Checking for existing packages..." -ForegroundColor Yellow

$packagesDir = "packages"
$efPackage = "packages\EntityFramework.6.5.1"

if (Test-Path $efPackage) {
    Write-Host "✓ EntityFramework 6.5.1 package already exists!" -ForegroundColor Green
    
    # Verify it's working
    if (Test-Path "$efPackage\lib\net45\EntityFramework.dll") {
        Write-Host "✓ EntityFramework.dll found - package is complete!" -ForegroundColor Green
        Write-Host ""
        Write-Host "=== Package Restore Check Complete ===" -ForegroundColor Cyan
        Write-Host "The EntityFramework package appears to be working correctly." -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "1. Open the solution in Visual Studio" -ForegroundColor White
        Write-Host "2. Build the solution (Ctrl+Shift+B)" -ForegroundColor White
        Write-Host "3. Run the application" -ForegroundColor White
        Read-Host "Press Enter to exit"
        exit 0
    }
}

# Step 5: Try different package restore methods
Write-Host "EntityFramework package missing or incomplete. Attempting restore..." -ForegroundColor Yellow

# Method 1: Try dotnet restore
Write-Host "Method 1: Trying dotnet restore..." -ForegroundColor Yellow
try {
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ dotnet restore successful!" -ForegroundColor Green
        
        # Check if EntityFramework is now available
        if (Test-Path $efPackage) {
            Write-Host "✓ EntityFramework package restored!" -ForegroundColor Green
            $success = $true
        }
    }
} catch {
    Write-Host "dotnet restore not available or failed" -ForegroundColor Red
}

# Method 2: Try to find and use NuGet CLI
if (-not $success) {
    Write-Host "Method 2: Looking for NuGet CLI..." -ForegroundColor Yellow
    
    # Check if nuget.exe exists in current directory
    if (Test-Path "nuget.exe") {
        Write-Host "Found nuget.exe in current directory" -ForegroundColor Green
        $nugetPath = ".\nuget.exe"
    } else {
        # Try to find nuget in PATH
        try {
            $nugetPath = (Get-Command nuget).Source
            Write-Host "Found NuGet CLI at: $nugetPath" -ForegroundColor Green
        } catch {
            Write-Host "NuGet CLI not found. Downloading..." -ForegroundColor Yellow
            
            # Download NuGet CLI
            try {
                $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
                Invoke-WebRequest -Uri $nugetUrl -OutFile "nuget.exe" -UseBasicParsing
                $nugetPath = ".\nuget.exe"
                Write-Host "✓ NuGet CLI downloaded successfully!" -ForegroundColor Green
            } catch {
                Write-Host "Failed to download NuGet CLI" -ForegroundColor Red
                $nugetPath = $null
            }
        }
    }
    
    if ($nugetPath) {
        Write-Host "Attempting NuGet restore..." -ForegroundColor Yellow
        try {
            & $nugetPath restore book2us.sln -NonInteractive
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ NuGet restore successful!" -ForegroundColor Green
                
                if (Test-Path $efPackage) {
                    Write-Host "✓ EntityFramework package restored!" -ForegroundColor Green
                    $success = $true
                }
            }
        } catch {
            Write-Host "NuGet restore failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Method 3: Manual package download
if (-not $success) {
    Write-Host "Method 3: Manual EntityFramework package download..." -ForegroundColor Yellow
    
    try {
        # Create packages directory
        if (-not (Test-Path $packagesDir)) {
            New-Item -ItemType Directory -Path $packagesDir | Out-Null
        }
        
        # Download EntityFramework package
        $efUrl = "https://www.nuget.org/api/v2/package/EntityFramework/6.5.1"
        $tempFile = "$packagesDir\EntityFramework.6.5.1.nupkg"
        
        Write-Host "Downloading EntityFramework 6.5.1..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $efUrl -OutFile $tempFile -UseBasicParsing
        
        # Extract package
        $efTargetDir = "$packagesDir\EntityFramework.6.5.1"
        if (Test-Path $efTargetDir) {
            Remove-Item -Path $efTargetDir -Recurse -Force
        }
        
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($tempFile, $efTargetDir)
        
        # Clean up
        Remove-Item $tempFile -Force
        
        Write-Host "✓ EntityFramework package downloaded and extracted!" -ForegroundColor Green
        $success = $true
        
    } catch {
        Write-Host "Manual download failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 6: Final verification
Write-Host ""
Write-Host "=== Verification ===" -ForegroundColor Cyan

if ($success -and (Test-Path $efPackage)) {
    Write-Host "✓ EntityFramework package restoration successful!" -ForegroundColor Green
    
    # Check for required DLL
    if (Test-Path "$efPackage\lib\net45\EntityFramework.dll") {
        Write-Host "✓ EntityFramework.dll found!" -ForegroundColor Green
        
        Write-Host ""
        Write-Host "=== Next Steps ===" -ForegroundColor Cyan
        Write-Host "1. Open Visual Studio" -ForegroundColor Yellow
        Write-Host "2. Open the Book2Us solution (book2us.sln)" -ForegroundColor Yellow
        Write-Host "3. Build the solution (Ctrl+Shift+B or Build → Build Solution)" -ForegroundColor Yellow
        Write-Host "4. Run the application (F5 or Debug → Start Debugging)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "The EntityFramework error should now be resolved!" -ForegroundColor Green
        
    } else {
        Write-Host "⚠ EntityFramework package exists but DLL not found" -ForegroundColor Yellow
        Write-Host "The package may be incomplete. Try building in Visual Studio to trigger full restore." -ForegroundColor Yellow
    }
    
} else {
    Write-Host "✗ EntityFramework package restoration failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "=== Manual Steps ===" -ForegroundColor Cyan
    Write-Host "1. Open Visual Studio" -ForegroundColor Yellow
    Write-Host "2. Go to Tools → NuGet Package Manager → Package Manager Console" -ForegroundColor Yellow
    Write-Host "3. Run: Install-Package EntityFramework -Version 6.5.1" -ForegroundColor Yellow
    Write-Host "4. Or: Update-Package -Reinstall" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "If that doesn't work, copy the packages folder from a working installation." -ForegroundColor Yellow
}

Write-Host ""
Read-Host "Press Enter to exit"