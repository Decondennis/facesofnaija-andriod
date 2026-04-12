# Login Connection Error - Troubleshooting Guide

## Issue
You're getting "security error connection failure" when trying to login to the Facesofnaija app.

## Root Cause Analysis

The app tries to connect to `facesofnaija.com` for authentication. The error can be caused by:

1. **Server is unreachable** - The server might be down or not accessible
2. **SSL Certificate issues** - Invalid or expired SSL certificate
3. **Network configuration** - Emulator can't reach the internet
4. **API endpoint issues** - Server endpoints have changed
5. **Firewall/proxy blocking** - Network security blocking the connection

## Quick Diagnostics

Run the connection test script:
```powershell
.\test-connection.ps1
```

Monitor login attempts in real-time:
```powershell
.\monitor-login.ps1
```
Then try to login in the app.

## Solutions

### Solution 1: Test with Local Server (Recommended for Development)

If `facesofnaija.com` is not accessible, you can test with a local server:

1. **Setup local server** on your Windows machine at `http://localhost/facesofnaija-api`

2. **Update the app configuration**:
   Edit `Facesofnaija\Resources\values\analytic.xml`:
   ```xml
   <!-- For emulator (10.0.2.2 points to host machine) -->
   <string name="ApplicationUrlWeb">10.0.2.2/facesofnaija-api</string>
   ```

3. **Rebuild and deploy**:
   ```powershell
   .\quick-deploy.ps1
   ```

### Solution 2: Bypass SSL Validation (Already Enabled)

The app already has SSL bypass enabled:
- ✅ `TurnTrustFailureOnWebException = true`
- ✅ `TurnSecurityProtocolType3072On = true`
- ✅ Updated `network_security_config.xml` with permissive settings

### Solution 3: Test Server Connectivity

Test if the server is responding:

```powershell
# From your PC
curl -I https://facesofnaija.com

# From emulator
adb shell curl -I https://facesofnaija.com
```

If the server doesn't respond, it's down or blocking connections.

### Solution 4: Use VPN or Change Network

If the server is blocked by your ISP or network:
1. Try a different network (mobile hotspot)
2. Use a VPN on your PC
3. Configure emulator to use VPN

### Solution 5: Check Server Requirements

Verify the server requirements:
- Server must be running
- API endpoints must be accessible
- Database must be configured
- SSL certificate must be valid (or cleartext traffic allowed)

## API Endpoint Being Called

When you login, the app calls:
```
POST https://facesofnaija.com/api/auth
```

With parameters:
- email
- password  
- timezone
- device_id

## Testing Credentials

Make sure you're using valid test credentials. If this is a fresh installation:
1. You may need to register a new account first
2. Or use existing test credentials from your server

## Debug Mode

To see detailed error messages:

1. **Run the monitor script**:
   ```powershell
   .\monitor-login.ps1
   ```

2. **Try to login** in the app

3. **Look for errors** like:
   - `Connection refused`
   - `Connection timeout`
   - `SSL handshake failed`
   - `No route to host`
   - `Network unreachable`

## Common Error Messages

| Error | Meaning | Solution |
|-------|---------|----------|
| Connection refused | Server is down or not listening | Check if server is running |
| Timeout | Server too slow or unreachable | Check network/firewall |
| SSL handshake failed | Certificate invalid | Use cleartext or fix certificate |
| No route to host | DNS or routing issue | Check DNS settings |
| Network unreachable | No internet | Check emulator network |

## Network Security Configuration

The app's `network_security_config.xml` is configured to:
- ✅ Allow cleartext HTTP traffic
- ✅ Trust all system certificates
- ✅ Trust user-added certificates
- ✅ Allow connections to facesofnaija.com, localhost, and IP addresses
- ✅ Debug mode allows all connections

## Verify App Configuration

Check the current server URL:
```xml
File: Facesofnaija\Resources\values\analytic.xml
Line 7: <string name="ApplicationUrlWeb">facesofnaija.com</string>
```

## Next Steps

1. **Run diagnostics**: `.\test-connection.ps1`
2. **Monitor logs**: `.\monitor-login.ps1` (keep running)
3. **Try to login** in the app
4. **Review the error** in the PowerShell window
5. **Share the exact error message** for further troubleshooting

## Alternative: Test API Directly

Test the API from your PC:

```powershell
# Test server availability
Invoke-WebRequest -Uri "https://facesofnaija.com" -Method GET

# Test auth endpoint (replace with real credentials)
$body = @{
    email = "test@example.com"
    password = "password123"
    timezone = "UTC"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://facesofnaija.com/api/auth" -Method POST -Body $body -ContentType "application/json"
```

If the PC can't connect either, the server is definitely unreachable.

## Contact Server Administrator

If the server is hosted externally:
1. Verify the server is online
2. Check firewall rules allow Android app access
3. Verify SSL certificate is valid
4. Ensure API endpoints are enabled
5. Check database connectivity

## Files Modified for Network Security

- ✅ `Facesofnaija/Resources/xml/network_security_config.xml`
- ✅ `Facesofnaija/AndroidManifest.xml` (UsesCleartextTraffic=true)
- ✅ `Facesofnaija/AppSettings.cs` (SSL bypass settings)

All security measures have been relaxed for development/testing.
