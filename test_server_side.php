<?php
$_GET['type'] = 'get_news_feed';
$_POST['user_id'] = '1';
$_POST['s'] = 'test';
$_SERVER['REQUEST_METHOD'] = 'POST';

require '/var/www/html/assets/init.php';
$sid = Wo_Secure($_POST['s']);
$uid = Wo_Secure($_POST['user_id']);

echo "Wo_CheckUserSessionID result: ";
var_dump(Wo_CheckUserSessionID($uid, $sid, 'phone'));

echo "\nUser data: ";
$ud = Wo_UserData($uid);
if ($ud) {
    echo "Found: " . $ud['username'] . "\n";
} else {
    echo "Not found\n";
}
