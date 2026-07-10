<?php
$file = "/var/www/html/assets/includes/functions_two.php";
$content = file_get_contents($file);

// Fix Wo_RegisterPage to handle non-array page_categories
$old = '    if (!empty($registration_data["page_category"])) {
        if (!in_array($registration_data["page_category"], array_keys($wo["page_categories"]))) {
            $registration_data["page_category"] = 1;
        }
    }';

$new = '    if (!empty($registration_data["page_category"])) {
        $pageCats = $wo["page_categories"] ?? array();
        if (!is_array($pageCats)) {
            $decoded = json_decode($pageCats, true);
            $pageCats = is_array($decoded) ? $decoded : array(1 => "General");
        }
        if (!in_array($registration_data["page_category"], array_keys($pageCats))) {
            $registration_data["page_category"] = 1;
        }
    }';

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed Wo_RegisterPage\n";

// Also fix Wo_RegisterGroup which has the same issue
$old2 = '    if (!empty($registration_data["category"])) {
        if (!in_array($registration_data["category"], array_keys($wo["group_categories"]))) {
            $registration_data["category"] = 1;
        }
    }';

$new2 = '    if (!empty($registration_data["category"])) {
        $groupCats = $wo["group_categories"] ?? array();
        if (!is_array($groupCats)) {
            $decoded = json_decode($groupCats, true);
            $groupCats = is_array($decoded) ? $decoded : array(1 => "General");
        }
        if (!in_array($registration_data["category"], array_keys($groupCats))) {
            $registration_data["category"] = 1;
        }
    }';

$content = str_replace($old2, $new2, $content);
file_put_contents($file, $content);
echo "Fixed Wo_RegisterGroup\n";

exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
