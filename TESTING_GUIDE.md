# Parity Testing & Verification Guide

## Overview
This guide provides step-by-step testing procedures to verify that the Android app properly aligns with the web app's API behavior after the implemented fixes.

---

## Part 1: Pre-Testing Setup

### Requirements
- Android Emulator (AVD): `Medium_Phone_API_36.1` or similar
- Android Studio with current project
- ADB installed and configured
- Test user account on server `172.236.19.52`
- Network connectivity to server

### Environment Verification
```bash
# Check server connectivity
adb shell ping 172.236.19.52

# Verify app is built
dotnet build Facesofnaija/Facesofnaija.csproj
```

### Test Accounts (Required)
Create these test accounts on the web app if they don't exist:

| Username | Password | Status | Purpose |
|----------|----------|--------|---------|
| testuser1 | Test@123 | Active | Standard login |
| testuser2 | Test@456 | Banned | Error code 7 testing |
| ratelimit | Test@789 | Active | Rate limit testing (attempt 10 failed logins) |

---

## Part 2: Critical Path Testing

### Test Case 1: Login with Username
**Objective**: Verify username-based authentication works

**Steps**:
1. Launch app and wait for login screen
2. Enter username: `testuser1`
3. Enter password: `Test@123`
4. Click "Login"

**Expected Result**:
- ✓ Login succeeds
- ✓ App navigates to timeline/feed
- ✓ User profile loads correctly
- ✓ No "incorrect password" error

**Verification**:
```csharp
// In logcat, should see:
// "Auth attempt: ... with 'username' -> Status: 200"
// NOT: "Auth attempt: ... with 'email' -> Status: ..."
```

**Debug Commands**:
```bash
adb logcat | grep "Auth attempt"
adb logcat | grep "api_status"
```

---

### Test Case 2: Invalid Password Error
**Objective**: Verify error code 5 is properly handled

**Steps**:
1. Enter username: `testuser1`
2. Enter password: `WrongPassword`
3. Click "Login"

**Expected Result**:
- ✓ Login fails
- ✓ Dialog shows: "Password is incorrect"
- ✓ User remains on login screen
- ✓ Can retry login

**Verification**:
```bash
adb logcat | grep "Password is incorrect"
```

---

### Test Case 3: Username Not Found Error
**Objective**: Verify error code 3 handling

**Steps**:
1. Enter username: `nonexistentuser123`
2. Enter password: `AnyPassword`
3. Click "Login"

**Expected Result**:
- ✓ Dialog shows: "Username not found"
- ✓ User remains on login screen

---

### Test Case 4: Rate Limiting Error
**Objective**: Verify error code 6 handling (requires server-side setup)

**Steps**:
1. Attempt login with correct credentials
2. Deliberately fail login 10 times in rapid succession
3. Next login attempt should trigger rate limit

**Expected Result**:
- ✓ Dialog shows: "Too many login attempts. Please try again later."
- ✓ Cannot login for cooldown period

**Verification**:
```bash
adb logcat | grep "Too many login attempts"
```

---

### Test Case 5: Banned User Error
**Objective**: Verify error code 7 handling

**Steps**:
1. Enter username: `testuser2` (pre-banned account)
2. Enter password: `Test@456`
3. Click "Login"

**Expected Result**:
- ✓ Dialog shows: "This user account is banned"
- ✓ Cannot proceed to feed

---

### Test Case 6: Device Registration
**Objective**: Verify device_type and android_m_device_id are sent

**Steps**:
1. Clear app data
2. Perform login
3. Check server database (or network trace)

**Expected Verification**:
```bash
# In network trace, POST to auth endpoint should include:
device_type=phone
android_m_device_id=<ANDROID_ID>

# Verify via server logs:
SELECT android_m_device_id FROM users WHERE username='testuser1' LIMIT 1;
```

---

### Test Case 7: Input Trimming
**Objective**: Verify whitespace is handled correctly

**Steps**:
1. Enter username: `  testuser1  ` (with leading/trailing spaces)
2. Enter password: `  Test@123  ` (with spaces)
3. Click "Login"

**Expected Result**:
- ✓ Login succeeds (credentials are trimmed)
- ✓ App shows feed

---

## Part 3: Feed Loading Tests

### Test Case 8: Timeline Feed Loads
**Objective**: Verify GetGlobalPostDirect fallback works

**Prerequisites**:
- User is logged in as testuser1
- Network is active

**Steps**:
1. Navigate to timeline/feed
2. Wait for posts to load
3. Scroll down to trigger pagination
4. Verify posts load with offset/pagination

**Expected Result**:
- ✓ Posts load in 2-3 seconds
- ✓ Multiple posts visible
- ✓ Scrolling loads more posts

**Verification**:
```bash
adb logcat | grep "FetchFeedPostsApi"
adb logcat | grep "GetGlobalPostDirect"
```

---

### Test Case 9: Empty Feed Handling
**Objective**: Verify app handles empty feed gracefully

**Prerequisites**:
- Test account with no friends/groups

**Steps**:
1. Log in with new test account
2. Navigate to feed
3. Verify empty state is shown

**Expected Result**:
- ✓ Empty state message shown
- ✓ No crash or spinner loop
- ✓ Can perform swipe-to-refresh

---

## Part 4: Network Resilience Tests

### Test Case 10: Network Disconnection Recovery
**Objective**: Verify app recovers from network loss

**Steps**:
1. Start login
2. Disable network (emulator network settings)
3. Click login
4. Wait 5 seconds
5. Re-enable network
6. Verify recovery

**Expected Result**:
- ✓ Shows network error message
- ✓ Allows retry without restarting app
- ✓ Successfully logs in after network restored

---

### Test Case 11: Server Unavailability
**Objective**: Verify graceful degradation if server is down

