# Book2Us NuGet Package Restore Troubleshooting Guide

## Problem: EntityFramework Package Not Found

Your friend is encountering this error:
```
The package EntityFramework with version 6.5.1 could not be found in C:\Users\thula\.nuget\packages\, C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages. Run a NuGet package restore to download the package.
```

## Quick Solution

### Method 1: Run the Fix Script (Easiest)
1. Navigate to the project folder on your friend's PC
2. Double-click `Fix-NuGet-Packages.bat` (or right-click and "Run as Administrator")
3. Follow the prompts - the script will automatically download missing packages

### Method 2: Use PowerShell Script
```powershell
# Open PowerShell in project directory
# Run:
.\Fix-NuGet-Packages.ps1
```

### Method 3: Manual Package Restore
```cmd
# Open Command Prompt in project directory
# Run one of these commands:
dotnet restore
nuget restore book2us.sln
msbuild book2us.sln /t:Restore
```

## Detailed Solutions

### Solution 1: Visual Studio Package Restore
1. **Open Visual Studio**
2. **Open the solution** (`book2us.sln`)
3. **Right-click on the solution** in Solution Explorer
4. **Select "Restore NuGet Packages"**
5. **Wait for restoration to complete**
6. **Build the solution** (Ctrl+Shift+B)

### Solution 2: Package Manager Console
1. **Open Visual Studio**
2. **Go to Tools → NuGet Package Manager → Package Manager Console**
3. **Run these commands:**
   ```powershell
   Update-Package -Reinstall
   Install-Package EntityFramework -Version 6.5.1
   ```

### Solution 3: Manual Package Download
1. **Download EntityFramework 6.5.1 manually:**
   - Go to: https://www.nuget.org/packages/EntityFramework/6.5.1
   - Click "Download" on the right side
   - Save the `.nupkg` file

2. **Extract to packages folder:**
   ```cmd
   # Create packages folder if it doesn't exist
   mkdir packages
   
   # Extract the package (rename .nupkg to .zip first)
   ren EntityFramework.6.5.1.nupkg EntityFramework.6.5.1.zip
   tar -xf EntityFramework.6.5.1.zip -C packages\EntityFramework.6.5.1
   ```

## Common Issues and Fixes

### Issue 1: NuGet CLI Not Found
**Problem:** "nuget is not recognized as an internal or external command"

**Solution:**
```cmd
# Download NuGet CLI
curl -o nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

# Add to PATH (temporary)
set PATH=%CD%;%PATH%

# Now run
nuget restore book2us.sln
```

### Issue 2: Package Source Issues
**Problem:** Packages not found in default sources

**Solution:**
```cmd
# Add NuGet.org as package source
nuget sources add -Name "nuget.org" -Source "https://api.nuget.org/v3/index.json"

# List current sources
nuget sources list

# Restore with explicit source
nuget restore book2us.sln -Source "https://api.nuget.org/v3/index.json"
```

### Issue 3: Corrupted Package Cache
**Problem:** Packages exist but are corrupted

**Solution:**
```cmd
# Clear NuGet cache
dotnet nuget locals all --clear
nuget locals all -clear

# Delete local package folders
rmdir /s /q %USERPROFILE%\.nuget\packages\EntityFramework.6.5.1
rmdir /s /q packages\EntityFramework.6.5.1

# Restore fresh
nuget restore book2us.sln
```

### Issue 4: Proxy/Network Issues
**Problem:** Corporate firewall or proxy blocking downloads

**Solution:**
```cmd
# Configure NuGet to use proxy
nuget config -set http_proxy=http://your-proxy:port
nuget config -set http_proxy.user=your-username
nuget config -set http_proxy.password=your-password

# Or download manually and place in packages folder
```

## Alternative Package Sources

If NuGet.org is blocked, try these alternatives:

1. **MyGet:** https://www.myget.org/F/aspnetwebstacknightly/
2. **Azure Artifacts:** (Corporate Microsoft packages)
3. **Local Network Share:** (Copy packages from working PC)

## Complete Package List for Book2Us

Make sure all these packages are restored:

```xml
<!-- Required packages -->
EntityFramework.6.5.1
Microsoft.AspNet.Mvc.5.2.9
Microsoft.AspNet.Razor.3.2.9
Microsoft.AspNet.WebPages.3.2.9
Microsoft.AspNet.Web.Optimization.1.1.3
Microsoft.Web.Infrastructure.2.0.1
Newtonsoft.Json.13.0.3
WebGrease.1.6.0
Antlr.3.5.0.2
Modernizr.2.8.3
jQuery.3.7.0
jQuery.Validation.1.19.5
bootstrap.5.2.3
```

## Verification Steps

After package restore, verify everything worked:

1. **Check packages folder exists:**
   ```cmd
   dir packages\EntityFramework.6.5.1
   ```

2. **Check if DLL files are present:**
   ```cmd
   dir packages\EntityFramework.6.5.1\lib\*
   ```

3. **Build the project:**
   ```cmd
   msbuild book2us.sln /p:Configuration=Debug
   ```

4. **Check for build errors:**
   - Open Visual Studio
   - Build → Build Solution
   - Look for any remaining package-related errors

## Prevention Tips

### For Future Deployments:

1. **Include packages folder** in your deployment package
2. **Use PackageReference** instead of packages.config (modern approach)
3. **Create a NuGet.config** file with package sources
4. **Document required packages** and versions

### Create NuGet.config:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

## Emergency Workaround

If all else fails, copy the working packages folder:

1. **From your working PC:**
   - Copy the entire `packages` folder
   - Copy `%USERPROFILE%\.nuget\packages` folder

2. **To friend's PC:**
   - Paste packages folder in project directory
   - Paste .nuget folder to `%USERPROFILE%`

## Getting Help

If issues persist:

1. **Check Visual Studio Output window** for detailed error messages
2. **Run with verbose logging:**
   ```cmd
   nuget restore book2us.sln -Verbosity detailed
   ```
3. **Check NuGet documentation:** https://docs.nuget.org/
4. **Try Stack Overflow:** Search for specific error messages

## Success Indicators

✅ **Package restore successful when:**
- No more "package not found" errors
- Build succeeds without errors
- Application runs without missing reference errors
- EntityFramework.dll is present in bin folder

The `Fix-NuGet-Packages.bat` script should handle most of these issues automatically!