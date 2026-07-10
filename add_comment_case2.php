<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);

$search = "case 'new_post':\n            include \"api/\$application/new_post.php\";\n            break;";
$replace = "case 'new_post':\n            include \"api/\$application/new_post.php\";\n            break;\n        case 'create_comment':\n        case 'create_post_comment':\n        case 'post_comment':\n            include \"api/\$application/create_comment.php\";\n            break;";

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Added create_comment cases\n";
