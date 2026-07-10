<?php
$ch = curl_init("http://172.236.19.52/api-v2.php?type=get-site-settings&access_token=test");
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, "type=get-site-settings&access_token=test");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 5);
$response = curl_exec($ch);
$httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
curl_close($ch);
echo "HTTP: $httpCode\n";
echo substr($response ?? "NULL", 0, 500) . "\n";
