# AutoCert Development Environment - Quick Setup Checklist

**Target:** Get your Windows 11 Pro machine ready for .NET 10 + Container development  
**Total Time:** 45-60 minutes (Phase 1), ~2 hours (All phases)  
**Workspace:** `c:\Users\USER\OneDrive\Documents\AutocertSolution`

---

## ⚡ Phase 1: CRITICAL (Do Today) - 45 minutes

### ☐ Step 1: Start Docker Desktop
- **Status:** Docker installed but daemon NOT running
- **Action:** Need to start the Docker Desktop service
- **Time:** 2 minutes
- **How-to:**
  1. Click Windows Start button
  2. Search for "Docker Desktop"
  3. Click to open
  4. Wait for system tray whale icon to stop animating
  5. Verify: `docker ps` (should show running containers, not errors)

**✓ COMPLETED?** [ ] Yes [ ] No  
**Verification Command:** `docker ps`

---

### ☐ Step 2: Install .NET 10 SDK
- **Status:** NOT INSTALLED (CRITICAL)
- **Action:** Download and install from Microsoft
- **Time:** 15 minutes
- **Download Size:** ~200 MB
- **Installation Size:** ~2 GB (on disk)
- **How-to:**
  1. Visit: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
  2. Click "Download .NET 10.0 SDK" for **Windows x64**
  3. Run the installer (dotnet-sdk-10.0.x-win-x64.exe)
  4. Check "Add to PATH" ✅
  5. Complete installation (~10 minutes)
  6. Close and reopen PowerShell
  7. Verify: `dotnet --version` (should show 10.x.x)

**Quick Command (if you have winget):**
```powershell
winget install Microsoft.DotNet.SDK.10
```

**✓ COMPLETED?** [ ] Yes [ ] No  
**Verification Command:** `dotnet --version`  
**Expected Output:** v10.0.x or 10.0.x

---

### ☐ Step 3: Install Node.js 20 LTS
- **Status:** NOT INSTALLED (CRITICAL for frontend)
- **Action:** Download and install LTS version
- **Time:** 10 minutes
- **Download Size:** ~200 MB
- **How-to:**
  1. Visit: https://nodejs.org/
  2. Click **"LTS (Recommended)"** button
  3. Run Windows Installer (.msi)
  4. Use default path: `C:\Program Files\nodejs`
  5. Complete installation (~5 minutes)
  6. Close and reopen PowerShell
  7. Verify: `node --version` (should be v20.x or higher)

**Quick Command (if you have winget):**
```powershell
winget install OpenJS.NodeJS
```

**✓ COMPLETED?** [ ] Yes [ ] No  
**Verification Commands:**  
- `node --version` — Expected: v20.x.x or higher
- `npm --version` — Expected: 10.x.x or higher

---

### ☐ Step 4: Install Python 3.12
- **Status:** NOT INSTALLED (CRITICAL for scripts)
- **Action:** Download and install latest stable
- **Time:** 10 minutes
- **Download Size:** ~100 MB
- **How-to:**
  1. Visit: https://www.python.org/downloads/
  2. Click **"Download Python 3.12.x"**
  3. Choose **Windows x86-64 executable installer**
  4. Run installer
  5. ⚠️ **IMPORTANT:** Check "Add Python 3.12 to PATH" at bottom left ✅
  6. Click "Install Now"
  7. Wait ~5 minutes
  8. Close and reopen PowerShell
  9. Verify: `python --version`

**Quick Command (if you have winget):**
```powershell
winget install Python.Python.3.12
```

**✓ COMPLETED?** [ ] Yes [ ] No  
**Verification Commands:**  
- `python --version` — Expected: Python 3.12.x
- `pip --version` — Expected: pip 24.x

---

## 🔍 Phase 1 Verification

**Run this to check if Phase 1 is complete:**

```powershell
Write-Host "`n=== PHASE 1 VERIFICATION ===" -ForegroundColor Cyan

$all_ok = $true

# Test Docker
if (docker ps 2>$null) {
    Write-Host "✅ Docker: $(docker --version)" -ForegroundColor Green
} else {
    Write-Host "❌ Docker: Not running" -ForegroundColor Red
    $all_ok = $false
}

# Test .NET
if (dotnet --version 2>$null) {
    Write-Host "✅ .NET SDK: $(dotnet --version)" -ForegroundColor Green
} else {
    Write-Host "❌ .NET SDK: Not found" -ForegroundColor Red
    $all_ok = $false
}

