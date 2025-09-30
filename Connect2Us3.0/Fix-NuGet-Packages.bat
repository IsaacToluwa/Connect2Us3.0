@echo off
REM Book2Us NuGet Package Restore Batch Script
REM This batch file helps restore missing NuGet packages

echo === Book2Us NuGet Package Restore ===
echo.

REM Check if dotnet is available
where dotnet >nul 2>nul
if %errorlevel% == 0 (
    echo Found dotnet CLI, attempting restore...
    dotnet restore
    if %errorlevel% == 0 (
        echo ✓ Package restore successful!
        goto :verify
    ) else (
        echo dotnet restore failed, trying alternative methods...
    )
)

REM Check if NuGet CLI is available
where nuget >nul 2>nul
if %errorlevel% == 0 (
    echo Found NuGet CLI, attempting restore...
    nuget restore book2us.sln
    if %errorlevel% == 0 (
        echo ✓ Package restore successful!
        goto :verify
    ) else (
        echo NuGet restore failed...
    )
) else (
    echo NuGet CLI not found. Downloading...
    
    REM Download NuGet CLI
    echo Downloading NuGet CLI...
    powershell -Command "Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile 'nuget.exe'"
    
    if exist nuget.exe (
        echo ✓ NuGet CLI downloaded successfully!
        echo Attempting package restore...
        nuget restore book2us.sln
        if %errorlevel% == 0 (
            echo ✓ Package restore successful!
            goto :verify
        )
    ) else (
        echo ✗ Failed to download NuGet CLI
    )
)

REM Manual EntityFramework package download
echo Attempting manual EntityFramework package restoration...
powershell -ExecutionPolicy Bypass -File "Fix-NuGet-Packages.ps1" -Force

:verify
echo.
echo === Verification ===
echo Checking for EntityFramework package...
if exist "packages\EntityFramework.6.5.1" (
    echo ✓ EntityFramework 6.5.1 package found!
    echo.
    echo === Next Steps ===
    echo 1. Open your solution in Visual Studio
    echo 2. Build the solution (F6 or Build -^> Build Solution)
    echo 3. The EntityFramework error should be resolved!
) else (
    echo ✗ EntityFramework package still missing
    echo.
    echo === Manual Solution ===
    echo 1. Open Visual Studio
    echo 2. Go to Tools -^> NuGet Package Manager -^> Package Manager Console
    echo 3. Run: Update-Package -Reinstall
    echo 4. Or: Install-Package EntityFramework -Version 6.5.1
)

echo.
echo === Package Restore Complete ===
pause