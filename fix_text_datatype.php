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
    
    // In success handler, parse the text response as JSON
    $old = "success: function(data) {";
    $new = "success: function(text) { var data = null; try { data = JSON.parse(text); } catch(e) { data = { errors: ['Invalid JSON response: ' + text.substring(0,200)] }; }";
    $c = str_replace($old, $new, $c);
    
    // Update error handler to show response
    $old2 = "(xhr.responseText || '').substring(0,500)";
    $new2 = "(xhr.responseText || 'empty').substring(0,1000)";
    $c = str_replace($old2, $new2, $c);
    
    file_put_contents($f, $c);
    echo "Fixed " . basename($f) . "\n";
}
