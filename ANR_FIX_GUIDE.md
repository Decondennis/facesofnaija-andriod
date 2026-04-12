# App Not Responding (ANR) Fix Guide

## 🔴 Problem
When trying to login, the app shows:
**"Facesofnaija isn't responding"**

## 🔍 Root Cause
The app is configured to connect to your local server at `10.0.2.2`, but the server isn't running. This causes:
1. Network requests hang/timeout
2. UI thread blocks waiting for response
3. Android shows ANR dialog after 5 seconds

## ✅ IMMEDIATE FIX: Start Your Local Server

### Step 1: Check if XAMPP/WAMP is installed

**For XAMPP:**
```powershell
Test-Path "C:\xampp\xampp-control.exe"
```

**For WAMP:**
```powershell
Test-Path "C:\wamp64\wampmanager.exe"
```

### Step 2: Start Your Server

#### If using XAMPP:
1. Open `C:\xampp\xampp-control.exe`
2. Click **Start** next to **Apache**
3. Click **Start** next to **MySQL**
4. Wait until both show green "Running"

#### If using WAMP:
1. Run `C:\wamp64\wampmanager.exe`
2. Left-click the icon in system tray
3. Select **Start All Services**
4. Icon turns green when ready

### Step 3: Verify Server is Running

```powershell
# Run our check script
.\check-server.ps1

# Or manually test
curl http://localhost
```

You should see HTML response or Apache welcome page.

### Step 4: Place Your Backend Files

Make sure your `facesofnaija-web` backend is in the web root:

**XAMPP**: `C:\xampp\htdocs\facesofnaija-web\`
**WAMP**: `C:\wamp64\www\facesofnaija-web\`

### Step 5: Test API Endpoint

```powershell
# Test if API is accessible
curl http://localhost/facesofnaija-web/api/settings
```

Should return JSON response (not 404).

### Step 6: Try Login Again

1. Open the app on emulator
2. Enter credentials
3. Click Login
4. Should work now!

---

## 🛠️ LONG-TERM FIX: Better Timeout Handling

To prevent ANR even when server is slow/down, we should add proper timeouts and background threading.

### Current Issue in Code:

The login button handler is calling an async method but blocking the UI:

```csharp
private async void BtnLoginOnClick(object sender, EventArgs e)
{
    // ... validation ...
    ToggleVisibility(true);  // Shows progress
    await AuthApi(TxtEmail.Text, TxtPassword.Text, ChkRemember.Checked);
    // If AuthApi hangs, UI freezes here
}
```

### Recommended Fixes:

#### Fix 1: Add Timeout to HttpClient (In WoWonderClient library)

If you have access to the WoWonderClient source:

```csharp
var client = new HttpClient();
client.Timeout = TimeSpan.FromSeconds(30); // Add timeout
```

#### Fix 2: Use CancellationToken

```csharp
private async void BtnLoginOnClick(object sender, EventArgs e)
{
    try
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        ToggleVisibility(true);
        await AuthApi(TxtEmail.Text, TxtPassword.Text, ChkRemember.Checked, cts.Token);
    }
    catch (OperationCanceledException)
    {
        ToggleVisibility(false);
        Methods.DialogPopup.InvokeAndShowDialog(this, 
            GetText(Resource.String.Lbl_Security), 
            "Connection timeout. Please check your internet connection.",
            GetText(Resource.String.Lbl_Ok));
    }
}
```

#### Fix 3: Show Better Error Messages

Update the AuthApi method to handle connection errors:

```csharp
try
{
    var (apiStatus, respond) = await RequestsAsync.Auth.AuthAsync(...);
    // ... handle response ...
}
catch (HttpRequestException ex)
{
    Methods.DialogPopup.InvokeAndShowDialog(this,
        GetText(Resource.String.Lbl_Security),
        "Cannot connect to server. Please check:\n" +
        "1. XAMPP/WAMP is running\n" +
        "2. Server URL is correct\n" +
        "3. Internet connection is active",
        GetText(Resource.String.Lbl_Ok));
}
catch (TaskCanceledException ex)
{
    Methods.DialogPopup.InvokeAndShowDialog(this,
        GetText(Resource.String.Lbl_Security),
        "Connection timeout. Server is not responding.",
        GetText(Resource.String.Lbl_Ok));
}
```

---

## 📋 Quick Troubleshooting Checklist

Before trying to login, verify:

- [ ] Web server is running (Apache/Nginx/IIS)
- [ ] MySQL is running
- [ ] Can access http://localhost in browser
- [ ] Backend files are in correct location
- [ ] API returns JSON (not 404)
- [ ] Firewall isn't blocking connections
- [ ] Emulator has internet connectivity

### Quick Commands:

```powershell
# Check server status
.\check-server.ps1

# Test API manually
curl http://localhost/api/settings

# View app logs
adb logcat | Select-String "facesofnaija|Exception|Error"

# Check if server is listening
netstat -ano | findstr :80
```

---

## 🎯 Current Configuration

Your app is configured to connect to:
- **Server URL**: `10.0.2.2` (localhost from emulator)
- **HTTP Port**: 80 (default)
- **API Base**: `/api/`

Example login request:
```
POST http://10.0.2.2/api/auth
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "timezone": "UTC",
  "device_id": "device-id"
}
```

---

## 🚨 Common Errors & Solutions

### Error: "App isn't responding"
**Cause**: Server not running or wrong URL
**Fix**: Start XAMPP/WAMP, verify at http://localhost

### Error: Connection refused
**Cause**: Apache not started
**Fix**: Start Apache in XAMPP Control Panel

### Error: Connection timeout
**Cause**: Server too slow or network issues
**Fix**: Check server logs, verify network

### Error: 404 Not Found
**Cause**: Wrong API path or URL rewriting not working
**Fix**: Check `.htaccess`, verify API routes

### Error: 500 Internal Server Error
**Cause**: PHP/MySQL error on server
**Fix**: Check server error logs:
- XAMPP: `C:\xampp\apache\logs\error.log`
- WAMP: `C:\wamp64\logs\apache_error.log`

---

## 💡 Best Practices

1. **Always start server before testing login**
2. **Use monitoring script**: `.\monitor-login.ps1`
3. **Check server logs** for backend errors
4. **Test API manually** with curl/Postman first
5. **Use proper error handling** in app code

---

## 🔄 Alternative: Use Mock API for Testing

If you can't run a local server, you can temporarily mock the API responses:

1. Use a service like [Mockoon](https://mockoon.com/)
2. Or [JSON Server](https://github.com/typicode/json-server)
3. Configure mock endpoints
4. Point app to mock server IP

---

## 📞 Still Having Issues?

1. **Run diagnostics**:
   ```powershell
   .\check-server.ps1
   ```

2. **Check server logs**:
   - XAMPP: `C:\xampp\apache\logs\error.log`
   - WAMP: `C:\wamp64\logs\apache_error.log`

3. **Monitor app logs**:
   ```powershell
   .\monitor-login.ps1
   ```

4. **Test API manually**:
   ```powershell
   Invoke-RestMethod -Uri "http://localhost/api/settings"
   ```

---

**Status**: ⚠️ Server must be running to login  
**Action**: Start XAMPP/WAMP and try again  
**Verification**: Run `.\check-server.ps1`
