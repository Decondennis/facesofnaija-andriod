# Facesofnaija App - Emulator Deployment Guide

## ✅ Status: Emulator Ready!

**Emulator:** Medium_Phone_API_36.1
**Device ID:** emulator-5554
**Status:** Online and ready

---

## How to Deploy the App

### Method 1: Deploy from Visual Studio (Recommended)

1. **Open Visual Studio** with the Facesofnaija project
2. **Select the Android Emulator:**
   - At the top toolbar, click the dropdown next to the green "Run" button
   - Select "Medium_Phone_API_36.1" (or emulator-5554)
3. **Click the Green Play Button** or press **F5**
   - Visual Studio will build and deploy the app
   - The app will automatically launch in the emulator
4. **Wait for deployment** (first time takes 2-3 minutes)

### Method 2: Deploy via Command Line

If you prefer command line deployment:

```powershell
# Navigate to project directory
cd "C:\Users\Dell\source\repos\workspace\Facesofnaija-andriod"

# Build the project
dotnet build Facesofnaija\Facesofnaija.csproj -c Debug

# Install APK to emulator
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" install -r "Facesofnaija\bin\Debug\net9.0-android36.0\com.facesofnaija.tlapp-Signed.apk"

# Launch the app
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" shell am start -n com.facesofnaija.tlapp/.SplashScreenActivity
```

---

## What to Test

### 1. App Launch & Authentication
- ✅ App should launch without crashes
- ✅ Splash screen displays
- ✅ Login screen appears
- ⚠️ **Note:** You'll need valid credentials or configure local server

### 2. API Configuration (IMPORTANT)

Before testing, update the API URL to point to your local web server:

**Option A: Point to Local Web Server**
1. Find your local IP address:
   ```powershell
   ipconfig | findstr "IPv4"
   ```
2. Update in code (one of these files):
   - `Facesofnaija/Helpers/Utils/InitializeWoWonder.cs`
   - Or check `AppSettings.cs` for API configuration

**Example Configuration:**
```csharp
// For emulator accessing localhost
public static string WebsiteUrl = "http://10.0.2.2/facesofnaija-web";

// OR for physical device (use your local IP)
public static string WebsiteUrl = "http://192.168.1.100/facesofnaija-web";

public static string ServerKey = "YOUR_SERVER_KEY_HERE";
```

### 3. Test Video Playback

Once logged in, navigate to posts with videos:

**Feed Video Test:**
- Scroll to video post
- Tap play button
- Verify video loads and plays
- Test pause, seek controls
- Check progress bar updates

**Full-Screen Video Test:**
- Tap video to open full screen
- Test play/pause
- Test seek bar
- Test back button
- Check time display updates

**Reels Video Test:**
- Navigate to Reels section
- Swipe through reels
- Verify video auto-plays
- Test like/comment buttons
- Check user info displays

### 4. Test Audio Playback

Navigate to audio posts (sound posts):

**Audio Post Test:**
- Find audio/sound post
- Tap play button
- Verify audio plays
- Test pause/resume
- Test skip forward/backward (10s)
- Test seek bar
- Check time displays
- Test next/previous track (if playlist)

### 5. Test UI Components

**Progress Dialogs (AndroidHUD Replacement):**
- Trigger loading actions
- Verify progress dialog appears
- Check it dismisses properly
- Test success/error toasts

**Scroll Performance:**
- Scroll through feed
- Verify infinite scroll works
- Check load more triggers
- Verify no crashes on fast scroll

---

## Troubleshooting

### Emulator Not Appearing in Visual Studio

1. **Restart Visual Studio**
2. **Refresh Device List:**
   - Tools → Android → Android Device Manager
   - Click refresh button
3. **Restart ADB:**
   ```powershell
   & "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" kill-server
   & "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" start-server
   ```

### App Crashes on Launch

1. **Check Logs:**
   ```powershell
   & "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" logcat -s "Facesofnaija"
   ```
2. **Clear App Data:**
   ```powershell
   & "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" shell pm clear com.facesofnaija.tlapp
   ```
3. **Reinstall:**
   ```powershell
   & "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" uninstall com.facesofnaija.tlapp
   ```
   Then redeploy from Visual Studio

### Video/Audio Not Playing

**Check Network:**
- Ensure emulator has internet access
- Verify API URL is accessible from emulator
- Test URL in emulator browser

