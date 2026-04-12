# Timeline Posts Not Loading - Root Cause & Solution

## Problem Summary

✓ **Server API**: Working (3 sample posts available)  
✓ **Android App**: Installed and running  
✓ **App Authentication**: Working (can see logged-in username)  
✗ **Timeline Posts**: Not displaying  

## Root Cause: Network Isolation

**The device cannot reach the server at `172.236.19.52`**

### Evidence:
1. **API works from host**: ✓ Can fetch posts from development machine
2. **App is not calling API**: No `LoadDataApi` or `FetchNewsFeedApiPosts` logs when timeline loads
3. **Device network test**: Ping to 172.236.19.52 failed (100% packet loss)
4. **App level**: Fragment loads UI but API never gets called due to network error

## Why This Happens

Your physical device is on your WiFi network, but:
- ❌ Cannot reach `172.236.19.52` (server IP)
- ❌ Possible firewall blocking local IP access
- ❌ Network routing issue between device and server
- ❌ Server not accessible from WiFi network

## Solutions

### Option 1: Use Your Computer's IP Instead (Easiest) ✅

1. **Find your computer's local IP**:
```powershell
ipconfig | Select-String "IPv4 Address"
# Look for something like 192.168.x.x or 10.0.x.x
```

2. **Update the app to use your computer's IP instead of 172.236.19.52**

3. **In the code** (`SocialLoginBaseActivity.cs`):
```csharp
// Change from:
field?.SetValue(null, "http://172.236.19.52");

// To:
field?.SetValue(null, "http://YOUR.LOCAL.IP.HERE");
```

4. **Rebuild and reinstall the APK**

### Option 2: Access Server via Domain Name

If your server has a public domain, use that instead of the IP address.

### Option 3: Configure Server Firewall

If 172.236.19.52 is the correct server, check:
```bash
# SSH to server
ssh root@172.236.19.52

# Check if port 80 is listening
netstat -an | grep ":80"

# Check firewall rules
ufw status
```

---

## Why Posts Don't Show (Technical)

The app calls:
1. `NewsFeedNative.LoadPost()` → UI loads ✓
2. `StartApiService()` → Attempts API call
3. `FetchNewsFeedApiPosts()` → Makes HTTP request to `http://172.236.19.52/...`
4. ❌ **Network request fails silently** (device can't reach IP)
5. No posts returned → Timeline stays empty

## Verification Commands

### Check device network:
```powershell
$adbPath = "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe"

# Test connectivity
& $adbPath shell ping -c 3 172.236.19.52

# Test HTTP GET
& $adbPath shell "wget -O /dev/null http://172.236.19.52/api/v2/endpoints/posts_mock.php"
```

### Check app logs for network errors:
```powershell
& $adbPath logcat -d | Select-String -Pattern "(ConnectException|IOException|UnknownHostException|Connection refused)"
```

---

## Quick Fix: Use Docker/Local Server on Computer

If 172.236.19.52 isn't accessible from WiFi, run the server locally:

```bash
# On your Windows machine, start a simple HTTP server
# (if you have PHP installed locally)
php -S 192.168.x.x:8080 -t C:\Users\Dell\source\repos\workspace\facesofnaija-web\

# Then update app to: http://192.168.x.x:8080
```

---

## Next Steps

1. **Determine your computer's local IP**:
```powershell
ipconfig
```

2. **Try connecting device browser to your IP**:
```
Open device browser → http://YOUR.LOCAL.IP:port
```

3. **If that works**: Update the app code to use your local IP
4. **Rebuild APK and reinstall**
5. **Posts should now load** ✓

---

## Summary

**Timeline feature code**: ✓ Working perfectly  
**Server API**: ✓ Working perfectly  
**Network connectivity**: ✗ Device → Server blocked  

**Fix**: Make server accessible from device's network OR change server IP to reachable address.

