# Timeline Loading - Complete Testing & Verification Guide

## Current Status: AWAITING SERVER CONFIGURATION

The Android app and API endpoints are ready. **The only blocker is Apache PHP configuration.**

---

## Phase 1: Server Configuration (DO THIS FIRST)

### For Ubuntu/Debian Linux with Apache:

```bash
# SSH into your server
ssh user@172.236.19.52

# Run as root or with sudo
sudo su

# Enable PHP-FPM with Apache
a2enmod proxy
a2enmod proxy_fcgi
a2enconf php8.1-fpm  # Adjust version to your PHP version

# Test configuration
apache2ctl -t
# Expected: Syntax OK

# Restart Apache
systemctl restart apache2

# Verify status
systemctl status apache2
```

### For Windows with XAMPP:

```powershell
# Run PowerShell as Administrator
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Execute the fix script
.\fix-xampp-php.ps1
```

### Quick Test (Any OS):

```bash
# From your development machine:
curl -X POST http://172.236.19.52/api/v2/endpoints/posts_mock.php

# Expected response:
# {"api_status":200,"data":[...posts...],"count":15,"message":"Success"}
```

---

## Phase 2: Verify Files Are in Place

### Check these files exist on the server:

```bash
# SSH into server
ssh user@172.236.19.52

# Check files
ls -la /var/www/html/api/v2/endpoints/
# Should show:
# -rw-r--r-- posts.php
# -rw-r--r-- posts_mock.php

# View first few lines of posts_mock.php
head -20 /var/www/html/api/v2/endpoints/posts_mock.php
# Should show: <?php header('...');

# Check permissions
chmod 644 /var/www/html/api/v2/endpoints/posts*.php
chown www-data:www-data /var/www/html/api/v2/endpoints/posts*.php
```

---

## Phase 3: Test API Directly

### Test Mock Endpoint:

```bash
curl -X POST http://172.236.19.52/api/v2/endpoints/posts_mock.php \
  -d "type=get_news_feed&limit=5&access_token=test" \
  -H "Content-Type: application/x-www-form-urlencoded"

# Expected response includes:
# "api_status": 200
# "count": 5
# "data": [...]
```

### Test Real Endpoint:

```bash
curl -X POST http://172.236.19.52/api/v2/endpoints/posts.php \
  -d "type=get_news_feed&limit=5&access_token=test" \
  -H "Content-Type: application/x-www-form-urlencoded"

# Initially might fail if database not set up - that's ok for now
```

---

## Phase 4: Test on Android Emulator

### Emulator is Ready with Updated APK:

```bash
# The current APK (com.facesofnaija.tlapp-Signed.apk) includes:
# ✓ Enhanced logging for diagnostics
# ✓ Mock endpoint fallback
# ✓ Better error handling

# If not already running, start fresh instance:
adb kill-server
adb start-server

# Check emulator can reach server
adb shell ping -c 3 172.236.19.52

# If ping works, the app should be able to contact the API
```

### Monitor Feed Loading:

```bash
# In one terminal, start logcat monitoring:
adb logcat | grep -E "(API:|DEBUG|posts_mock|timeline)"

# In another terminal, simulate app usage or test directly:
# 1. Log in to the app
# 2. Navigate to Timeline tab
# 3. Watch logcat output - you should see:
#    - "API: Starting FetchNewsFeedApiPosts"
#    - "DEBUG LoadDataApi: apiStatus=200"
#    - "DEBUG LoadDataApi: Posts before RemoveAll: 15"
#    - Post IDs being added
```

---

## Phase 5: Test on Physical Device

Once the emulator test succeeds:

```bash
# Connect physical device via ADB
adb devices -l

# Install APK on device
adb install -r Facesofnaija/bin/Release/net9.0-android36.0/com.facesofnaija.tlapp-Signed.apk

# Start app
adb shell am start -n com.facesofnaija.tlapp/crc64e231b352b2de0b5e.SplashScreenActivity

# Monitor logs
adb logcat | grep -E "(API:|DEBUG|Exception)" | head -100

# Take screenshot to verify posts are displayed
adb shell screencap -p /sdcard/timeline.png
adb pull /sdcard/timeline.png
```

---

## Troubleshooting Guide

### Issue: "403 Forbidden" from Apache

**Cause**: PHP-FPM not configured with Apache

**Solution**:
```bash
# Linux
a2enmod proxy_fcgi
a2enconf php8.1-fpm
systemctl restart apache2

# Windows (XAMPP)
# Restart Apache from XAMPP Control Panel
```

### Issue: "500 Internal Server Error"

