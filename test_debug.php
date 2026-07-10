<?php
error_reporting(E_ALL);
ini_set('display_errors', 1);
$_GET['type'] = 'get_news_feed';
$_GET['application'] = 'phone';
$_POST['user_id'] = '1';
$_POST['s'] = 'test';
$_POST['limit'] = '1';
$_POST['offset'] = '0';
$_SERVER['REQUEST_METHOD'] = 'POST';
ob_start();
require_once('/var/www/html/assets/init.php');
require_once('/var/www/html/api/phone/core/functions.php');
$api_version = '1.5.2';
include '/var/www/html/api/phone/get_news_feed.php';
$out = ob_get_clean();
echo substr($out, 0, 3000);
