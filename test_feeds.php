<?php
// Test the phone API feed endpoint
$url = "http://172.236.19.52/app_api.php?application=phone&type=get_news_feed";
$data = array(
    "type" => "get_news_feed",
    "access_token" => "test",
    "user_id" => "1",
    "s" => "test",
    "server_key" => "",
    "limit" => "3",
    "offset" => "0"
);

$ch = curl_init($url);
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, http_build_query($data));
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 10);

$response = curl_exec($ch);
$httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
curl_close($ch);

echo "HTTP Code: $httpCode\n";
echo "Response:\n";
echo substr($response, 0, 500) . "\n";

// Also test v2 endpoint
echo "\n\n=== Testing v2 endpoint ===\n";
$ch = curl_init("http://172.236.19.52/api-v2.php?type=get_news_feed&access_token=test");
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, "type=get_news_feed&access_token=test&limit=3&after_post_id=0");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_TIMEOUT, 10);
$response = curl_exec($ch);
$httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
curl_close($ch);
echo "HTTP Code: $httpCode\n";
echo "Response:\n";
echo substr($response, 0, 500) . "\n";
