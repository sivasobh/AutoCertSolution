# AutoCert Development Environment - Installation Guide

**Created:** February 9, 2026  
**Target:** Windows 11 Pro, .NET 10 Development  
**Estimated Installation Time:** 45-60 minutes (Phase 1)

---

## 🎯 Quick Summary

Your machine **IS missing 4 critical tools** that block development. This guide will help you install them in the correct order.

| Priority | Tool | Status | Installation Time |
|----------|------|--------|-------------------|
| 🔴 P0 | Start Docker | ⚠️ Service not running | 2 minutes |
| 🔴 P0 | .NET 10 SDK | ❌ Not installed | 15 minutes |
| 🔴 P0 | Node.js 20 LTS | ❌ Not installed | 10 minutes |
| 🔴 P0 | Python 3.12 | ❌ Not installed | 10 minutes |
| 🟡 P1 | Terraform | ❌ Not installed | 5 minutes |
| 🟡 P1 | Helm | ❌ Not installed | 5 minutes |
| 🟢 P2 | Kubernetes (local) | ❌ Not enabled | 10 minutes |

---

## 📋 Pre-Installation Checklist

- [ ] You have Administrator access to this Windows 11 machine
- [ ] You have internet connectivity
- [ ] You have ~5 GB free disk space (you have 257 GB ✅)
- [ ] You're ready to install 4-6 tools

---

## Phase 1: Critical Tools (MUST DO TODAY)

### Step 1: Start Docker Desktop ⏱️ 2 minutes

Docker is already installed on your machine but the daemon is not running.

**Method 1: Using GUI (Easiest)**
1. Click the Windows **Start** button
2. Search for "Docker Desktop"
3. Click to open
4. Wait for the whale icon to appear in system tray
5. Docker is ready when it stops showing "Starting..."

**Method 2: Using PowerShell (if you prefer)**
```powershell
# Open PowerShell as Administrator and run:
Start-Process "C:\Program Files\Docker\Docker\Docker.exe"

# Wait ~2 minutes for startup
Write-Host "Waiting for Docker to start..."
Start-Sleep -Seconds 120

# Verify Docker is running
docker ps
```

**Method 3: Restart Docker Service**
```powershell
# Open PowerShell as Administrator
Restart-Service -Name "Docker Desktop Service" -Force
# or
net start "Docker Desktop Service"
```

**Verify Success:**
```powershell
docker --version
docker ps
# Should show: The daemon is running without errors
```

---

### Step 2: Install .NET 10 SDK ⏱️ 15 minutes

This is **essential** - you cannot develop the AutoCert project without it.

**Option A: Using Windows Installer (Recommended)**

1. **Download the installer**
   - Visit: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
   - Click **"Download .NET 10 SDK"** (Windows x64)
   - Save the `.exe` file (dotnet-sdk-10.0.x-win-x64.exe)
   - File size: ~200 MB

2. **Run the installer**
   - Double-click the downloaded `.exe` file
   - Click "Install"
   - Accept the license
   - Make sure **"Add to PATH"** is checked ✅
   - Click "Install" and wait ~10 minutes
   - Click "Finish"

3. **Verify Installation**
   ```powershell
   # Close and reopen PowerShell (to reload PATH)
   dotnet --version
   # Expected output: 10.0.x
   
   dotnet --list-sdks
   # Should show your newly installed SDK
   ```

**Option B: Using Package Manager (winget)**

```powershell
# Open PowerShell as Administrator
winget install Microsoft.DotNet.SDK.10
# Follow prompts to accept
```

**Option C: Using Chocolatey**

```powershell
# Open PowerShell as Administrator
choco install dotnet-10.0-sdk -y
```

**Troubleshooting:**
```powershell
# If dotnet not found after installation:
# Manually add to PATH
$env:Path += ";C:\Program Files\dotnet"

# Verify installation location
Get-ChildItem "C:\Program Files\dotnet\sdk"
```

---

### Step 3: Install Node.js 20 LTS ⏱️ 10 minutes

Required for Blazor WebAssembly frontend development and npm/build tools.

**Option A: Using Windows Installer (Recommended)**

1. **Download Node.js LTS**
   - Visit: https://nodejs.org/en/
   - Click the **LTS (Recommended for Most Users)** button
   - Download the Windows Installer (`.msi`)
   - File size: ~200 MB

