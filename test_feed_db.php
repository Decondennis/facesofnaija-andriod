<?php
chdir('/var/www/html');
require_once('assets/init.php');
require_once('api/phone/core/functions.php');

$uid = 1;
$lim = 1;
$query = "SELECT p.`id` FROM " . T_POSTS . " p WHERE (
    p.`user_id` = {$uid} OR
    p.`user_id` IN (
        SELECT `follower_id` FROM " . T_FOLLOWERS . " WHERE `user_id` = {$uid} AND `active` = 1
    ) OR
    p.`page_id` IN (
        SELECT `page_id` FROM " . T_PAGES . " WHERE `user_id` = {$uid}
    ) OR
    p.`group_id` IN (
        SELECT `id` FROM " . T_GROUPS . " WHERE `user_id` = {$uid}
    )
) AND p.`active` = '1' ORDER BY p.`id` DESC LIMIT {$lim}";

echo "Query: $query\n\n";
$result = mysqli_query($sqlConnect, $query);
if ($result) {
    echo "Query OK, rows: " . mysqli_num_rows($result) . "\n";
    if (mysqli_num_rows($result) > 0) {
        $row = mysqli_fetch_assoc($result);
        echo "First post ID: " . $row['id'] . "\n";
    }
} else {
    echo "Error: " . mysqli_error($sqlConnect) . "\n";
}
