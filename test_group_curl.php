<?php
$ch = curl_init("http://172.236.19.52/requests.php?f=groups&s=create_group");
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_HTTPHEADER, array("X-Requested-With: XMLHttpRequest"));
curl_setopt($ch, CURLOPT_POSTFIELDS, "group_name=TestGroupABC123&group_title=Test+Group&about=Testing&category=1&privacy=1");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 10);
$r = curl_exec($ch);
echo "HTTP: " . curl_getinfo($ch, CURLINFO_HTTP_CODE) . "\n";
echo "Body: " . ($r ?? "NULL") . "\n";
curl_close($ch);
