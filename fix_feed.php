<?php
$f = "/var/www/html/api/phone/get_news_feed.php";
$c = file_get_contents($f);

// Insert page/group condition before the closing parenthesis of the WHERE clause
$pos = strpos($c, ") AND p.`active` = '1'");
if ($pos !== false) {
    $insert = " OR\n        p.`page_id` IN (\n            SELECT `page_id` FROM Wo_Pages WHERE `user_id` = \$uid\n        ) OR\n        p.`group_id` IN (\n            SELECT `id` FROM Wo_Groups WHERE `user_id` = \$uid\n        )";
    $c = substr_replace($c, $insert, $pos, 0);
    file_put_contents($f, $c);
    echo "OK: get_news_feed.php updated\n";
} else {
    echo "Pattern not found\n";
}
