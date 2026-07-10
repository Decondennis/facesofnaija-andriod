<?php
$f = "/var/www/html/api/phone/new_post.php";
$c = file_get_contents($f);
// Fix: add missing closing parenthesis after postTraveling line
$search = "'postTraveling' => Wo_Secure(\$traveling), // PARENT_ID_FIX_APPLIED\nif (!empty(\$_POST['post_color'])";
$replace = "'postTraveling' => Wo_Secure(\$traveling),\n                );\n                if (!empty(\$_POST['post_color'])";
$c = str_replace($search, $replace, $c);
file_put_contents($f, $c);
echo "Fixed\n";
