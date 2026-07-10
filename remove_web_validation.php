<?php
$file = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$content = file_get_contents($file);

// Remove the client-side validation we added earlier
$start = strpos($content, "\n   \$('form.create-group-form').on('submit'");
$end = strpos($content, "});\n</script>", $start);
if ($start !== false && $end !== false) {
    $before = substr($content, 0, $start);
    $after = substr($content, $end + 5); // skip "});\n"
    $content = $before . "\n" . $after;
    file_put_contents($file, $content);
    echo "Removed client-side validation\n";
} else {
    echo "Pattern not found, checking alternative...\n";
    // Try alternative pattern with double quotes
    $start = strpos($content, "\n   \$(form.create-group-form).on('submit'");
    if ($start !== false) {
        echo "Found alternative at $start\n";
    } else {
        echo "Not found at all\n";
        // Show what we have around the script tag
        $scriptPos = strrpos($content, "</script>");
        if ($scriptPos !== false) {
            echo "Last 500 chars before </script>:\n";
            echo substr($content, $scriptPos - 500, 500) . "\n";
        }
    }
}
