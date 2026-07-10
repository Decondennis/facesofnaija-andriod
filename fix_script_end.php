<?php
$f = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$c = file_get_contents($f);
// Remove the last 3 lines (comment + broken tag) and add proper closing
$lines = explode("\n", $c);
$last = count($lines);
// Remove last 3 lines if they contain the broken tag
for ($i = $last - 1; $i >= max(0, $last - 5); $i--) {
    $trimmed = trim($lines[$i]);
    if ($trimmed === '// Client-side group name validation' || $trimmed === '/script>' || empty($trimmed)) {
        unset($lines[$i]);
    }
}
$c = implode("\n", $lines) . "\n</script>\n";
file_put_contents($f, $c);
echo "Fixed\n";
