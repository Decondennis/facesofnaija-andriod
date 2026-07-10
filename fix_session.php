<?php
$f = "/var/www/html/api/phone/get_news_feed.php";
$c = file_get_contents($f);
// Fix: handle both 'session' and 's' parameters
$c = str_replace(
    '} else if (empty($_POST["s"])) {',
    '} else if (empty($_POST["session"]) && empty($_POST["s"])) {',
    $c
);
$c = str_replace(
    '$s      = Wo_Secure($_POST["s"]);',
    '$s      = Wo_Secure($_POST["session"] ?? $_POST["s"]);',
    $c
);
file_put_contents($f, $c);
echo "Fixed\n";
