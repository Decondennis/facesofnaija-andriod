<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);

// Fix the broken create_story block
// Replace "case 'create_story':\n        case \n            include"
// with   "case 'create_story':\n            include"
$content = str_replace(
    "case 'create_story':\n        case \n            include",
    "case 'create_story':\n            include",
    $content
);

file_put_contents($file, $content);
echo "DONE";