2. **Run the installer**
   - Double-click the downloaded `.msi` file
   - Click "Install"
   - Accept the license
   - Use default installation path: `C:\Program Files\nodejs`
   - Make sure npm is included
   - Click "Install" and wait ~5 minutes
   - Click "Finish"

3. **Verify Installation**
   ```powershell
   # Close and reopen PowerShell (to reload PATH)
   node --version
   # Expected output: v20.x.x
   
   npm --version
   # Expected output: 10.x.x
   
   npm list -g
   # Shows globally installed packages
   ```

**Option B: Using Package Manager (winget)**

```powershell
# Open PowerShell as Administrator
winget install OpenJS.NodeJS
# Follow prompts to accept
```

**Option C: Using Chocolatey**

```powershell
# Open PowerShell as Administrator
choco install nodejs -y
```

**Verify Success:**
```powershell
node --version   # Should be v20.x or higher
npm --version    # Should be 10.x or higher
```

---

### Step 4: Install Python 3.12 ⏱️ 10 minutes

Required for certificate renewal scripts and Azure/AWS CLI integrations.

**Option A: Using Windows Installer (Recommended)**

1. **Download Python**
   - Visit: https://www.python.org/downloads/
   - Click **"Download Python 3.12.x"** (latest stable)
   - Choose Windows installer (Windows x86-64 executable installer)
   - File size: ~100 MB

2. **Run the installer**
   - Double-click the downloaded `.exe` file
   - ✅ **IMPORTANT: Check "Add Python 3.12 to PATH"** (at bottom left)
   - Click "Install Now"
   - Wait ~5 minutes for installation
   - Uncheck "Disable path length limit" (optional but recommended unchecked)
   - Click "Close"

3. **Verify Installation**
   ```powershell
   # Close and reopen PowerShell (to reload PATH)
   python --version
   # Expected output: Python 3.12.x
   
   pip --version
   # Expected output: pip 24.x from C:\...\python3.12
   
   pip list
   # Shows installed packages
   ```

**Option B: Using Package Manager (winget)**

```powershell
# Open PowerShell as Administrator
winget install Python.Python.3.12
# Follow prompts to accept
```

**Option C: Using Microsoft Store**

```powershell
# Open PowerShell and type:
python
# This will open Microsoft Store to install Python
# Download and install from there
```

**Verify Success:**
```powershell
python --version     # Should be Python 3.12.x
pip --version        # Should be pip 24.x+
```

---

## Verification - Phase 1 Complete? ✅

Run this command to check if Phase 1 is complete:

```powershell
Write-Host "=== AutoCert Phase 1 Verification ===" -ForegroundColor Cyan

Write-Host "`n1. Docker:" -ForegroundColor Yellow
if (docker ps 2>$null) { 
    Write-Host "✅ Docker running - $(docker --version)" -ForegroundColor Green 
} else { 
    Write-Host "❌ Docker not running - start Docker Desktop" -ForegroundColor Red 
}

Write-Host "`n2. .NET SDK:" -ForegroundColor Yellow
if (dotnet --version 2>$null) { 
    Write-Host "✅ .NET installed - $(dotnet --version)" -ForegroundColor Green 
} else { 
    Write-Host "❌ .NET not found in PATH - restart PowerShell or reinstall" -ForegroundColor Red 
}

Write-Host "`n3. Node.js:" -ForegroundColor Yellow
if (node --version 2>$null) { 
    Write-Host "✅ Node.js installed - $(node --version)" -ForegroundColor Green 
} else { 
    Write-Host "❌ Node.js not found - reinstall" -ForegroundColor Red 
}

Write-Host "`n4. npm:" -ForegroundColor Yellow
if (npm --version 2>$null) { 
    Write-Host "✅ npm installed - $(npm --version)" -ForegroundColor Green 
} else { 
    Write-Host "❌ npm not found - reinstall Node.js" -ForegroundColor Red 
}

Write-Host "`n5. Python:" -ForegroundColor Yellow
if (python --version 2>$null) { 
    Write-Host "✅ Python installed - $(python --version)" -ForegroundColor Green 
} else { 
    Write-Host "❌ Python not found - reinstall with 'Add to PATH'" -ForegroundColor Red 
}

