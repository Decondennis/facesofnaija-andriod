# Timeline Loading - Quick Test Checklist

## Setup (1-2 minutes)

- [ ] Android Emulator running: `Medium_Phone_API_36.1`
- [ ] App installed: `com.facesofnaija.tlapp`
- [ ] ADB connected: `adb devices` shows device
- [ ] Network to server `172.236.19.52` available

## Test Procedure (5 minutes)

### Step 1: Start Fresh
```bash
# Clear app data and cache
adb shell pm clear com.facesofnaija.tlapp

# Launch app
adb shell am start -n com.facesofnaija.tlapp/crc64e231b352b2de0b5e.SplashScreenActivity

# Wait 5 seconds for login screen
```

### Step 2: Login
1. On login screen, enter:
   - **Username**: `testuser1`
   - **Password**: `Test@123`
2. Click **Login**
3. ⏱️ **Expected**: Navigate to timeline in < 5 seconds
4. ⏱️ **Expected**: No "incorrect password" errors

### Step 3: Verify Timeline Posts Load
1. After login, look at the timeline screen
2. ✅ **PASS**: Multiple posts visible (not just story + create box)
3. ✅ **PASS**: Posts display within 3 seconds
4. ✅ **PASS**: Post images load correctly
5. ✅ **PASS**: Post text and author names visible

### Step 4: Verify Scrolling & Pagination  
1. Scroll down in timeline
2. ✅ **PASS**: More posts load as you scroll
3. ✅ **PASS**: No crashes or ANRs during scroll
4. ✅ **PASS**: Smooth 60 FPS scrolling

### Step 5: Verify Refresh Works
1. Pull down to refresh (swipe down)
2. ✅ **PASS**: Loading animation appears
3. ✅ **PASS**: New posts fetch (< 3 seconds)
4. ✅ **PASS**: Posts update in feed

## Debug Logging (If Issues Found)

### Check if API calls are happening:
```bash
adb logcat -c  # Clear logs
# Do login + load timeline
adb logcat | grep -E "API|FetchFeed|LoadData" | head -20
```

### Expected log output:
```
API = Started FetchNewsFeedApi 0
API = LoadDataApi Start 0
API = Ended with offset 0With count of XX
```

### Check for errors:
```bash
adb logcat | grep -E "Error|Exception|failed|ERROR" | grep -i timeline
```

## Pass/Fail Criteria

### ✅ PASS (Timeline Fix Successful)
- [ ] At least 5+ posts visible on timeline
- [ ] Posts load within 3 seconds
- [ ] Login succeeds with username
- [ ] Scrolling shows more posts
- [ ] No crashes or ANRs
- [ ] No "Task already running" repeated messages

### ❌ FAIL (Needs More Work)
- [ ] Still only story + create box visible
- [ ] Timeline takes > 5 seconds to load
- [ ] Login fails with "incorrect password"
- [ ] Crashes when scrolling
- [ ] ANRs (app freezes)

## Quick Notes

**The Fix**: Corrected null-check logic in `FetchFeedPostsApi` (line 106)
- **Before**: Broke early on null task (backwards logic)
- **After**: Only breaks early if task is running (correct logic)

**Expected Impact**: Timeline posts should now load on first view + pagination

**If Still Not Working**: 
- Check logcat for API errors
- Verify server at 172.236.19.52 is accessible
- Try different test user (e.g., testuser2)
- Check network connectivity

---

## Test Report Template

```markdown
## Timeline Loading Test - [DATE]

**Tester**: [Your Name]  
**Device**: AVD Medium_Phone_API_36.1  
**Build**: com.facesofnaija.tlapp-Signed.apk  
**Server**: 172.236.19.52

### Results
- Login: [ ] PASS [ ] FAIL
- Posts Load: [ ] PASS [ ] FAIL  
- Timeline Visible: [ ] PASS [ ] FAIL
- Pagination: [ ] PASS [ ] FAIL
- No Crashes: [ ] PASS [ ] FAIL

### Notes
[Any issues found, logs, observations]

### Recommendation
[ ] READY FOR RELEASE
[ ] NEEDS MORE FIXES
```

---

**Estimated Test Time**: 5-10 minutes  
**Success Indicator**: Multiple posts visible on timeline within 3 seconds of login

