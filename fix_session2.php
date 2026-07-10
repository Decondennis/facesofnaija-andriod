<?php
$f = "/var/www/html/api/phone/get_news_feed.php";
// Restore from repo first
copy("/tmp/facesofnaija-web/api/phone/get_news_feed.php", $f);
$c = file_get_contents($f);

// Now fix: need to check for both 'session' and 's' POST parameters
// The app sends 'session' parameter, but original code checks 's'
$search1 = '} else if (empty($_POST[\'s\'])) {';
$replace1 = '} else if (empty($_POST[\'session\']) && empty($_POST[\'s\'])) {';
$c = str_replace($search1, $replace1, $c);

$search2 = '$s      = Wo_Secure($_POST[\'s\']);';
$replace2 = '$s      = Wo_Secure(!empty($_POST[\'session\']) ? $_POST[\'session\'] : $_POST[\'s\']);';
$c = str_replace($search2, $replace2, $c);

file_put_contents($f, $c);
echo "Fixed\n";
