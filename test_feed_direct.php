<?php
$_GET["type"] = "get_news_feed";
$_POST["user_id"] = "1";
$_POST["session"] = "test";
$_POST["limit"] = "1";
$_POST["offset"] = "0";
$_SERVER["REQUEST_METHOD"] = "POST";
$api_version = "1.5.2";
try {
    chdir("/var/www/html");
    require_once "/var/www/html/assets/init.php";
    require_once "/var/www/html/api/phone/core/functions.php";
    ob_start();
    include "/var/www/html/api/phone/get_news_feed.php";
    $output = ob_get_clean();
    echo substr($output, 0, 1000);
} catch (Throwable $e) {
    echo "Error: " . $e->getMessage() . "\n" . $e->getTraceAsString();
}
