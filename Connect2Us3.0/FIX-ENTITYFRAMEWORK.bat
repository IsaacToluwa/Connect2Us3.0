@echo off
REM Book2Us Package Restore - Easy Fix for Friend's PC
REM This batch file fixes the EntityFramework missing package issue

echo ========================================
echo Book2Us Package Restore Tool
echo Fixes EntityFramework 6.5.1 missing error
echo ========================================
echo.

REM Check if PowerShell is available
where powershell >nul 2>nul
if %errorlevel% == 0 (
    echo Found PowerShell - running automated fix...
    echo.
    
    REM Run the PowerShell script
    powershell -ExecutionPolicy Bypass -File "Friend-Package-Restore.ps1"
    
    if %errorlevel% == 0 (
        echo.
        echo ✅ Package restore completed successfully!
        echo.
        echo Next steps:
        echo 1. Open Visual Studio
        echo 2. Open the Book2Us solution
        echo 3. Build the solution (Ctrl+Shift+B)
        echo 4. Run the application
        echo.
    ) else (
        echo.
        echo ⚠️  PowerShell script had some issues.
        echo Trying alternative method...
        goto :manual_method
    )
) else (
    echo PowerShell not found - using manual method...
    goto :manual_method
)

goto :end

:manual_method
echo.
echo 🔧 Manual Package Restore Method
echo ========================================
echo.

REM Try dotnet restore first
where dotnet >nul 2>nul
if %errorlevel% == 0 (
    echo Found dotnet CLI - attempting restore...
    dotnet restore
    if %errorlevel% == 0 (
        echo ✅ dotnet restore successful!
        goto :end
    )
)

REM Try to download and use NuGet CLI
echo.
echo Downloading NuGet CLI...
powershell -Command "Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile 'nuget.exe'" >nul 2>nul

if exist nuget.exe (
    echo ✅ NuGet CLI downloaded successfully!
    echo.
    echo Running NuGet restore...
    nuget restore book2us.sln
    if %errorlevel% == 0 (
        echo ✅ NuGet restore successful!
    ) else (
        echo ❌ NuGet restore failed.
        goto :visual_studio_method
    )
) else (
    echo ❌ Could not download NuGet CLI.
    goto :visual_studio_method
)

goto :end

:visual_studio_method
echo.
echo 📦 Visual Studio Method
echo ========================
echo When Visual Studio opens:
echo 1. Right-click on the solution in Solution Explorer
echo 2. Select "Restore NuGet Packages"
echo 3. Wait for restoration to complete
echo 4. Build the solution (Ctrl+Shift+B)
echo.
echo Opening Visual Studio project...
if exist book2us.sln (
    start book2us.sln
) else (
    echo Could not find book2us.sln file
echo Please open the solution manually in Visual Studio
)

:end
echo.
echo ========================================
echo 🎯 Package restore process complete!
echo.
echo If you still see errors:
echo 1. Open Visual Studio
echo 2. Go to Tools → NuGet Package Manager → Package Manager Console
echo 3. Type: Install-Package EntityFramework -Version 6.5.1
echo 4. Press Enter and wait for installation
echo.
echo ========================================
pause