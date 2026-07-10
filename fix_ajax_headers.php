<?php
// Fix both create-page and create-group to add X-Requested-With header and better error display
$files = [
    "/var/www/html/themes/facesofnaija/layout/page/create-page.phtml",
    "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml",
];

foreach ($files as $f) {
    $c = file_get_contents($f);
    
    // Add headers to $.ajax call
    $old = "type: 'POST',
         data: formData,
         dataType: 'json',";
    $new = "type: 'POST',
         headers: { 'X-Requested-With': 'XMLHttpRequest' },
         data: formData,
         dataType: 'json',";
    $c = str_replace($old, $new, $c);
    
    // Replace the error handler to show full response
    $old2 = "if (xhr.responseText && xhr.responseText.length < 200) msg += '<br>' + xhr.responseText;";
    $new2 = "msg += '<hr><pre>' + xhr.responseText.substring(0,500) + '</pre>'; console.log('Response:', xhr.responseText);";
    $c = str_replace($old2, $new2, $c);
    
    file_put_contents($f, $c);
    echo "Fixed " . basename($f) . "\n";
}
