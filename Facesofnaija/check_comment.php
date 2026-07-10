<?php
$ch = curl_init('http://172.236.19.52/app_api.php?application=phone&type=get_post_comments');
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, 'user_id=330&s=59b38fa2f870c557d5aac20f3e9e51e74951732a3e8c1cd99f0423a7a4f871ed789187e070989611abebb7c39f4b5e46bbcfab2b565ef32b&post_id=1519');
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
$r = curl_exec($ch);
$d = json_decode($r, true);
if (!$d || empty($d['data'])) { echo "no data\n"; exit; }
$c = $d['data'][0];
echo "reaction: " . json_encode($c['reaction'] ?? 'MISSING', JSON_PRETTY_PRINT) . "\n";
