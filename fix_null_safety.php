<?php
$files = [
    "/var/www/html/themes/facesofnaija/layout/page/create-page.phtml",
    "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml",
];

foreach ($files as $f) {
    $c = file_get_contents($f);
    $old = "xhr.responseText.substring(0,500)";
    $new = "(xhr.responseText || '').substring(0,500)";
    $c = str_replace($old, $new, $c);
    file_put_contents($f, $c);
    echo "Fixed " . basename($f) . "\n";
}
