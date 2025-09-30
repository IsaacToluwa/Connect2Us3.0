# Book2Us NuGet Package Restore Script
# This script fixes the EntityFramework package restore issue

param(
    [string]$ProjectPath = ".",
    [switch]$Force = $false
)

Write-Host "=== Book2Us NuGet Package Restore ===" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow

# Function to check if a command exists
function Test-Command {
    param([string]$Command)
    try {
        Get-Command $Command -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# Function to restore packages using different methods
function Restore-NuGetPackages {
    Write-Host "Attempting to restore NuGet packages..." -ForegroundColor Yellow
    
    # Method 1: Try dotnet restore (for .NET Core/5+ projects)
    if (Test-Command "dotnet") {
        Write-Host "Trying dotnet restore..." -ForegroundColor Green
        try {
            dotnet restore
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Packages restored successfully using dotnet!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            Write-Host "dotnet restore failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    # Method 2: Try NuGet CLI
    if (Test-Command "nuget") {
        Write-Host "Trying nuget restore..." -ForegroundColor Green
        try {
            nuget restore book2us.sln
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Packages restored successfully using NuGet CLI!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            Write-Host "nuget restore failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    # Method 3: Try MSBuild
    if (Test-Command "msbuild") {
        Write-Host "Trying msbuild restore..." -ForegroundColor Green
        try {
            msbuild book2us.sln /t:Restore /p:RestorePackagesConfig=true
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Packages restored successfully using MSBuild!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            Write-Host "msbuild restore failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    # Method 4: Manual package download for EntityFramework
    Write-Host "Attempting manual EntityFramework package restoration..." -ForegroundColor Yellow
    
    try {
        # Create packages directory if it doesn't exist
        $packagesDir = "packages"
        if (-not (Test-Path $packagesDir)) {
            New-Item -ItemType Directory -Path $packagesDir | Out-Null
        }
        
        # Check if EntityFramework package exists
        $efPackagePath = "packages\EntityFramework.6.5.1"
        if ((Test-Path $efPackagePath) -and -not $Force) {
            Write-Host "✓ EntityFramework 6.5.1 package already exists!" -ForegroundColor Green
            return $true
        }
        
        # Download EntityFramework package manually
        Write-Host "Downloading EntityFramework 6.5.1 package..." -ForegroundColor Yellow
        
        $nugetUrl = "https://www.nuget.org/api/v2/package/EntityFramework/6.5.1"
        $tempFile = "EntityFramework.6.5.1.nupkg"
        
        # Download the package
        try {
            Invoke-WebRequest -Uri $nugetUrl -OutFile $tempFile -UseBasicParsing
            Write-Host "Downloaded EntityFramework package successfully!" -ForegroundColor Green
            
            # Extract the package
            if (Test-Path $efPackagePath) {
                Remove-Item -Path $efPackagePath -Recurse -Force
            }
            
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            [System.IO.Compression.ZipFile]::ExtractToDirectory($tempFile, $efPackagePath)
            
            # Clean up
            Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
            
            Write-Host "✓ EntityFramework package extracted successfully!" -ForegroundColor Green
            return $true
        }
        catch {
            Write-Host "Failed to download EntityFramework: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "Manual package restoration failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    return $false
}

# Function to verify package restoration
function Test-PackageRestoration {
    Write-Host "Verifying package restoration..." -ForegroundColor Yellow
    
    $requiredPackages = @(
        "EntityFramework.6.5.1",
        "Microsoft.AspNet.Mvc.5.2.9",
        "Microsoft.AspNet.Razor.3.2.9",
        "Microsoft.AspNet.WebPages.3.2.9"
    )
    
    $allFound = $true
    foreach ($package in $requiredPackages) {
        $packagePath = "packages\$package"
        if (Test-Path $packagePath) {
            Write-Host "✓ Found: $package" -ForegroundColor Green
        }
        else {
            Write-Host "✗ Missing: $package" -ForegroundColor Red
            $allFound = $false
        }
    }
    
    return $allFound
}

# Function to install NuGet CLI if needed
function Install-NuGetCLI {
    Write-Host "NuGet CLI not found. Would you like to install it? (Y/N)" -ForegroundColor Yellow
    $response = Read-Host
    
    if ($response -eq 'Y' -or $response -eq 'y') {
        try {
            $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
            $nugetPath = ".\nuget.exe"
            
            Write-Host "Downloading NuGet CLI..." -ForegroundColor Yellow
            Invoke-WebRequest -Uri $nugetUrl -OutFile $nugetPath -UseBasicParsing
            
            # Add current directory to PATH temporarily
            $env:PATH = ".;$env:PATH"
            
            Write-Host "✓ NuGet CLI installed successfully!" -ForegroundColor Green
            return $true
        }
        catch {
            Write-Host "Failed to install NuGet CLI: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
    return $false
}

# Main execution
Write-Host "Starting package restoration process..." -ForegroundColor Cyan

# Change to project directory
Set-Location $ProjectPath

# Check if packages.config exists
if (-not (Test-Path "packages.config")) {
    Write-Host "packages.config not found in current directory. Looking for project files..." -ForegroundColor Yellow
    
    # Look for .csproj files
    $csprojFiles = Get-ChildItem -Filter "*.csproj" -Recurse
    if ($csprojFiles.Count -gt 0) {
        Write-Host "Found project file: $($csprojFiles[0].FullName)" -ForegroundColor Green
        Set-Location $csprojFiles[0].DirectoryName
    }
    else {
        Write-Host "No project files found. Please run this script from your project directory." -ForegroundColor Red
        exit 1
    }
}

# Try to restore packages
$success = Restore-NuGetPackages

if (-not $success) {
    Write-Host "Standard package restore failed. Trying alternative methods..." -ForegroundColor Yellow
    
    # Install NuGet CLI if not present
    if (-not (Test-Command "nuget")) {
        Install-NuGetCLI
    }
    
    # Try again with newly installed NuGet CLI
    if (Test-Command "nuget") {
        $success = Restore-NuGetPackages
    }
}

# Final verification
if ($success) {
    Write-Host "Package restoration completed!" -ForegroundColor Green
    
    if (Test-PackageRestoration) {
        Write-Host "✓ All required packages are present!" -ForegroundColor Green
        Write-Host "" -ForegroundColor White
        Write-Host "=== Next Steps ===" -ForegroundColor Cyan
        Write-Host "1. Build your solution in Visual Studio" -ForegroundColor Yellow
        Write-Host "2. Or run: dotnet build" -ForegroundColor Yellow
        Write-Host "3. Your application should now work without the EntityFramework error!" -ForegroundColor Green
    }
    else {
        Write-Host "⚠ Some packages are still missing. Please check the output above." -ForegroundColor Yellow
    }
}
else {
    Write-Host "✗ Package restoration failed. Please try the following:" -ForegroundColor Red
    Write-Host "1. Install Visual Studio with .NET workload" -ForegroundColor Yellow
    Write-Host "2. Install NuGet CLI manually from: https://www.nuget.org/downloads" -ForegroundColor Yellow
    Write-Host "3. Run 'nuget restore' from your project directory" -ForegroundColor Yellow
    Write-Host "4. Or manually copy the packages folder from a working installation" -ForegroundColor Yellow
}

Write-Host "" -ForegroundColor White
Write-Host "=== Package Restore Complete ===" -ForegroundColor Cyan