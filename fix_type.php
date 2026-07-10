<?php
$f = "/var/www/html/api/phone/get_news_feed.php";
$c = file_get_contents($f);
$c = str_replace(
    "if (\$type == 'get_news_list') {",
    "if (\$type == 'get_news_list' || \$type == 'get_news_feed') {",
    $c
);
file_put_contents($f, $c);
echo "OK\n";
