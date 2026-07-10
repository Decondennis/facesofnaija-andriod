<?php
error_reporting(E_ALL);
ini_set('display_errors', 1);
require '/var/www/html/assets/init.php';
echo "init OK\n";
$ch = curl_init('http://172.236.19.52/app_api.php?application=phone&type=get_news_feed');
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, 'user_id=330&s=59b38fa2f870c557d5aac20f3e9e51e74951732a3e8c1cd99f0423a7a4f871ed789187e070989611abebb7c39f4b5e46bbcfab2b565ef32b&limit=1');
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
$r = curl_exec($ch);
echo "curl done len=" . strlen($r) . "\n";
$d = json_decode($r, true);
if (!$d) { echo "JSON decode FAILED\n"; exit; }
echo "JSON OK\n";
$p = $d['posts'][0];
echo 'url: ' . var_export(isset($p['url']) ? $p['url'] : 'MISSING', true) . "\n";
echo 'post_url: ' . var_export(isset($p['post_url']) ? $p['post_url'] : 'MISSING', true) . "\n";
