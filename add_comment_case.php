<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);

// Find the phone API cases and add comment creation after new_post
$search = "case 'new_post':\n            include \"api/\" . \$application . \"/new_post.php\";\n            break;";
$replace = $search . "\n        case 'create_comment':\n        case 'create_post_comment':\n        case 'post_comment':\n            include \"api/\" . \$application . \"/create_comment.php\";\n            break;";

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Added create_comment cases to app_api.php\n";
