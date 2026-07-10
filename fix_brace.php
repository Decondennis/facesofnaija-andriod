<?php
$file = "/var/www/html/xhr/groups.php";
$content = file_get_contents($file);
$lines = explode("\n", $content);

// Find the extra closing brace by checking brace balance
$balance = 0;
$extraCloseLines = [];
foreach ($lines as $i => $line) {
    $lineNum = $i + 1;
    $trimmed = trim($line);
    
    // Count braces in this line
    $openInLine = substr_count($line, '{');
    $closeInLine = substr_count($line, '}');
    
    $prevBalance = $balance;
    $balance += $openInLine - $closeInLine;
    
    // If balance goes below 0, we have an extra closing brace
    if ($balance < 0) {
        $extraCloseLines[] = $lineNum;
        // Adjust: pretend this line doesn't have the extra closing
        $balance = 0;
    }
}

echo "Extra closing braces at lines: " . implode(", ", $extraCloseLines) . "\n";

if (!empty($extraCloseLines)) {
    // Remove the first extra closing brace
    $targetLine = $extraCloseLines[0] - 1; // 0-indexed
    $lines[$targetLine] = preg_replace('/}\s*$/', '', $lines[$targetLine]);
    $lines[$targetLine] = rtrim($lines[$targetLine]);
    $newContent = implode("\n", $lines);
    file_put_contents($file, $newContent);
    echo "Removed extra brace at line $targetLine\n";
    
    // Re-check
    exec("php -l $file 2>&1", $out, $ret);
    echo implode("\n", $out) . "\n";
}
