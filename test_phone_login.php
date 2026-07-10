<?php
$url = "http://172.236.19.52/app_api.php?application=phone&type=user_login";
$data = array("username" => "admin@facesofnaija.com", "password" => "admin123");
$ch = curl_init($url);
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, http_build_query($data));
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 10);
$response = curl_exec($ch);
$httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
curl_close($ch);
echo "HTTP: $httpCode\n";
echo "Response: " . substr($response ?? "NULL", 0, 500) . "\n";