# Test Node.js
if (node --version 2>$null) {
    Write-Host "✅ Node.js: $(node --version)" -ForegroundColor Green
} else {
    Write-Host "❌ Node.js: Not found" -ForegroundColor Red
    $all_ok = $false
}

# Test npm
if (npm --version 2>$null) {
    Write-Host "✅ npm: v$(npm --version)" -ForegroundColor Green
} else {
    Write-Host "❌ npm: Not found" -ForegroundColor Red
    $all_ok = $false
}

# Test Python
if (python --version 2>$null) {
    Write-Host "✅ Python: $(python --version)" -ForegroundColor Green
} else {
    Write-Host "❌ Python: Not found" -ForegroundColor Red
    $all_ok = $false
}

if ($all_ok) {
    Write-Host "`n✅ PHASE 1 COMPLETE!" -ForegroundColor Green
    Write-Host "You can now proceed to Phase 2 tools.`n" -ForegroundColor Green
} else {
    Write-Host "`n❌ Some tools are still missing.`n" -ForegroundColor Red
}
```

**✓ PHASE 1 COMPLETE?** [ ] Yes [ ] No

---

## 🚀 Phase 2: IMPORTANT (Do This Week) - 20 minutes

### ☐ Step 5: Install Terraform
- **Status:** NOT INSTALLED
- **Action:** Download or use package manager
- **Time:** 5 minutes
- **How-to (Option A - Package Manager):**
  ```powershell
  # Using winget
  winget install HashiCorp.Terraform
  
  # Using Chocolatey
  choco install terraform -y
  ```
- **How-to (Option B - Manual):**
  1. Visit: https://www.terraform.io/downloads
  2. Download "Windows (AMD64)" zip
  3. Extract to `C:\Tools\terraform\`
  4. Add to PATH or use `terraform.exe` with full path

**✓ COMPLETED?** [ ] Yes [ ] No  
**Verification Command:** `terraform --version`

---

### ☐ Step 6: Install Helm
- **Status:** NOT INSTALLED
- **Action:** Download or use package manager
- **Time:** 5 minutes
- **How-to (Option A - Package Manager):**
  ```powershell
  # Using winget
  winget install Helm.Helm
  
  # Using Chocolatey
  choco install kubernetes-helm -y
  ```
- **How-to (Option B - Manual):**
  1. Visit: https://github.com/helm/helm/releases
  2. Download `helm-v3.x.x-windows-amd64.zip`
  3. Extract to `C:\Tools\helm\`
  4. Add to PATH

**✓ COMPLETED?** [ ] Yes [ ] No  
**Verification Command:** `helm version`

---

### ☐ Step 7: Enable Kubernetes in Docker Desktop
- **Status:** NOT ENABLED (kubectl installed but no cluster)
- **Action:** Enable K8s in Docker Desktop settings
- **Time:** 10 minutes (cluster startup)
- **How-to:**
  1. Open Docker Desktop (from system tray or Start Menu)
  2. Click gear icon (Settings)
  3. Go to **Kubernetes** tab (left sidebar)
  4. Check "Enable Kubernetes" checkbox
  5. Click "Apply & Restart"
  6. Wait 5-10 minutes for cluster to initialize
  7. Verify: `kubectl cluster-info`

**✓ COMPLETED?** [ ] Yes [ ] No  
**Verification Command:** `kubectl get nodes`

---

## 🟢 Phase 3: OPTIONAL (As Needed) - 15 minutes

### ☐ Step 8: Install Azure CLI (if using Azure)
```powershell
winget install Microsoft.AzureCLI
# Then: az login
```
**✓ COMPLETED?** [ ] Yes [ ] No

---

### ☐ Step 9: Install AWS CLI (if using AWS)
```powershell
winget install Amazon.AWSCLI
# Then: aws configure
```
**✓ COMPLETED?** [ ] Yes [ ] No

---

### ☐ Step 10: Install OpenSSL (for certificate operations)
```powershell
# Option 1: Windows version
choco install openssl -y