Write-Host "`n=== Phase 1 Status ===" -ForegroundColor Cyan
$allOk = (docker ps 2>$null) -and (dotnet --version 2>$null) -and (node --version 2>$null) -and (python --version 2>$null)
if ($allOk) {
    Write-Host "✅ Phase 1 Complete - Ready to proceed to Phase 2" -ForegroundColor Green
} else {
    Write-Host "❌ Some tools missing - complete installations above" -ForegroundColor Red
}
```

Save as `verify-phase1.ps1` and run:
```powershell
.\verify-phase1.ps1
```

---

## Phase 2: Important Tools (Do This Week)

Once Phase 1 is complete, install these:

### Step 5: Install Terraform ⏱️ 5 minutes

Infrastructure as Code for automated certificate renewal.

**Option A: Using Windows Installer**
1. Visit: https://www.terraform.io/downloads
2. Download "Windows (AMD64)"
3. Extract to: `C:\Tools\terraform\` or `C:\Program Files\terraform\`
4. Add to PATH or use full path

**Option B: Using winget**
```powershell
winget install HashiCorp.Terraform
```

**Option C: Using Chocolatey**
```powershell
choco install terraform -y
```

**Verify:**
```powershell
terraform --version
```

---

### Step 6: Install Helm ⏱️ 5 minutes

Kubernetes package manager for deploying to K8s.

**Option A: Using winget**
```powershell
winget install Helm.Helm
```

**Option B: Using Chocolatey**
```powershell
choco install kubernetes-helm -y
```

**Option C: Manual Download**
1. Visit: https://github.com/helm/helm/releases
2. Download `helm-v3.x.x-windows-amd64.zip`
3. Extract to `C:\Tools\helm\`
4. Add to PATH

**Verify:**
```powershell
helm version
```

---

### Step 7: Enable Kubernetes in Docker Desktop ⏱️ 10 minutes

1. Open **Docker Desktop**
2. Click the gear icon (Settings)
3. Go to **Kubernetes** tab (left sidebar)
4. Check the box: "Enable Kubernetes"
5. Click "Apply & Restart"
6. Wait 5-10 minutes for cluster to start
7. You'll see a notification when complete

**Verify:**
```powershell
kubectl cluster-info
kubectl get nodes
# Should show docker-desktop as a ready node
```

---

## Phase 3: Optional but Useful

### Install Azure CLI (if you're using Azure)
```powershell
# Download from: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows
# Or use winget:
winget install Microsoft.AzureCLI

# Verify
az --version
az login
```

### Install AWS CLI (if you're using AWS)
```powershell
# Download from: https://aws.amazon.com/cli/
# Or use winget/chocolatey:
winget install Amazon.AWSCLI
choco install awscli

# Verify
aws --version
aws configure
```

### Install OpenSSL (for certificate operations)
```powershell
# Using Chocolatey:
choco install openssl -y

# Or use WSL Ubuntu (already installed):
wsl openssl version
```

---

## Automated Installation Script (Optional)

If you're comfortable with automation, save this as `Install-AutocertDev.ps1`:

```powershell
# AutoCert Development Environment Installer
# Run as Administrator
# Usage: .\Install-AutocertDev.ps1

param(
    [switch]$SkipDocker,
    [switch]$SkipDotNet,
    [switch]$SkipNode,
    [switch]$SkipPython,
    [switch]$SkipTerraform,
    [switch]$SkipHelm
)

Write-Host "🚀 AutoCert Development Environment Installer" -ForegroundColor Green
Write-Host "============================================`n"

# Check for admin rights
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Check for winget or chocolatey
$useWinget = $false
$useChoco = $false

if (Get-Command winget -ErrorAction SilentlyContinue) {
    $useWinget = $true
    Write-Host "✅ Found winget - will use for installations" -ForegroundColor Green
} elseif (Get-Command choco -ErrorAction SilentlyContinue) {
    $useChoco = $true
    Write-Host "✅ Found Chocolatey - will use for installations" -ForegroundColor Green
} else {
    Write-Host "❌ Neither winget nor Chocolatey found" -ForegroundColor Red
    Write-Host "Install Chocolatey: https://chocolatey.org/install" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n📦 Starting installations...`n"

# 1. Docker Desktop
if (-not $SkipDocker) {
    Write-Host "1️⃣  Docker Desktop..." -ForegroundColor Yellow
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        Write-Host "✅ Docker already installed" -ForegroundColor Green
        Write-Host "   Please start Docker Desktop from Start Menu" -ForegroundColor Cyan
    } else {
        if ($useWinget) {
            winget install Docker.DockerDesktop --accept-source-agreements --accept-package-agreements
        } else {
            choco install docker-desktop -y
        }
    }
}

# 2. .NET SDK
if (-not $SkipDotNet) {
    Write-Host "`n2️⃣  .NET 10 SDK..." -ForegroundColor Yellow
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        Write-Host "✅ .NET already installed: $(dotnet --version)" -ForegroundColor Green
    } else {
        if ($useWinget) {
            winget install Microsoft.DotNet.SDK.10 --accept-source-agreements --accept-package-agreements
        } else {
            choco install dotnet-10.0-sdk -y
        }
    }
}

