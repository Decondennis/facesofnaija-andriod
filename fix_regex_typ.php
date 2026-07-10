<?php
$files = [
    "/var/www/html/xhr/pages.php",
    "/var/www/html/api/v2/endpoints/create-page.php",
    "/var/www/html/api/phone/create_page.php",
];

foreach ($files as $f) {
    if (!file_exists($f)) continue;
    $c = file_get_contents($f);
    // Fix the preg_replace pattern '/[^\w]/_' -> '/[^\w]/'
    $old = "preg_replace('/[^\\w]/_', '_', \$_POST['page_name'])";
    $new = "preg_replace('/[^\\w]/', '_', \$_POST['page_name'])";
    $c = str_replace($old, $new, $c);
    file_put_contents($f, $c);
    exec("php -l $f 2>&1", $out);
    echo "$f: " . implode(" ", $out) . "\n";
}
