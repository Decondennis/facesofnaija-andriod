<?php
$file = "/var/www/html/assets/includes/functions_two.php";
$content = file_get_contents($file);

// Fix Wo_RegisterPage
$old = '    if (!empty($registration_data["page_category"])) {
        if (!in_array($registration_data["page_category"], array_keys($wo["page_categories"]))) {
            $registration_data["page_category"] = 1;
        }
    }';

$new = '    if (!empty($registration_data["page_category"])) {
        $fonPageCats = $wo["page_categories"] ?? array();
        if (!is_array($fonPageCats)) {
            $decoded = json_decode($fonPageCats, true);
            $fonPageCats = is_array($decoded) ? $decoded : array(1 => "General");
        }
        if (!in_array($registration_data["page_category"], array_keys($fonPageCats))) {
            $registration_data["page_category"] = 1;
        }
    }';

$pos = strpos($content, $old);
if ($pos !== false) {
    $content = substr_replace($content, $new, $pos, strlen($old));
    echo "Fixed Wo_RegisterPage\n";
} else {
    echo "Could not find Wo_RegisterPage pattern\n";
}

// Fix Wo_RegisterGroup
$old2 = '    if (!empty($registration_data["category"])) {
        if (!in_array($registration_data["category"], array_keys($wo["group_categories"]))) {
            $registration_data["category"] = 1;
        }
    }';

$new2 = '    if (!empty($registration_data["category"])) {
        $fonGroupCats = $wo["group_categories"] ?? array();
        if (!is_array($fonGroupCats)) {
            $decoded = json_decode($fonGroupCats, true);
            $fonGroupCats = is_array($decoded) ? $decoded : array(1 => "General");
        }
        if (!in_array($registration_data["category"], array_keys($fonGroupCats))) {
            $registration_data["category"] = 1;
        }
    }';

$pos2 = strpos($content, $old2);
if ($pos2 !== false) {
    $content = substr_replace($content, $new2, $pos2, strlen($old2));
    echo "Fixed Wo_RegisterGroup\n";
} else {
    echo "Could not find Wo_RegisterGroup pattern\n";
}

file_put_contents($file, $content);
exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