# 3. Node.js
if (-not $SkipNode) {
    Write-Host "`n3️⃣  Node.js & npm..." -ForegroundColor Yellow
    if (Get-Command node -ErrorAction SilentlyContinue) {
        Write-Host "✅ Node.js already installed: $(node --version)" -ForegroundColor Green
    } else {
        if ($useWinget) {
            winget install OpenJS.NodeJS --accept-source-agreements --accept-package-agreements
        } else {
            choco install nodejs -y
        }
    }
}

# 4. Python
if (-not $SkipPython) {
    Write-Host "`n4️⃣  Python 3..." -ForegroundColor Yellow
    if (Get-Command python -ErrorAction SilentlyContinue) {
        Write-Host "✅ Python already installed: $(python --version)" -ForegroundColor Green
    } else {
        if ($useWinget) {
            winget install Python.Python.3.12 --accept-source-agreements --accept-package-agreements
        } else {
            choco install python -y
        }
    }
}

# 5. Terraform
if (-not $SkipTerraform) {
    Write-Host "`n5️⃣  Terraform..." -ForegroundColor Yellow
    if (Get-Command terraform -ErrorAction SilentlyContinue) {
        Write-Host "✅ Terraform already installed: $(terraform --version | Select-Object -First 1)" -ForegroundColor Green
    } else {
        if ($useWinget) {
            winget install HashiCorp.Terraform --accept-source-agreements --accept-package-agreements
        } else {
            choco install terraform -y
        }
    }
}

# 6. Helm
if (-not $SkipHelm) {
    Write-Host "`n6️⃣  Helm..." -ForegroundColor Yellow
    if (Get-Command helm -ErrorAction SilentlyContinue) {
        Write-Host "✅ Helm already installed: $(helm version --client 2>/dev/null)" -ForegroundColor Green
    } else {
        if ($useWinget) {
            winget install Helm.Helm --accept-source-agreements --accept-package-agreements
        } else {
            choco install kubernetes-helm -y
        }
    }
}

Write-Host "`n✅ Installation complete!" -ForegroundColor Green
Write-Host "`n📌 IMPORTANT NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Close and reopen PowerShell to update PATH"
Write-Host "2. Start Docker Desktop from the Start Menu"
Write-Host "3. Run the verification script: .\verify-phase1.ps1"
Write-Host "4. Once all tools are verified, enable Kubernetes in Docker Desktop Settings`n"
```

Run it with:
```powershell
# Allow script execution
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force

# Run the installer
.\Install-AutocertDev.ps1
```

---

## 🔧 Post-Installation Configuration

### 1. Configure Git (First Time)
```powershell
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
git config --global core.autocrlf true  # Important for Windows
```

### 2. Configure npm
```powershell
npm config set legacy-peer-deps true  # For some package compatibility

# Or use a .npmrc file in your project
# (Already configured in AutoCert project)
```

### 3. Configure Docker
```powershell
# After Docker Desktop is running:
docker ps  # Verify daemon is responding

# Optional: Configure resources
# Open Docker Desktop > Settings > Resources
# Recommended for development:
# - CPUs: 4-8
# - Memory: 6-8 GB
# - Swap: 1-2 GB
```

### 4. Test .NET Project Creation
```powershell
# Create a test project to verify .NET works
dotnet new console -n TestAutocert -o C:\temp\TestAutocert
cd C:\temp\TestAutocert
dotnet run
# Should output "Hello, World!"
```

---

## ⚠️ Troubleshooting Common Issues

### Issue: "dotnet: command not found" after installation
**Solution:**
```powershell
# Close PowerShell completely and reopen (important!)
# If still not found, manually add to PATH:

