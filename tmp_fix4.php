<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);

// Show the exact bytes around create_story
$start = strpos($content, "case 'create_story':");
if ($start !== false) {
    $section = substr($content, $start, 250);
    echo "EXACT SECTION:\n";
    echo json_encode($section) . "\n\n";
    echo "---\n";
    echo $section . "\n";
}
