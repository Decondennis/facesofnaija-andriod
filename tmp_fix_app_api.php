<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);

$newCases = '
        case \'get_news_feed\':
            include "api/".$application."/get_news_feed.php";
            break;
        case \'get_announcements\':
            include "api/".$application."/get_announcements.php";
            break;
        case \'get_post_comments\':
            include "api/".$application."/get_post_comments.php";
            break;
        case \'update_story_view\':
            include "api/".$application."/update_story_view.php";
            break;
';

$search = "case 'create_story':";
$pos = strpos($content, $search);
if ($pos !== false) {
    $breakPos = strpos($content, "break;", $pos);
    $insertPos = $breakPos + 7;
    $content = substr($content, 0, $insertPos) . $newCases . substr($content, $insertPos);
    file_put_contents($file, $content);
    echo "SUCCESS: Added missing API cases\n";
} else {
    echo "ERROR: Could not find insertion point\n";
}
