# Testing Facesofnaija with Local Server

## ✅ App Configured for Local Server

The app is now configured to connect to your local machine:
- **Server URL**: `10.0.2.2` (emulator's way to access your PC's localhost)
- **Status**: Deployed and running on emulator

## 🖥️ Local Server Setup Requirements

### 1. Make Sure Your Web Server is Running

Your local server should be accessible at:
```
http://localhost
# or
http://127.0.0.1
```

Common setups:
- **XAMPP**: `http://localhost/facesofnaija-web`
- **WAMP**: `http://localhost/facesofnaija-web`
- **IIS**: `http://localhost`
- **Built-in PHP server**: `php -S localhost:80`

### 2. Verify Server is Running

Test from your PC:
```powershell
# Test if web server responds
curl http://localhost

# Test API endpoint (adjust path as needed)
curl http://localhost/api/auth
# or
curl http://localhost/facesofnaija-web/api/auth
```

### 3. Check MySQL is Running

Make sure MySQL service is running:
```powershell
# Check MySQL service status
Get-Service MySQL* | Select-Object Name, Status

# Or check if MySQL port is listening
Test-NetConnection -ComputerName localhost -Port 3306
```

## 📂 Common Local Server Paths

Based on your GitHub repo `Decondennis/facesofnaija-web`, your API is likely at:

```
http://localhost/facesofnaija-web/api/auth
```

If your setup is different, update the app configuration:

**Edit**: `Facesofnaija/Resources/values/analytic.xml`
```xml
<!-- If your API is in a subfolder -->
<string name="ApplicationUrlWeb">10.0.2.2/facesofnaija-web</string>

<!-- If API is at root -->
<string name="ApplicationUrlWeb">10.0.2.2</string>

<!-- If using different port -->
<string name="ApplicationUrlWeb">10.0.2.2:8080</string>
```

Then rebuild:
```powershell
.\quick-deploy.ps1
```

## 🧪 Testing Steps

### Step 1: Start Your Local Server

Make sure:
- ✅ Apache/Nginx/IIS is running
- ✅ MySQL is running
- ✅ PHP is configured correctly
- ✅ Database is created and migrated

### Step 2: Test API from PC

```powershell
# Test basic connectivity
curl http://localhost

# Test API endpoint
Invoke-RestMethod -Uri "http://localhost/api/settings" -Method GET
```

### Step 3: Test from Emulator

```powershell
# Test if emulator can reach your PC
adb shell ping -c 3 10.0.2.2

# Test HTTP connection
adb shell curl -I http://10.0.2.2
```

### Step 4: Monitor App Login

```powershell
# Run this in one terminal
.\monitor-login.ps1
```

Then try to login in the app and watch the logs.

## 🔍 Debugging Connection Issues

### Issue: "Connection refused" or "Timeout"

**Cause**: Server not running or firewall blocking

**Solution**:
```powershell
# 1. Check if server is listening
netstat -ano | findstr :80
netstat -ano | findstr :443

# 2. Disable Windows Firewall temporarily to test
# Windows Security → Firewall → Turn off (temporarily)

# 3. Make sure Apache/Nginx is listening on all interfaces (0.0.0.0)
# Not just 127.0.0.1
```

### Issue: API returns 404

**Cause**: Wrong path or URL rewriting not working

**Solution**:
Check your API routes and `.htaccess` file. Update the URL in `analytic.xml`:
```xml
<string name="ApplicationUrlWeb">10.0.2.2/facesofnaija-web</string>
```

### Issue: CORS errors

**Cause**: Server not configured for mobile app requests

**Solution**:
Add to your PHP backend (in `index.php` or API entry point):
```php
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");
header("Access-Control-Allow-Headers: Content-Type, Authorization");
```

## 📱 What the App Will Call

When you try to login, the app makes this request:

```http
POST http://10.0.2.2/api/auth
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "timezone": "UTC",
  "device_id": "unique-device-id"
}
```

Expected response:
```json
{
  "api_status": 200,
  "data": {
    "access_token": "your-token-here",
    "user_id": "123",
    "is_new": false
  }
}
```

## 🛠️ Server Configuration Files

Make sure your local server has:

1. **Database Configuration** (`config.php` or similar):
```php
$db_host = 'localhost';
$db_name = 'facesofnaija';
$db_user = 'root';
$db_pass = '';
```

2. **Base URL Configuration**:
```php
define('BASE_URL', 'http://localhost/facesofnaija-web');
```

3. **API Routes Enabled**:
Check `.htaccess` or web server config for URL rewriting.

## 📊 Test Credentials

Create a test user in your database or use existing credentials:
- Email: (your test email)
- Password: (your test password)

Or register through web interface first:
```
http://localhost/facesofnaija-web/register
```

## 🔄 Quick Command Reference

```powershell
# Rebuild and deploy app
.\quick-deploy.ps1

# Monitor login attempts
.\monitor-login.ps1

# Test server from PC
curl http://localhost

# Test server from emulator
adb shell curl http://10.0.2.2

# Check app logs
adb logcat | Select-String "facesofnaija|Exception|Error"
```

## 🎯 Success Checklist

Before trying to login:
- [ ] Web server (Apache/XAMPP/WAMP) is running
- [ ] MySQL service is running
- [ ] Database exists and has tables
- [ ] Can access `http://localhost` from browser
- [ ] API endpoint returns valid JSON
- [ ] Have valid test credentials
- [ ] Firewall allows connections (temporarily disable to test)
- [ ] App is running on emulator
- [ ] Monitoring script is running

## 🚀 When Everything Works

You should see:
1. ✅ Login screen accepts credentials
2. ✅ Progress indicator shows
3. ✅ App navigates to main screen
4. ✅ User data loads

Monitor logs should show:
```
✓ Connected to 10.0.2.2
✓ API response 200
✓ Authentication successful
```

## ⚡ Quick Troubleshooting

| Error | Quick Fix |
|-------|-----------|
| Connection refused | Start web server |
| Timeout | Check firewall |
| 404 Not Found | Update URL with correct path |
| 500 Server Error | Check PHP/server logs |
| CORS error | Add CORS headers |
| Invalid credentials | Check username/password |
| Database error | Verify MySQL running |

## 📞 Still Not Working?

Run the monitoring script and share the output:
```powershell
.\monitor-login.ps1
```

Also check your web server logs:
- **XAMPP**: `C:\xampp\apache\logs\error.log`
- **WAMP**: `C:\wamp64\logs\apache_error.log`
- **IIS**: Event Viewer → Windows Logs → Application

---

**Current Configuration**: `10.0.2.2` (localhost)
**Status**: ✅ Deployed and ready to test
**Next**: Start your local server and try logging in!
