<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);

// The broken section
$old = "case 'create_story':\n        case \n            include \"api/\" . \$application . \"/create_story.php\";\n            break;\n\n        case 'get_news_feed':";

// What it should be
$new = "case 'create_story':\n            include \"api/\$application/create_story.php\";\n            break;\n        case 'get_news_feed':";

if (strpos($content, $old) !== false) {
    $content = str_replace($old, $new, $content);
    file_put_contents($file, $content);
    echo "FIXED";
} else {
    echo "Pattern not found - checking what exists...\n";
    $start = strpos($content, "case 'create_story':");
    if ($start !== false) {
        echo substr($content, $start, 200);
    } else {
        echo "create_story case not found!";
    }
}
