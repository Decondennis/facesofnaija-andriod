# Timeline Deployment - Quick Start Guide

## ✓ Server is READY - 172.236.19.52

The backend API is now configured and responding with sample posts!

```
✓ Apache running
✓ PHP 8.3.6 enabled
✓ posts_mock.php responding with 3 sample posts
✓ API returns: HTTP 200, api_status: 200
```

## Deploy Android App in 3 Commands

### 1. Install APK
```bash
adb install -r "C:\Users\Dell\source\repos\workspace\Facesofnaija-andriod\Facesofnaija\bin\Release\net9.0-android36.0\com.facesofnaija.tlapp-Signed.apk"
```

### 2. Launch App
```bash
adb shell am start -n com.facesofnaija.tlapp/crc64e231b352b2de0b5e.SplashScreenActivity
```

### 3. Monitor Timeline
```bash
adb logcat | grep "API:"
```

## What You'll See

**Timeline will show:**
- ✓ Story section at top
- ✓ "What's going on?" composer
- ✓ **3 sample posts with content & timestamps**
- ✓ Like/comment counts
- ✓ Scrollable feed

## If Timeline is Empty

```bash
# 1. Check if API is responding
curl -X POST http://172.236.19.52/api/v2/endpoints/posts_mock.php

# 2. Check server logs
ssh root@172.236.19.52 'tail -20 /var/log/apache2/error.log'

# 3. Verify .htaccess
ssh root@172.236.19.52 'cat /var/www/html/api/v2/endpoints/.htaccess'
```

## Server Credentials
- **IP**: 172.236.19.52
- **User**: root
- **Password**: Chsom.22

## Test Results

✓ **Server Test**:
```
HTTP Status: 200 OK
API Status: 200
Posts Returned: 3
Sample Post: "Sample Post 1"
```

✓ **Endpoint**: http://172.236.19.52/api/v2/endpoints/posts_mock.php

✓ **Ready**: YES - Proceed with APK deployment

---

**Next**: Install APK → Launch app → Check timeline!
