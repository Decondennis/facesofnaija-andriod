# Server URL Updated to facesofnaija.net

## ✅ Changes Applied

The app has been successfully updated to use your new server:

### 1. Main Server URL
**File**: `Facesofnaija/Resources/values/analytic.xml`
```xml
<string name="ApplicationUrlWeb">facesofnaija.net</string>
```

### 2. Network Security Configuration
**File**: `Facesofnaija/Resources/xml/network_security_config.xml`
- Added `facesofnaija.net` domain
- Added `www.facesofnaija.net` subdomain
- Allows both HTTP and HTTPS connections
- SSL certificate validation bypassed for testing

### 3. App Deployment
- ✅ App rebuilt successfully
- ✅ Deployed to emulator
- ✅ App launched and running

## 🌐 Server Details

**Backend Repository**: `Decondennis/facesofnaija-web`
**Server URL**: `facesofnaija.net`
**Database**: MySQL (running)

## 📝 API Endpoints

The app will now connect to:
```
https://facesofnaija.net/api/auth         (Login)
https://facesofnaija.net/api/register     (Registration)
https://facesofnaija.net/api/*            (All other endpoints)
```

## ⚠️ DNS Propagation Note

Currently, DNS for `facesofnaija.net` is not resolving from this location. This could be because:
1. DNS changes are still propagating (can take 24-48 hours)
2. DNS is configured but not publicly accessible yet
3. Need to wait for domain registration to complete

**To check DNS status**:
```powershell
nslookup facesofnaija.net
# or
Test-NetConnection facesofnaija.net -Port 443
```

## 🧪 Testing Login

Once DNS resolves, try logging in:

### Monitor Login Attempts:
```powershell
.\monitor-login.ps1
```

Then login in the app to see real-time logs.

### Test Server from PC:
```powershell
# Test if server responds
curl https://facesofnaija.net

# Test API endpoint
$body = @{
    email = "your-email@example.com"
    password = "your-password"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://facesofnaija.net/api/auth" -Method POST -Body $body -ContentType "application/json"
```

### Test from Emulator:
```powershell
# Test DNS resolution
adb shell ping -c 3 facesofnaija.net

# Test HTTP connection
adb shell curl -I https://facesofnaija.net
```

## 🔐 Login Requirements

Make sure:
1. ✅ MySQL database is running
2. ✅ Web server is running (Apache/Nginx)
3. ✅ API endpoints are accessible
4. ✅ CORS is enabled for mobile app
5. ✅ SSL certificate is installed (or HTTP is allowed)
6. ✅ You have valid user credentials

## 📱 App Configuration Summary

| Setting | Value |
|---------|-------|
| Server URL | facesofnaija.net |
| Package Name | com.facesofnaija.tlapp |
| Database | SQLite (local) + MySQL (server) |
| .NET Version | 9.0 |
| Android API | 36 (Android 16) |
| Min SDK | 21 (Android 5.0) |

## 🚀 Quick Deploy

To redeploy with any future changes:
```powershell
.\quick-deploy.ps1
```

## 🔍 Troubleshooting

### If login still fails:

1. **Check server logs** (on your web server)
2. **Monitor app logs**:
   ```powershell
   .\monitor-login.ps1
   ```
3. **Verify API response**:
   - Check if server returns proper JSON
   - Verify HTTP status codes (200, 400, etc.)
   - Check for CORS errors

4. **Common Issues**:
   - Server firewall blocking mobile app
   - API endpoints returning HTML instead of JSON
   - Database connection issues
   - Invalid credentials

### DNS Not Resolving?

If `facesofnaija.net` DNS isn't working yet:

1. **Use IP address temporarily**:
   ```xml
   <string name="ApplicationUrlWeb">your.server.ip.address</string>
   ```

2. **Or use local testing**:
   ```xml
   <string name="ApplicationUrlWeb">10.0.2.2/facesofnaija-api</string>
   ```
   Then rebuild: `.\quick-deploy.ps1`

## ✨ Next Steps

1. **Verify DNS resolves**: Wait for DNS propagation
2. **Test server access**: Use curl/Postman to test API
3. **Create test account**: Register through web interface
4. **Try login in app**: Use test credentials
5. **Monitor logs**: Use `.\monitor-login.ps1` to debug

## 📞 Support

If login still doesn't work after DNS resolves:
1. Share the exact error from `monitor-login.ps1`
2. Check server logs for incoming requests
3. Verify API response format matches expected structure
4. Test API endpoints with Postman/curl first

---

**Status**: ✅ App configured and deployed
**Action Required**: Wait for DNS propagation or provide server IP
**Ready for Testing**: Once facesofnaija.net resolves
