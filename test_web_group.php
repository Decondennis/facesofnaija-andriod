<?php
$ch = curl_init("http://172.236.19.52/requests.php?f=groups&s=create_group");
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, "group_name=TestGroup123&group_title=Test+Group&about=This+is+a+test&category=1&privacy=1");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 15);
curl_setopt($ch, CURLOPT_COOKIE, "PHPSESSID=test123");
$r = curl_exec($ch);
echo "HTTP: " . curl_getinfo($ch, CURLINFO_HTTP_CODE) . "\n";
echo "Body: " . ($r ?? "NULL") . "\n";
curl_close($ch);
