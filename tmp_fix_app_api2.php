<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);

// Fix the broken create_story block
$broken = "case 'create_story':\n        case \n            include \"api/\" . \\\$application . \"/create_story.php\";\n            break;\n\n        case 'get_news_feed':\n            include \"api/\" . \\\$application . \"/get_news_feed.php\";\n            break;\n        case 'get_announcements':\n            include \"api/\" . \\\$application . \"/get_announcements.php\";\n            break;\n        case 'get_post_comments':\n            include \"api/\" . \\\$application . \"/get_post_comments.php\";\n            break;\n        case 'update_story_view':\n            include \"api/\" . \\\$application . \"/update_story_view.php\";\n            break;";

$fixed = "case 'create_story':\n            include \"api/\\\$application/create_story.php\";\n            break;\n        case 'get_news_feed':\n            include \"api/\\\$application/get_news_feed.php\";\n            break;\n        case 'get_announcements':\n            include \"api/\\\$application/get_announcements.php\";\n            break;\n        case 'get_post_comments':\n            include \"api/\\\$application/get_post_comments.php\";\n            break;\n        case 'update_story_view':\n            include \"api/\\\$application/update_story_view.php\";\n            break;";

$content = str_replace($broken, $fixed, $content);
file_put_contents($file, $content);
echo "SUCCESS: Fixed broken cases\n";
