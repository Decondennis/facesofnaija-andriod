<?php
$file = "/var/www/html/api/phone/login.php";
$content = file_get_contents($file);
$search = "'cookie' => \$session";
$replace = "'cookie' => \$session,\n                        'access_token' => \$session";
$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Done";
