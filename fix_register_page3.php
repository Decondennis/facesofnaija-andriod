<?php
$file = "/var/www/html/assets/includes/functions_two.php";
$content = file_get_contents($file);

// Normalize line endings to LF
$content = str_replace("\r\n", "\n", $content);

// Fix Wo_RegisterPage
$old = '    if (!empty($registration_data["page_category"])) {
        if (!in_array($registration_data["page_category"], array_keys($wo["page_categories"]))) {
            $registration_data["page_category"] = 1;
        }
    }';

$new = '    if (!empty($registration_data["page_category"])) {
        $fonPageCats = $wo["page_categories"] ?? array();
        if (!is_array($fonPageCats)) { $decoded = json_decode($fonPageCats, true); $fonPageCats = is_array($decoded) ? $decoded : array(1 => "General"); }
        if (!in_array($registration_data["page_category"], array_keys($fonPageCats))) {
            $registration_data["page_category"] = 1;
        }
    }';

$content = str_replace($old, $new, $content);

// Fix Wo_RegisterGroup
$old2 = '    if (!empty($registration_data["category"])) {
        if (!in_array($registration_data["category"], array_keys($wo["group_categories"]))) {
            $registration_data["category"] = 1;
        }
    }';

$new2 = '    if (!empty($registration_data["category"])) {
        $fonGroupCats = $wo["group_categories"] ?? array();
        if (!is_array($fonGroupCats)) { $decoded = json_decode($fonGroupCats, true); $fonGroupCats = is_array($decoded) ? $decoded : array(1 => "General"); }
        if (!in_array($registration_data["category"], array_keys($fonGroupCats))) {
            $registration_data["category"] = 1;
        }
    }';

$content = str_replace($old2, $new2, $content);

// Convert back to CRLF
$content = str_replace("\n", "\r\n", $content);

file_put_contents($file, $content);
echo "Fixed\n";

exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
