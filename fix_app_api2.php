<?php
$file = '/var/www/html/app_api.php';
$content = file_get_contents($file);
$lines = explode("\n", $content);
$newLines = array();
$fixed = false;
foreach ($lines as $line) {
    if (preg_match('/if\s*\(empty/', $line) && preg_match('/session/', $line) && !preg_match('/\$_POST/', $line)) {
        $newLines[] = 'if (empty($_POST["s"]) && !empty($_POST["session"])) { $_POST["s"] = $_POST["session"]; }';
        $fixed = true;
    } else {
        $newLines[] = $line;
    }
}
if (!$fixed) {
    // Find the line after $type assignment and insert there
    $temp = array();
    $inserted = false;
    foreach ($lines as $line) {
        $temp[] = $line;
        if (preg_match('/type = Wo_Secure/', $line) && !$inserted) {
            $temp[] = 'if (empty($_POST["s"]) && !empty($_POST["session"])) { $_POST["s"] = $_POST["session"]; }';
            $inserted = true;
        }
    }
    $newLines = $temp;
}
file_put_contents($file, implode("\n", $newLines));
echo "Fixed: " . ($fixed ? "replaced corrupted line" : "inserted new line") . "\n";
