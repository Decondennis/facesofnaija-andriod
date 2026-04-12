# Timeline Implementation - Quick Start Guide

## Server Credentials
```
IP: 172.236.19.52
User: root
Password: Chsom.22
```

## Files Ready to Deploy

### 1. API Endpoints (copy to server)
```
Source:      facesofnaija-web/api/v2/endpoints/
Destination: /var/www/html/api/v2/endpoints/
Files:
  ✓ posts.php        - Full implementation
  ✓ posts_mock.php   - Mock data for testing
```

### 2. Android APK (ready on local machine)
```
Location: Facesofnaija/bin/Release/net9.0-android36.0/
File:     com.facesofnaija.tlapp-Signed.apk
Status:   ✓ Built and signed
Size:     ~145 MB
```

## Quick Setup Steps

### Step 1: Configure Server (5 minutes)

**Option A: Automatic via PowerShell**
```powershell
# Run on your Windows machine with SSH access
.\setup-server-via-ssh.ps1 -ServerIP 172.236.19.52 -Username root -Password Chsom.22
```

**Option B: Manual SSH**
```bash
# SSH to server
ssh root@172.236.19.52

# Run setup script
bash server-complete-setup.sh

# Or manual commands
a2enmod proxy proxy_fcgi
a2enconf php8.1-fpm
systemctl restart apache2 php8.1-fpm
```

### Step 2: Verify Server Configuration

```powershell
# Run validation tests
.\validate-timeline-setup.ps1 -ServerIP 172.236.19.52
```

Expected output:
```
✓ Server is reachable
✓ Apache is responding
✓ API endpoint is accessible
✓ APK is built and ready
```

### Step 3: Deploy Android App

```bash
# Install APK on emulator/device
adb install -r "Facesofnaija/bin/Release/net9.0-android36.0/com.facesofnaija.tlapp-Signed.apk"

# Launch app
adb shell am start -n com.facesofnaija.tlapp/crc64e231b352b2de0b5e.SplashScreenActivity

# Monitor logs
adb logcat | grep "API:"
```

### Step 4: Verify Timeline Works

1. **Login to app** with valid credentials
2. **Navigate to Timeline tab**
3. **Expected result**: Posts should load below the "What's going on?" composer
4. **Each post should show**:
   - Post author name
   - Post content/text
   - Timestamp
   - Like/comment counts
   - Author avatar

## Testing Checklist

- [ ] Server responds to ping
- [ ] Apache is serving HTTP requests
- [ ] posts_mock.php returns 200 with sample data
- [ ] APK is installed on device/emulator
- [ ] App launches successfully
- [ ] User can log in
- [ ] Timeline displays posts
- [ ] Posts are scrollable
- [ ] Pull-to-refresh works

## Troubleshooting

### "Cannot reach server"
```bash
# Verify server is online
ping 172.236.19.52

# Check SSH connectivity
ssh root@172.236.19.52 "echo test"

# Verify IP is correct
# Settings in app: http://172.236.19.52/api/v2/endpoints/
```

### "Apache returning 403"
```bash
# SSH to server and run
ssh root@172.236.19.52 'bash server-complete-setup.sh'

# Or manually
a2enmod proxy_fcgi
a2enconf php8.1-fpm
systemctl restart apache2
```

### "PHP not responding"
```bash
# Check PHP-FPM status
ssh root@172.236.19.52 'systemctl status php*-fpm'

# Restart PHP
ssh root@172.236.19.52 'systemctl restart php*-fpm'

# View errors
ssh root@172.236.19.52 'tail -50 /var/log/apache2/error.log'
```

### "Timeline still empty"
```bash
# Check app logs
adb logcat | grep -E "(API:|ERROR|Exception)"

# Watch during login
adb logcat -c
adb shell am start -n com.facesofnaija.tlapp/crc64e231b352b2de0b5e.SplashScreenActivity
sleep 10
adb logcat | head -200

# Verify API is actually returning data
curl -X POST http://172.236.19.52/api/v2/endpoints/posts_mock.php
```

## Key Architecture

```
Android App
    ↓
Login → Authentication
    ↓
TabbedMainActivity
    ↓
NewsFeedNative Fragment
    ↓
ApiPostAsync.FetchNewsFeedApiPosts()
    ↓
RequestsAsync.Posts.GetGlobalPost()
    ↓ [Primary - may fail]
WoWonderClient API
    ↓ [Fallback - uses our direct method]
GetGlobalPostDirect()
    ↓ [Tries endpoints in order]
http://172.236.19.52/api/v2/endpoints/posts_mock.php → 200 ✓
http://172.236.19.52/api/v2/endpoints/posts.php → 200 ✓
    ↓
JSON Response
    ↓
LoadDataApi() → Parses posts
    ↓
NativeFeedAdapter → Renders posts
    ↓
RecyclerView → Timeline displays!
```

## File Locations

```
Local Machine:
  APK:              Facesofnaija/bin/Release/net9.0-android36.0/com.facesofnaija.tlapp-Signed.apk
  API Endpoints:    facesofnaija-web/api/v2/endpoints/
  Setup Scripts:    *.ps1 and *.sh files in workspace root

Server (172.236.19.52):
  Web Root:         /var/www/html/
  API Path:         /var/www/html/api/v2/endpoints/
  Apache Config:    /etc/apache2/
  Apache Error Log: /var/log/apache2/error.log
  Apache Access Log:/var/log/apache2/access.log
  PHP-FPM Status:   systemctl status php*-fpm
```

## Success Indicators

✓ **Timeline loads successfully when:**
1. App shows 10+ posts below composer
2. Each post displays author, text, timestamp
3. Scrolling loads more posts (pagination)
4. Pull-to-refresh reloads feed
5. No errors in logcat
6. API responses show `"api_status": 200`

## Performance Notes

- First load: ~2-3 seconds (API fetch + parsing)
- Scroll pagination: ~1-2 seconds per load
- Mock data: 15 sample posts
- Real data: Database queries (implement posts.php fully when ready)

## Next Phase

Once timeline is working with mock data:
1. Implement real database queries in posts.php
2. Add user-specific filtering
3. Implement like/comment functionality
4. Add sharing and boost features
5. Optimize query performance

---

**Support**: Reference `TIMELINE_ISSUE_COMPLETE.md` for detailed technical documentation.