**Cause**: PHP syntax error or missing dependencies

**Solution**:
```bash
# Check Apache error log
tail -50 /var/log/apache2/error.log

# Check PHP syntax
php -l /var/www/html/api/v2/endpoints/posts_mock.php

# Check PHP extensions (if using real posts.php with database)
php -m | grep -i pdo
```

### Issue: "Connection refused"

**Cause**: Emulator can't reach server

**Solution**:
```bash
# From emulator
adb shell ping 172.236.19.52

# From host, verify server is running
ping 172.236.19.52

# If using local XAMPP on Windows
# Make sure Windows Defender isn't blocking Apache
# Check Settings > Firewall > Allow an app > Apache HTTP Server
```

### Issue: "No posts showing in Android app despite API returning data"

**Cause**: Data model mismatch

**Solution**:
```bash
# 1. Check logcat for parse errors:
adb logcat | grep -i "parse\|json\|exception"

# 2. Verify API response format matches PostObject model
# Expected minimum fields:
# post_id, post_text, time, publisher (with id, name, avatar)
# user_data (with id, name, avatar)

# 3. Check the GetGlobalPostDirect parse logic in ApiPostAsync.cs
# Line around: var posts = jObject.ToObject<PostObject>();
```

---

## Expected Results Timeline

### ✅ After Server is Fixed (5-10 minutes)

```
Timeline Load Flow:
1. User logs in → credentials validated
2. TabbedMainActivity loads → calls NewsFeedNative
3. NewsFeedNative.OnViewCreated → calls LoadPost()
4. LoadPost → calls StartApiService()
5. StartApiService → calls FetchNewsFeedApiPosts()
6. FetchNewsFeedApiPosts → calls RequestsAsync.Posts.GetGlobalPost()
7. If fails → calls GetGlobalPostDirect() → tries posts_mock.php
8. posts_mock.php returns 200 with 15 sample posts
9. LoadDataApi processes posts → adds to adapter
10. NativeFeedAdapter.NotifyItemRangeInserted → RecyclerView renders posts
11. Timeline displays posts! ✓
```

### ✅ What User Sees

**Before Fix**:
- Splash screen
- Login screen
- Timeline with only:
  - Story section (empty)
  - "What's going on?" composer
  - Empty timeline below

**After Fix**:
- Splash screen  
- Login screen
- Timeline with:
  - Story section at top
  - "What's going on?" composer
  - **15 sample posts loaded** ✓
  - Post text, timestamps, likes, comments
  - Ability to scroll and load more posts

---

## Files Involved

| Component | File | Status |
|-----------|------|--------|
| API Endpoint (real) | `facesofnaija-web/api/v2/endpoints/posts.php` | ✅ Created |
| API Endpoint (mock) | `facesofnaija-web/api/v2/endpoints/posts_mock.php` | ✅ Created |
| Android App | `Facesofnaija/bin/Release/.../com.facesofnaija.tlapp-Signed.apk` | ✅ Built |
| Android Logic | `Facesofnaija/Activities/NativePost/Post/ApiPostAsync.cs` | ✅ Updated |
| Android Logging | Console.WriteLine in ApiPostAsync | ✅ Added |

---

## Verification Checklist

- [ ] Server is running and accessible at http://172.236.19.52
- [ ] Apache is serving files (test with index.html)
- [ ] PHP is enabled and working
- [ ] `posts_mock.php` returns HTTP 200
- [ ] `posts_mock.php` returns valid JSON with `api_status: 200`
- [ ] APK is built and signed
- [ ] APK is installed on test device/emulator
- [ ] App can be launched
- [ ] User can log in
- [ ] Logcat shows `API:` debug messages
- [ ] Timeline shows posts (not empty)
- [ ] Posts are scrollable and count updates
- [ ] Pull-to-refresh works
- [ ] Load more works (pagination)

---

## Success Metrics

✅ **Timeline loading issue is RESOLVED when:**

1. Posts appear below the composer on the timeline
2. At least 10+ posts are visible
3. Each post shows: text, timestamp, author name, likes/comments
4. Scrolling loads more posts
5. No NullReferenceException in logcat
6. No "API:" error messages in logs

---

## Next Phase: Production Implementation

Once testing succeeds with mock data:

1. Implement real database classes (`classes/Post.php`, `classes/User.php`)
2. Connect to actual database
3. Implement post filters and sorting
4. Remove mock endpoint dependency
5. Deploy to production

**See also**: `TIMELINE_DIAGNOSIS_COMPLETE.md` for detailed technical analysis.