# Check if .NET is installed:
Get-ChildItem "C:\Program Files\dotnet"

# If found, add to PATH:
$env:Path += ";C:\Program Files\dotnet"

# Make permanent:
[Environment]::SetEnvironmentVariable("Path", "$env:Path;C:\Program Files\dotnet", "User")
```

### Issue: "Docker daemon not responding"
**Solution:**
```powershell
# Option 1: Restart Docker Desktop
# Right-click Docker icon > Quit
# Then open Docker Desktop from Start Menu

# Option 2: Restart the service
Restart-Service -Name "com.docker.service" -Force

# Option 3: Reset Docker
"C:\Program Files\Docker\Docker\docker.exe" --reset
```

### Issue: Docker uses all RAM / system is slow
**Solution:**
```powershell
# Open Docker Desktop > Settings > Resources
# Reduce allocated memory to 4-6 GB
# Reduce CPUs to 4
# Apply and restart
```

### Issue: npm install very slow
**Solution:**
```powershell
npm config set registry https://registry.npmjs.org/
npm cache clean --force
npm install  # Retry
```

### Issue: Python "is not recognized"
**Solution:**
```powershell
# Reinstall Python with "Add Python to PATH" checked
# Or add manually:
[Environment]::SetEnvironmentVariable("Path", "$env:Path;C:\Users\USER\AppData\Local\Programs\Python\Python312", "User")
```

---

## ✅ Final Verification Checklist

After all installations, verify everything works:

```powershell
# Copy and paste this entire block:

Write-Host "🔍 Final Verification" -ForegroundColor Cyan
Write-Host "==================== `n"

$checks = @(
    @{Name="Docker"; Cmd="docker --version"},
    @{Name=".NET SDK"; Cmd="dotnet --version"},
    @{Name="Node.js"; Cmd="node --version"},
    @{Name="npm"; Cmd="npm --version"},
    @{Name="Python"; Cmd="python --version"},
    @{Name="Git"; Cmd="git --version"},
    @{Name="Terraform"; Cmd="terraform --version"},
    @{Name="Helm"; Cmd="helm version --short"}
)

$passed = 0
$failed = 0

foreach ($check in $checks) {
    try {
        $result = Invoke-Expression $check.Cmd 2>&1 | Select-Object -First 1
        Write-Host "✅ $($check.Name): $result" -ForegroundColor Green
        $passed++
    } catch {
        Write-Host "❌ $($check.Name): NOT FOUND" -ForegroundColor Red
        $failed++
    }
}

Write-Host "`n================" -ForegroundColor Cyan
Write-Host "✅ Passed: $passed" -ForegroundColor Green
Write-Host "❌ Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })

if ($failed -eq 0) {
    Write-Host "`n🎉 All tools installed successfully!" -ForegroundColor Green
    Write-Host "You're ready to start AutoCert development!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Some tools are missing. Please install them." -ForegroundColor Yellow
}
```

---

## 📞 Support Resources

| Issue | Resource |
|-------|----------|
| .NET Installation | https://dotnet.microsoft.com/download/dotnet/10.0 |
| Node.js Installation | https://nodejs.org/en/download/package-manager/ |
| Python Installation | https://docs.python.org/3/using/windows.html |
| Docker Desktop | https://docs.docker.com/desktop/install/windows-install/ |
| WSL 2 Issues | https://learn.microsoft.com/en-us/windows/wsl/install |
| Package Manager Issues | https://github.com/microsoft/winget-cli/issues |

---

## 🎯 Next: Start Development!

Once all Phase 1 tools are installed and verified:

1. **Clone or create the AutoCert repository**
   ```powershell
   git clone <your-repo-url>
   cd AutocertSolution
   ```

2. **Restore NuGet packages**
   ```powershell
   dotnet restore
   ```

3. **Build the solution**
   ```powershell
   dotnet build
   ```

4. **Run the application**
   ```powershell
   dotnet run --project src/AutoCert.API
   ```

5. **Start local services (Docker)**
   ```powershell
   docker-compose up -d
   ```

---

**Last Updated:** February 9, 2026  
**Installation Guide Version:** 1.0  
**Target Environment:** Windows 11 Pro, .NET 10, Docker, Kubernetes
