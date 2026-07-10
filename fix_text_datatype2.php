<?php
$files = [
    "/var/www/html/themes/facesofnaija/layout/page/create-page.phtml",
    "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml",
];

foreach ($files as $f) {
    $c = file_get_contents($f);
    
    // Change dataType from 'json' to 'text' and manually parse
    $c = str_replace(
        "dataType: 'json',",
        "dataType: 'text',",
        $c
    );
    
    // In success handler, replace the function parameter to parse text response
    $c = str_replace(
        "success: function(data) {",
        "success: function(text) { try { var data = JSON.parse(text); } catch(e) { var data = { errors: ['JSON error: ' + text.substring(0,200)] }; }",
        $c
    );
    
    file_put_contents($f, $c);
    echo "Fixed " . basename($f) . "\n";
}
