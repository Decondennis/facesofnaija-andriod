<?php
$file = "/var/www/html/api/v2/endpoints/new_post.php";
$content = file_get_contents($file);

// Just remove the error_reporting line (that's the main cause of JSON breakage)
$content = str_replace('error_reporting(E_ALL); ini_set("display_errors", 1);' . "\n", '', $content);

file_put_contents($file, $content);
echo "Fixed\n";
exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
