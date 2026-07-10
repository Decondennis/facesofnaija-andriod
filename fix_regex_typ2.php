<?php
$files = [
    "/var/www/html/xhr/pages.php",
    "/var/www/html/api/v2/endpoints/create-page.php",
    "/var/www/html/api/phone/create_page.php",
];

foreach ($files as $f) {
    if (!file_exists($f)) continue;
    $c = file_get_contents($f);
    // Fix: '/[^\w]/_' -> '/[^\w]/'
    $c = str_replace("/[^\\w]/_", "/[^\\w]/", $c);
    file_put_contents($f, $c);
    exec("php -l $f 2>&1", $out);
    echo "$f: " . $out[0] . "\n";
}

// Also fix api/v2/endpoints/create-group.php (same issue)
$gf = "/var/www/html/api/v2/endpoints/create-group.php";
if (file_exists($gf)) {
    $gc = file_get_contents($gf);
    $gc = str_replace("/[^\\w]/_", "/[^\\w]/", $gc);
    file_put_contents($gf, $gc);
    exec("php -l $gf 2>&1", $out);
    echo "$gf: " . $out[0] . "\n";
}
