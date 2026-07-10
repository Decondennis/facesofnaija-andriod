<?php
$file = "/var/www/html/xhr/groups.php";
$content = file_get_contents($file);

$search = 'header("Content-type: application/json");';

$diag_code = '
    $debug_file = __DIR__ . "/group_create_debug.log";
    file_put_contents($debug_file, date("c") . " errors: " . print_r(isset($errors) ? $errors : array(), true) . "\n", FILE_APPEND);
    file_put_contents($debug_file, date("c") . " data: " . print_r(isset($data) ? $data : array(), true) . "\n", FILE_APPEND);
    file_put_contents($debug_file, date("c") . " POST: " . print_r($_POST, true) . "\n", FILE_APPEND);
';

$replace = $diag_code . "\n" . $search;

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Added diagnostics\n";
