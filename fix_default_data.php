<?php
$file = "/var/www/html/xhr/groups.php";
$content = file_get_contents($file);

// Add default $data initialization after line 3 (if ($s == 'create_group') {)
$search = "if (\$s == 'create_group') {\n";
$replace = "if (\$s == 'create_group') {\n        \$data = array('status' => 400, 'errors' => array('Unknown error'));\n";
$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Added default \$data initialization\n";
