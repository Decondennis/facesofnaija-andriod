$adb = "C:\Users\Dell\AppData\Local\Android\Sdk\platform-tools\adb.exe"

$apkFile = Get-ChildItem -Recurse "Facesofnaija\bin\Release\*-Signed.apk" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $apkFile) {
    Write-Host "No signed APK found. Building now..." -ForegroundColor Yellow
    dotnet build "Facesofnaija\Facesofnaija.csproj" -c Release -r android-arm64 --no-restore
    $apkFile = Get-ChildItem -Recurse "Facesofnaija\bin\Release\*-Signed.apk" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
}

$apk = $apkFile.FullName
Write-Host "Using latest APK: $apk (Built: $($apkFile.LastWriteTime))" -ForegroundColor Cyan
Write-Host "Looking for connected device..." -ForegroundColor Yellow

$deviceSerial = $null

for ($attempt = 1; $attempt -le 30; $attempt++) {
    $lines = & $adb devices
    foreach ($line in $lines) {
        if ($line -match "^([^\s]+)\s+device$") {
            $deviceSerial = $matches[1]
            break
        }
    }
    if ($deviceSerial) { break }
    
    foreach ($line in $lines) {
        if ($line -match "^([^\s]+)\s+unauthorized$") {
            Write-Host "Device detected ($($matches[1])) but UNAUTHORIZED! Please unlock your phone and tap 'Allow USB debugging'." -ForegroundColor Red
        }
        if ($line -match "^([^\s]+)\s+offline$") {
            Write-Host "Device detected ($($matches[1])) but OFFLINE. Reconnecting..." -ForegroundColor DarkYellow
            & $adb reconnect
        }
    }
    
    Write-Host "Waiting for device... (attempt $attempt/30)" -ForegroundColor Gray
    Start-Sleep -Seconds 2
}

if (-not $deviceSerial) {
    Write-Host "ERROR: No authorized Android device found." -ForegroundColor Red
    Write-Host "Please ensure:" -ForegroundColor Yellow
    Write-Host "  1. USB cable is securely connected."
    Write-Host "  2. Phone USB mode is set to 'File Transfer' (MTP)."
    Write-Host "  3. 'USB Debugging' is enabled in Developer Options."
    Write-Host "  4. You accepted the 'Allow USB debugging' prompt on your phone screen."
    exit 1
}

Write-Host "Target Device: $deviceSerial" -ForegroundColor Green
Write-Host "Installing $apk..." -ForegroundColor Yellow
$installOutput = & $adb -s $deviceSerial install -r -d $apk 2>&1
Write-Host ($installOutput -join "`n")

Write-Host "Launching app on device..." -ForegroundColor Yellow
$launchOutput = & $adb -s $deviceSerial shell am start -n com.facesofnaija.tlapp/com.facesofnaija.tlapp.SplashScreenActivity 2>&1
Write-Host ($launchOutput -join "`n")

Write-Host "Verifying installed package..." -ForegroundColor Yellow
$pkgCheck = & $adb -s $deviceSerial shell pm list packages facesofnaija
Write-Host $pkgCheck

Write-Host "`n=== Deployment Finished ===" -ForegroundColor Green
