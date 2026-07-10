<?php
$url = "http://172.236.19.52/app_api.php?application=phone&type=user_login";
$ch = curl_init($url);
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, "username=facesofnaija&password=test123");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 10);
$r = curl_exec($ch);
echo "HTTP: " . curl_getinfo($ch, CURLINFO_HTTP_CODE) . "\n";
echo "Body: [" . ($r ?? "NULL") . "]\n";
echo "Len: " . strlen($r ?? "") . "\n";
curl_close($ch);
