<?php
$file = "/var/www/html/xhr/groups.php";
$content = file_get_contents($file);

// Find and remove the extra debug code and braces that broke the file
// The debug code added lines like: $debug_file = __DIR__ . "/group_create_debug.log";
// Remove all debug_file lines
$lines = explode("\n", $content);
$clean = [];
$skip = false;
foreach ($lines as $line) {
    if (strpos($line, '$debug_file') !== false || strpos($line, 'file_put_contents($debug_file') !== false) {
        continue;
    }
    $clean[] = $line;
}

$content = implode("\n", $clean);
file_put_contents($file, $content);
echo "Cleaned debug lines\n";

// Check syntax
$output = shell_exec("php -l $file 2>&1");
echo $output . "\n";

// If still broken, do additional fix
if (strpos($output, 'Parse error') !== false) {
    echo "Still broken - attempting additional fix\n";
    // Read the file again and try to find mismatched braces
    $content = file_get_contents($file);
    // Count braces
    $open = substr_count($content, '{');
    $close = substr_count($content, '}');
    echo "Open braces: $open, Close braces: $close, Diff: " . ($open - $close) . "\n";
}