**Check URLs:**
- Video URLs should be full URLs (https://...)
- Or relative URLs that get base URL prepended
- Check VideoPlayerController logs for errors

**Enable Logs:**
```powershell
# Filter for MediaPlayer errors
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" logcat -s "MediaPlayer"
```

### API Connection Issues

1. **Verify Web Server Running:**
   - Open `http://localhost/facesofnaija-web` in browser
   - Should see the web app

2. **Test from Emulator:**
   - Open Chrome in emulator
   - Navigate to `http://10.0.2.2/facesofnaija-web`
   - Should load the web site

3. **Check Server Key:**
   - Login to web app admin panel
   - Go to Settings → API Settings
   - Copy the Windows App API Key
   - Update in Android app code

---

## Debugging Tips

### View Logs in Real-Time

**All Logs:**
```powershell
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" logcat
```

**Filtered Logs:**
```powershell
# App errors only
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" logcat *:E

# Video playback
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" logcat -s "VideoPlayerController"

# Audio playback
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" logcat -s "AudioPlayerController"

# Network requests
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" logcat -s "OkHttp"
```

### Debug Build vs Release Build

**For Testing:** Use **Debug** build
- Includes debug symbols
- Easier to trace errors
- Shows more detailed logs

**For Performance Testing:** Use **Release** build
- Optimized performance
- Smaller APK size
- Production-like behavior

---

## Performance Monitoring

### Check Memory Usage

```powershell
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" shell dumpsys meminfo com.facesofnaija.tlapp
```

### Check CPU Usage

```powershell
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" shell top -n 1 | findstr "facesofnaija"
```

### Monitor Network Traffic

```powershell
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" shell tcpdump -i any -s 0 -w - | wireshark -k -i -
```

---

## Next Steps After Testing

### 1. Configure API URL

**File to Update:** Find where API URL is configured
- Likely in `InitializeWoWonder.cs` or similar
- Update `WebsiteUrl` to your server
- Update `ServerKey` with actual key

### 2. Test All Features

Create a test checklist:
- [ ] Login/Register
- [ ] View feed
- [ ] Create post (text/image/video/audio)
- [ ] Like/Comment/Share
- [ ] Video playback (feed, fullscreen, reels)
- [ ] Audio playback
- [ ] Messaging
- [ ] User profile
- [ ] Settings
- [ ] Search
- [ ] Notifications

### 3. Web API Integration

Based on WEB_API_ANALYSIS.md:
- [ ] Verify PostDataObject matches web API
- [ ] Test post loading with real data
- [ ] Verify media URLs resolve correctly
- [ ] Test authentication flow
- [ ] Test all API endpoints

### 4. UI/UX Alignment

- [ ] Compare with web app UI
- [ ] Match color schemes
- [ ] Align navigation patterns
- [ ] Update icons if needed
- [ ] Test on different screen sizes

### 5. Performance Optimization

- [ ] Profile memory usage
- [ ] Optimize video caching
- [ ] Reduce APK size
- [ ] Optimize database queries
- [ ] Test on low-end devices

---

## Emulator Commands Reference

### Start/Stop Emulator

```powershell
# Start emulator
Start-Process -FilePath "C:\Users\Dell\AppData\Local\Android\Sdk\emulator\emulator.exe" -ArgumentList "-avd","Medium_Phone_API_36.1"

# Stop emulator
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" -s emulator-5554 emu kill
```

### Take Screenshots

```powershell
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" shell screencap -p /sdcard/screenshot.png
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" pull /sdcard/screenshot.png
```

### Record Screen

```powershell
# Start recording
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" shell screenrecord /sdcard/demo.mp4

# Stop recording (Ctrl+C after done)
# Pull video
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" pull /sdcard/demo.mp4
```

### Push/Pull Files

```powershell
# Push file to emulator
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" push "local-file.txt" "/sdcard/"

# Pull file from emulator
& "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe" pull "/sdcard/remote-file.txt" "."
```

---

## Summary

✅ **Build Status:** Successful (0 errors)
✅ **Emulator Status:** Online (emulator-5554)
✅ **Video Layouts:** Created (3 files)
✅ **MediaPlayer:** Implemented and ready
✅ **Web API:** Analyzed and documented

**Ready to Deploy!**

1. Press F5 in Visual Studio
2. Wait for app to launch in emulator
3. Test video/audio playback
4. Review logs for any issues
5. Configure API URL for real data

**Good luck with testing! 🚀**

---

*Deployment Guide created: January 2025*
*Platform: .NET 9 + Android API 36*
*Emulator: Medium Phone API 36.1*
