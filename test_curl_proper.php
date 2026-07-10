<?php
$ch = curl_init("http://172.236.19.52/requests.php?f=pages&s=create_page");
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_HTTPHEADER, array(
    "X-Requested-With: XMLHttpRequest",
    "Referer: http://172.236.19.52/create-page"
));
curl_setopt($ch, CURLOPT_POSTFIELDS, "page_name=TestPageABC&page_title=Test+Page&page_category=1&page_description=Testing&hash_id=test");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 10);
curl_setopt($ch, CURLOPT_COOKIE, "PHPSESSID=test123");
$r = curl_exec($ch);
echo "HTTP: " . curl_getinfo($ch, CURLINFO_HTTP_CODE) . "\n";
echo "Body: " . ($r ?? "NULL") . "\n";
curl_close($ch);
