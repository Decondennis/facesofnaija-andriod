<?php
$file = "/var/www/html/requests.php";
$content = file_get_contents($file);

// Replace plain text exits with JSON responses
$content = str_replace(
    'exit("Restrcited Area");',
    'header("Content-Type: application/json"); echo json_encode(array("errors" => array("Restricted Area"))); exit();',
    $content
);

$content = str_replace(
    'exit("Please login or signup to continue.");',
    'header("Content-Type: application/json"); echo json_encode(array("errors" => array("Please login or signup to continue."))); exit();',
    $content
);

file_put_contents($file, $content);
echo "Fixed requests.php to return JSON\n";
exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
