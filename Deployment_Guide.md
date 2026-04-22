# Complete End-to-End Application Deployment Guide

**Deploying an ASP.NET Core Application to Windows Server (IIS) via pfSense & Proxmox**

This document covers the complete end-to-end deployment of an ASP.NET Core application (from GitHub) to a Windows Server VM running inside Proxmox, placed behind a pfSense firewall. It includes setting up IIS, configuring the MS SQL Server Database, applying a Wildcard SSL certificate, fixing firewall rules, and resolving common folder permissions issues.

---

## Table of Contents

1. [Phase 1: Prepare Windows Server & IIS](#phase-1-prepare-windows-server--iis)
2. [Phase 2: Database Restoration & Connection](#phase-2-database-restoration--connection)
3. [Phase 3: SSL Certificate Setup](#phase-3-ssl-certificate-setup)
4. [Phase 4: Clone, Publish, and Configure Application](#phase-4-clone-publish-and-configure-application)
5. [Phase 5: Deploy to IIS](#phase-5-deploy-to-iis)
6. [Phase 6: Networking & Firewalls](#phase-6-networking--firewalls)
7. [Phase 7: CI/CD with GitHub Actions](#phase-7-cicd-with-github-actions)
8. [Phase 8: Troubleshooting Common Errors](#phase-8-troubleshooting-common-errors)

---

## Phase 1: Prepare Windows Server & IIS

### 1. Install IIS and Required Features

1. Open **Server Manager**.
2. Click **Add Roles and Features**.
3. Proceed to the Server Roles step and check **Web Server (IIS)**.
4. Expand Web Server (IIS) → Web Server → **Application Development** and ensure the following are checked:
   - **.NET Extensibility**
   - **ASP.NET**
   - **ISAPI Extensions**
   - **ISAPI Filters**
5. Proceed and click **Install**.

### 2. Install the .NET Hosting Bundle (CRITICAL)

IIS cannot run an ASP.NET Core application without the Hosting Bundle. You must install the version that matches the framework your application targets (e.g., .NET 6).

1. Go to the official [Microsoft .NET Download Page](https://dotnet.microsoft.com/en-us/download/dotnet).
2. Select your target version (e.g., .NET 6.0).
3. Under the **ASP.NET Core Runtime** section (usually on the right side), click the **Hosting Bundle** link to download the installer (e.g., `dotnet-hosting-6.0.xx-win.exe`).
4. Run the installer.
5. Once complete, open Command Prompt as Administrator and run:
   ```cmd
   iisreset
   ```

---

## Phase 2: Database Restoration & Connection

_Disclaimer: You must safely restore the `.bak` file without overwriting any existing databases on your SQL Server instance._

### 1. Restore the Database

1. Open **SQL Server Management Studio (SSMS)** and connect to your database instance (e.g., `WIN-3HFJ0PPQBIH\SQLEXPRESS`).
   - _If the `sa` password was auto-saved and unknown, connect using **Windows Authentication**._
2. Right-click the **Databases** node and select **Restore Database...**
3. Select **Device**, click the `...`, and add your `.bak` file.
4. **CRITICAL:** Under the **Destination -> Database** field, type a **brand new name** (e.g., `MyAppDB_New`) to avoid overwriting existing data.
5. In the **Files** tab, assure the file paths are correctly pointing to your SQL Data folder (e.g., `MyAppDB_New.mdf` and `MyAppDB_New_log.ldf`).
6. In the **Options** tab, **DO NOT** check "Overwrite the existing database" (WITH REPLACE).
7. Click **OK** to restore.

### 2. Verify Database Credentials

If you connected using Windows Auth and don't know the `sa` password, create a new SQL user:

1. In SSMS, go to **Security** → **Logins** → Right-click and select **New Login**.
2. Enter a login name (e.g., `appuser`), select **SQL Server Authentication**, and set a password.
3. Under **User Mapping**, check the box next to your newly restored database (`MyAppDB_New`) and assign it the **db_owner** role.

---

## Phase 3: SSL Certificate Setup

### 1. Transfer the Wildcard Certificate

If transferring the `.pfx` file from a Linux machine to your Windows Server, use `scp`:

```bash
scp wildcard_slt_lk.pfx Administrator@192.168.100.114:C:/Users/Administrator/Desktop/
```

### 2. Import Certificate to Windows Certificate Store

IIS reads certificates from the local machine store, not the file system.

1. Press `Win + R`, type `mmc`, and hit Enter.
2. Click **File** → **Add/Remove Snap-in**.
3. Select **Certificates**, click **Add**.
4. **CRITICAL:** Choose **Computer account** (not My user account), click Next → **Local computer** → Finish. Click OK.
5. In the left pane, expand **Certificates (Local Computer)** → **Personal** → **Certificates**.
6. Right-click the **Certificates** folder → **All Tasks** → **Import...**
7. Browse to your `.pfx` file, enter the password, and finish the wizard.

---

## Phase 4: Clone, Publish, and Configure Application

### 1. Clone and Publish

Cloning the source code is not enough. You must compile the application.

1. Open PowerShell/Command Prompt and navigate to your cloned repository folder.
2. Run the `dotnet publish` command, setting the output directory to an IIS-accessible folder:
   ```cmd
   dotnet publish -c Release -o C:\inetpub\wwwroot\career-portal-slt
   ```

### 2. Configure Database Connection String

1. Open the `appsettings.json` file inside the published output folder (`C:\inetpub\wwwroot\career-portal-slt`).
2. Update the `DefaultConnection` string with your SQL credentials:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=WIN-3HFJ0PPQBIH\\SQLEXPRESS;Database=MyAppDB_New;User Id=sa;Password=Admin123;TrustServerCertificate=True;Integrated Security=False;Encrypt=false"
   }
   ```
   _(Note: Setting `Integrated Security=False` forces the application to use the specified User ID and Password)._

### 3. File Upload Configuration & Folder Creation

If your app saves attachments (like CVs):

1. Check the `Application:UploadPath` value in `appsettings.json` (e.g., `C:\JobApplications`).
2. **Create that folder** physically on the server's C: drive.

---

## Phase 5: Deploy to IIS

### 1. Create the App Pool

1. Open **IIS Manager**.
2. Right-click **Application Pools** → **Add Application Pool**.
3. Name it **Career_Portal**.
4. **CRITICAL:** Set the .NET CLR version to **No Managed Code** (this is required for ASP.NET Core).

### 2. Create the Website

1. Right-click **Sites** → **Add Website...**
2. **Site name:** `career-portal-slt`
3. **Application pool:** Select the **Career_Portal** you just created.
4. **Physical path:** `C:\inetpub\wwwroot\career-portal-slt`
5. **Binding:**
6. **Binding:**
   - Type: **https**
   - Port: **3114** (or whatever custom port you need)
   - IP address: **All Unassigned**
   - SSL Certificate: Select the wildcard certificate you imported earlier.

### 3. Grant Folder Permissions (Fixing 500.5 and Upload Errors)

IIS needs permission to read the application folder, and write to the upload folder.

1. Right-click your published app folder (`C:\inetpub\wwwroot\career-portal-slt`) → **Properties** → **Security** tab → **Edit** → **Add**.
2. Type `IIS_IUSRS` and assign it **Read & Execute** permissions.
3. Repeat the exact same process for your upload folder (e.g., `C:\JobApplications`), but check the **Modify** and **Write** permissions.

_(Always run `iisreset` or recycle the App Pool in IIS whenever you change `appsettings.json` or permissions)._

---

## Phase 6: Networking & Firewalls

Since you are hosting on a custom port, traffic must be mapped correctly backwards from the public internet into the VM.

### 1. Windows Server Firewall (Inbound Rules)

1. Open **Windows Defender Firewall with Advanced Security**.
2. Click **Inbound Rules** → **New Rule...**
3. Type: **Port** → **TCP** → Specific local ports: **3114**.
4. Action: **Allow the connection**.
5. Profile: Ensure Domain, Private, and Public are all checked.
6. Alternatively, run this in PowerShell as Admin:
   ```powershell
   New-NetFirewallRule -DisplayName "Allow IIS HTTPS MyApp 3114" -Direction Inbound -LocalPort 3114 -Protocol TCP -Action Allow
   ```

### 2. pfSense Port Forwarding (NAT)

1. Log into your pfSense router interface.
2. Go to **Firewall** → **NAT** → **Port Forward**.
3. Create a new rule:
   - **Interface:** WAN
   - **Protocol:** TCP
   - **Destination Port Range:** 3114 to 3114
   - **Redirect Target IP:** `<Internal IP of the Windows Server>`
   - **Redirect Target Port:** 3114

### 3. DNS

Ensure your public domain's DNS A-record (e.g., `dpdlab1.slt.lk`) points directly to the **Public WAN IP address** of your pfSense firewall.

---

## Phase 7: CI/CD with GitHub Actions

Automate your deployment by setting up a GitHub Actions workflow. This ensures every push to `main` builds and deploys your app automatically.

### 1. Setup GitHub Secrets

Go to your GitHub Repository → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**. Add the following:

- `SERVER_IP`: The public IP of your pfSense (e.g., `124.43.216.136`).
- `SERVER_USERNAME`: Windows Server username (e.g., `Administrator`).
- `SERVER_PASSWORD`: Windows Server password.
- `SSH_PORT`: The SSH port (usually `22`, unless NATed differently).

### 2. Workflow Configuration

I have created the workflow file at `.github/workflows/main.yml`. It performs these steps:

- Builds and publishes the .NET 6 project.
- Uses `scp-action` to copy the files to `C:\inetpub\wwwroot\career-portal-slt`.
- Uses `ssh-action` to run `Restart-WebAppPool -Name 'Career_Portal'` on the Windows Server to refresh the app.

---

## Phase 8: Troubleshooting Common Errors

- **"Took too long to respond" (Browser Timeout):**
  This means traffic isn't reaching IIS. Usually caused by a missing Windows Firewall inbound rule, or a misconfigured NAT rule in pfSense.
- **HTTP Error 502.5 - ANCM Out-Of-Process Startup Failure:**
  IIS tried to start the app but it crashed instantly. Usually caused by installing the wrong version of the **.NET Hosting Bundle** (e.g., you installed the Runtime, but not the Hosting Bundle). Check event viewer, or run `dotnet .\JobApp.dll` manually to see the underlying exception.
- **Blank Screen / Infinite Loading during Manual Test:**
  If you run the app manually via command line and it hangs, the database connection is failing/timing out. Check your `appsettings.json` connection string.
- **"Error in uploading attachments" / Submission Failures:**
  The app successfully connected to the database but crashed trying to save a file. Navigate to the upload directory specified in `appsettings.json` and ensure the `IIS_IUSRS` group has **Modify / Write** permissions.
- **Unexpected Azure Shutdowns (Event IDs 7036, 10016, 6008):**
  If hosted in Azure, never manually stop the "ATTET" or Azure VM Agent services. Doing so causes Azure health-checks to fail, forcing the hypervisor to reboot the server unexpectedly.

---

_End of documentation._
