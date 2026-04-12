# Facesofnaija App - Storage Issue Fix

## ❌ Current Issue
The app cannot be installed on the emulator because:
- **Error**: `INSTALL_FAILED_INSUFFICIENT_STORAGE: Failed to override installation location`
- **App Size**: ~84MB APK
- **Emulator Storage**: 84% full (insufficient free space)

## ✅ Fixes Applied
1. **AndroidManifest.xml**: Fixed `appComponentFactory` from `"androidx"` to `"androidx.core.app.CoreComponentFactory"`
2. **Facesofnaija.csproj**: Added properties to prevent native library compression:
   - `AndroidStoreUncompressedFileExtensions=.so`
   - `EmbedAssembliesIntoApk=true`
3. **Package Format**: Changed from AAB to APK for debug builds

## 🔧 Solutions to Install the App

### Option 1: Wipe Emulator Data (Recommended)
This will clear all data and give you a fresh emulator with full storage:

```powershell
# Stop the emulator first, then run:
C:\Users\Dell\AppData\Local\Android\Sdk\emulator\emulator.exe -avd Medium_Phone_API_36.1 -wipe-data
```

Then deploy again:
```powershell
.\quick-deploy.ps1
```

### Option 2: Delete Unused Apps from Emulator
Remove apps you don't need from the emulator:

```powershell
# List all installed packages
adb shell pm list packages

# Uninstall apps you don't need (example)
adb uninstall com.example.someapp
```

### Option 3: Create New Emulator with More Storage
1. Open **Android Studio** → **Device Manager**
2. Click **Create Device**
3. Select a phone (e.g., Pixel 5)
4. Select **API Level 36** (Android 15)
5. Click **Show Advanced Settings**
6. Increase **Internal Storage** to **8GB** or more
7. Click **Finish**

### Option 4: Use a Physical Android Device
If you have an Android phone/tablet:

1. Enable Developer Options on your device:
   - Go to Settings → About Phone
   - Tap "Build Number" 7 times
   
2. Enable USB Debugging:
   - Go to Settings → Developer Options
   - Enable "USB Debugging"

3. Connect your device via USB

4. Verify connection:
   ```powershell
   adb devices
   ```

5. Deploy:
   ```powershell
   .\quick-deploy.ps1
   ```

## 📋 After Fixing Storage

Once you have enough storage, deploy the app:

```powershell
# Deploy and launch
.\quick-deploy.ps1
```

Or manually:

```powershell
# Clean build
dotnet clean Facesofnaija\Facesofnaija.csproj

# Build and install
dotnet build Facesofnaija\Facesofnaija.csproj -t:Install -f net9.0-android36.0

# Launch
adb shell am start -n com.facesofnaija.tlapp/crc64e231b352b2de0b5e.SplashScreenActivity

# View logs
adb logcat | Select-String "facesofnaija|AndroidRuntime|FATAL"
```

## 🎯 What Was Fixed

### 1. AndroidManifest.xml
**Before:**
```xml
android:appComponentFactory="androidx"
```

**After:**
```xml
android:appComponentFactory="androidx.core.app.CoreComponentFactory"
```

This fixes the ClassNotFoundException crash.

###2. Facesofnaija.csproj
Added for both Debug and Release configurations:
```xml
<AndroidStoreUncompressedFileExtensions>.so</AndroidStoreUncompressedFileExtensions>
<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
```

This prevents the "MUST be STORED" native library crash.

### 3. Debug Package Format
**Before:**
```xml
<AndroidPackageFormat>aab</AndroidPackageFormat>
```

**After:**
```xml
<AndroidPackageFormat>apk</AndroidPackageFormat>
```

APK format is better for debugging and testing.

## 📝 Next Steps

1. **Choose one of the storage fix options above**
2. **Run the deployment script**: `.\quick-deploy.ps1`
3. **App should launch successfully without crashing**

The app now has the correct configuration and will work once there's enough storage space!