# Option 2: WSL Ubuntu (likely already installed)
wsl openssl version
```
**✓ COMPLETED?** [ ] Yes [ ] No

---

## 📊 Installation Summary

### Installed vs. Missing Tools

| Tool | Status | Installed? | Critical? |
|------|--------|-----------|-----------|
| **Git** | ✅ Installed | Yes | Optional |
| **Docker** | ✅ Installed (daemon needs start) | Yes | High |
| **Docker Compose** | ✅ Installed | Yes | High |
| **VS Code** | ✅ Installed | Yes | Optional |
| **PowerShell** | ✅ Installed | Yes | Optional |
| **Windows 11 Pro** | ✅ Installed | Yes | Optional |
| **WSL 2 (Ubuntu)** | ✅ Running | Yes | Optional |
| **kubectl** | ✅ Installed | Yes | High |
| **jq** | ✅ Installed | Yes | Optional |
| **.NET 10 SDK** | ❌ Missing | [ ] | **CRITICAL** |
| **Node.js & npm** | ❌ Missing | [ ] | **CRITICAL** |
| **Python 3** | ❌ Missing | [ ] | **CRITICAL** |
| **Terraform** | ❌ Missing | [ ] | Important |
| **Helm** | ❌ Missing | [ ] | Important |
| **PostgreSQL (client)** | ❌ Missing | [ ] | Optional |
| **OpenSSL** | ❌ Missing | [ ] | Optional |
| **Azure CLI** | ❌ Missing | [ ] | Conditional |
| **AWS CLI** | ❌ Missing | [ ] | Conditional |

---

## 🎯 Quick Command Reference

```powershell
# All verification commands in one place

Write-Host "=== AutoCert Environment Check ===" -ForegroundColor Cyan

Write-Host "`n✅ INSTALLED:"
git --version
docker --version
docker compose --version
code --version
kubectl version --client

Write-Host "`n❌ TO INSTALL:"
Write-Host "dotnet --version (should not error after install)"
Write-Host "node --version (should not error after install)"
Write-Host "npm --version (should not error after install)"
Write-Host "python --version (should not error after install)"
Write-Host "terraform --version (should not error after install)"
Write-Host "helm version (should not error after install)"
```

---

## 🔧 Troubleshooting Quick Links

| Issue | Solution |
|-------|----------|
| Docker daemon not running | Click Docker icon in Start Menu |
| "Command not found" after install | Close/reopen PowerShell or restart computer |
| Docker using all RAM | Docker Settings > Resources > reduce memory allocation |
| npm install very slow | Run: `npm cache clean --force` then retry |
| Python not found | Reinstall Python with "Add to PATH" checked |
| .NET files won't run | Reinstall .NET SDK and add to PATH manually |

---

## 📈 System Resources Check

Your machine specs:
- **OS:** Windows 11 Pro (Build 26200) ✅
- **CPU:** Multiple cores ✅
- **RAM:** 32 GB total, ~14.9 GB free ✅ (plenty for development)
- **Disk:** C: drive has 257 GB free ✅ (excellent)
- **WSL 2:** Ubuntu running ✅ (available for Linux tools)

**Verdict:** Hardware is **excellent** for .NET + container development! 🎉

---

## 🎬 After Phase 1 Completion

Once Phase 1 is complete and verified:

### 1. Clone/Get the AutoCert Repository
```powershell
cd c:\Users\USER\OneDrive\Documents\AutocertSolution
git clone <repo-url> .
# OR just use this folder as your workspace
```

### 2. Restore NuGet Dependencies
```powershell
dotnet restore
```

### 3. Build the Project
```powershell
dotnet build
```

### 4. Start Local Services
```powershell
docker-compose up -d
# (You'll need a docker-compose.yml file with PostgreSQL, Redis, RabbitMQ)
```

### 5. Run the Application
```powershell
dotnet run --project src/AutoCert.API
# Application should be available at http://localhost:5000
```

---

## 📞 Support Resources

- **.NET 10 Docs:** https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- **Node.js Docs:** https://nodejs.org/en/docs/
- **Docker Docs:** https://docs.docker.com/
- **Kubernetes Docs:** https://kubernetes.io/docs/
- **Azure Docs:** https://learn.microsoft.com/en-us/azure/
- **Terraform Docs:** https://www.terraform.io/docs/

---

## ✅ Final Checklist

### Day 1 - Complete by end of day
- [ ] Docker Desktop started
- [ ] .NET 10 SDK installed and verified
- [ ] Node.js & npm installed and verified
- [ ] Python 3 installed and verified
- [ ] All Phase 1 verification tests passing

### Days 2-3 - Complete this week
- [ ] Terraform installed
- [ ] Helm installed
- [ ] Kubernetes enabled in Docker Desktop
- [ ] All Phase 2 tools verified
- [ ] Repository cloned/set up
- [ ] Project builds successfully (`dotnet build`)

### Ready for Development! 🚀
- [ ] All tools installed and verified
- [ ] Local services running (`docker-compose up`)
- [ ] Application starts successfully (`dotnet run`)
- [ ] Can connect to local Kubernetes cluster

---

**Created:** February 9, 2026  
**Last Updated:** February 9, 2026  
**Next Step:** Read `INSTALLATION_GUIDE.md` for detailed instructions on each tool
