<?php
$file = "/var/www/html/xhr/groups.php";
$content = file_get_contents($file);

// Add diagnostic logging after the response is set
$search = "header(\"Content-type: application/json\");";
$replace = "\$debug_file = __DIR__ . '/group_create_debug.log';\n" .
"file_put_contents(\$debug_file, date('c') . \" Response: \" . (isset(\$errors) ? json_encode(array('errors' => \$errors)) : json_encode(\$data)) . \"\\n\", FILE_APPEND);\n" .
"file_put_contents(\$debug_file, date('c') . \" POST: \" . print_r(\$_POST, true) . \"\\n\", FILE_APPEND);\n" .
"file_put_contents(\$debug_file, date('c') . \" errors: \" . print_r(\$errors ?? array(), true) . \"\\n\", FILE_APPEND);\n" .
"file_put_contents(\$debug_file, date('c') . \" data: \" . print_r(\$data ?? array(), true) . \"\\n\", FILE_APPEND);\n" .
"\$search";

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Added diagnostics\n";