**Steps**:
1. Stop web server or point to invalid IP
2. Attempt login
3. Wait for timeout

**Expected Result**:
- ✓ Shows appropriate error within 30 seconds
- ✓ Error message is helpful
- ✓ No ANR (Application Not Responding)

---

## Part 5: Data Model Validation

### Test Case 12: Post Data Integrity
**Objective**: Verify post fields are properly parsed

**Prerequisites**:
- Logged in and timeline visible
- Post is displayed

**Steps**:
1. Long-press a post to view details
2. Check that all fields display correctly:
   - Post text
   - Author name
   - Avatar/profile picture
   - Timestamp
   - Like/comment counts

**Expected Result**:
- ✓ All fields render correctly
- ✓ Images load properly
- ✓ No missing or corrupted data

**Debug**:
```bash
# Check JSON parsing in logs
adb logcat | grep "PostDataObject"
```

---

## Part 6: Automated Testing (Optional)

### Unit Test Template
```csharp
[TestMethod]
public async Task LoginWithUsername_ShouldSucceed()
{
    // Arrange
    var auth = new SocialLoginBaseActivity();
    var username = "testuser1";
    var password = "Test@123";
    
    // Act
    var (status, response) = await auth.TryAuthDirectAsync(username, password);
    
    // Assert
    Assert.AreEqual(200, status);
    Assert.IsInstanceOfType(response, typeof(AuthObject));
}

[TestMethod]
public async Task LoginWithInvalidPassword_ShouldReturnError5()
{
    // Arrange
    var auth = new SocialLoginBaseActivity();
    
    // Act
    var (status, response) = await auth.TryAuthDirectAsync("testuser1", "WrongPassword");
    
    // Assert
    Assert.AreEqual(400, status);
    if (response is ErrorObject error)
        Assert.AreEqual("5", error.Error.ErrorId);
}
```

---

## Part 7: Performance Metrics

### Baseline Metrics to Track

| Metric | Target | Method |
|--------|--------|--------|
| Login Time | < 5s | Measure from click to feed view |
| Feed Load Time | < 3s | Measure from screen open to posts visible |
| Scroll Performance | 60 FPS | Use Android Profiler |
| Memory Usage | < 250MB | Monitor via Android Profiler |
| Network Efficiency | < 500KB per load | Monitor via Network Profiler |

### Measurement Commands
```bash
# CPU/Memory profiling
adb shell dumpsys meminfo | grep facesofnaija

# Network traffic
adb shell tcpdump -i any host 172.236.19.52 -w /data/local/tmp/capture.pcap

# Performance metrics
adb shell am dumpheaps facesofnaija.facesofnaija /data/local/tmp/heap.hprof
```

---

## Part 8: Test Report Template

Create a file: `TEST_RESULTS_[DATE].md`

```markdown
# Test Results - [DATE]

## Environment
- Device: [AVD name]
- OS: Android 16
- Build: [Build number]
- Server: 172.236.19.52

## Critical Tests
- [ ] Test 1: Login with Username - PASS/FAIL
- [ ] Test 2: Invalid Password Error - PASS/FAIL
- [ ] Test 3: Username Not Found - PASS/FAIL
- [ ] Test 4: Rate Limiting - PASS/FAIL
- [ ] Test 5: Banned User - PASS/FAIL

## Feed Tests
- [ ] Test 8: Timeline Loads - PASS/FAIL
- [ ] Test 9: Empty Feed - PASS/FAIL

## Issues Found
1. [Description]
   - Steps to reproduce
   - Expected vs actual
   - Severity: Critical/High/Medium/Low

## Sign-Off
- Tester: [Name]
- Date: [Date]
- Status: APPROVED / NEEDS FIXES
```

---

## Troubleshooting Guide

### "Incorrect Password" Still Shows
**Possible Causes**:
1. Username being sent as email key instead
   - Check: `adb logcat | grep "with 'username'"`
   - Fix: Verify TryAuthDirectAsync has username first in array
   
2. Server is using different password hashing
   - Check: Compare password on server vs app input
   - Fix: Ensure same password in test accounts

3. Credential trimming not working
   - Check: `adb logcat | grep "emailOrUsername"`
   - Fix: Verify `.Trim()` is called in LoginActivity

### "Feed Not Loading"
**Possible Causes**:
1. Access token not stored
   - Check: `adb logcat | grep "access_token"`
   - Fix: Verify SetDataLogin is called after successful auth

2. Get GlobalPostDirect not being called
   - Check: `adb logcat | grep "GetGlobalPostDirect"`
   - Fix: Ensure FetchFeedPostsApi includes fallback

3. Server endpoint changed
   - Check: Network trace for actual URL
   - Fix: Update URL in GetGlobalPostDirect

### ANR or App Freeze
**Debug Steps**:
1. Capture ANR trace: `adb bugreport > anr.zip`
2. Check logcat for blocking calls
3. Review: `TabbedMainActivity.OnCreate()` for heavy workloads
4. Reduce API timeout or implement cancellation tokens

---

## Sign-Off Checklist

Before deploying to production:

- [ ] All critical tests pass
- [ ] No new ANRs or crashes
- [ ] Login works with username and email
- [ ] Error messages are clear and helpful
- [ ] Feed loads within target time
- [ ] Device registration confirmed on server
- [ ] Password trimming verified
- [ ] No sensitive data in logs
- [ ] Network error handling works
- [ ] Performance metrics acceptable

---

**Testing Responsible**: [QA Team]  
**Start Date**: [Date]  
**Completion Target**: [Date]  
**Status**: Ready for Testing

// CORRECT - proper null check first, then access
if (task != null && (task.IsCompleted == false || task.Status == TaskStatus.Running))
{
    Console.WriteLine("API = Task already running, returning...");
    return null;
}

