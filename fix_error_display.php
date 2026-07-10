<?php
$f = "/var/www/html/themes/facesofnaija/layout/page/create-page.phtml";
$c = file_get_contents($f);

// Replace the error handler to show full response
$old = "if (xhr.responseText && xhr.responseText.length < 200) msg += '<br>' + xhr.responseText;";
$new = "msg += '<hr><pre>' + xhr.responseText.substring(0,1000) + '</pre>'; console.log('Response:', xhr.responseText);";

$c = str_replace($old, $new, $c);
file_put_contents($f, $c);
echo "Fixed error display\n";
