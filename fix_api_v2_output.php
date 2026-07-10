<?php
$file = "/var/www/html/api-v2.php";
// Read current content and add the echo at the end
$content = file_get_contents($file);
$content = rtrim($content) . "\necho json_encode(\$response_data, JSON_PRETTY_PRINT);\n";
file_put_contents($file, $content);
echo "Added output to api-v2.php\n";
