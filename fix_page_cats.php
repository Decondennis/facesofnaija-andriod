<?php
// Step 1: Add page categories to wo_config if they don't exist
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) { die("DB error: " . $db->connect_error . "\n"); }

// Check if page_categories exists in config
$r = $db->query("SELECT name FROM wo_config WHERE name = 'page_categories'");
if ($r && $r->num_rows == 0) {
    $cats = array();
    $cats[1] = "Business";
    $cats[2] = "Company";
    $cats[3] = "Artist";
    $cats[4] = "Brand";
    $cats[5] = "Entertainment";
    $cats[6] = "Food & Drink";
    $cats[7] = "Health";
    $cats[8] = "Hotel";
    $cats[9] = "News";
    $cats[10] = "Non-Profit";
    $cats[11] = "Organization";
    $cats[12] = "Public Figure";
    $cats[13] = "Real Estate";
    $cats[14] = "School";
    $cats[15] = "Shopping";
    $cats[16] = "Sports";
    $cats[17] = "Technology";
    $cats[18] = "Website";
    $json = json_encode($cats);
    $db->query("INSERT INTO wo_config (name, value) VALUES ('page_categories', '" . $db->real_escape_string($json) . "')");
    echo "Inserted page_categories\n";
} else {
    echo "page_categories already exists\n";
}

// Check if group_categories exists
$r = $db->query("SELECT name FROM wo_config WHERE name = 'group_categories'");
if ($r && $r->num_rows == 0) {
    $cats = array();
    $cats[1] = "General";
    $cats[2] = "Education";
    $cats[3] = "Entertainment";
    $cats[4] = "Music";
    $cats[5] = "Sports";
    $cats[6] = "Technology";
    $cats[7] = "Business";
    $cats[8] = "Gaming";
    $cats[9] = "Health";
    $cats[10] = "News";
    $cats[11] = "Travel";
    $cats[12] = "Art";
    $cats[13] = "Photography";
    $cats[14] = "Fashion";
    $cats[15] = "Food";
    $cats[16] = "Science";
    $cats[17] = "Charity";
    $cats[18] = "Other";
    $json = json_encode($cats);
    $db->query("INSERT INTO wo_config (name, value) VALUES ('group_categories', '" . $db->real_escape_string($json) . "')");
    echo "Inserted group_categories\n";
} else {
    echo "group_categories already exists\n";
}

$db->close();

// Step 2: Fix the create-page.phtml template to add fallback categories
$file = "/var/www/html/themes/facesofnaija/layout/page/create-page.phtml";
$content = file_get_contents($file);

// Replace the foreach that uses $wo['page_categories'] with one that checks if it's empty
$old = '<?php foreach ($wo[\'page_categories\'] as $category_id => $category_name) {?>';
$new = '<?php 
$pageCats = $wo[\'page_categories\'] ?? array();
if (empty($pageCats)) {
    $pageCats = array(1=>"Business",2=>"Company",3=>"Artist",4=>"Brand",5=>"Entertainment",6=>"Food & Drink",7=>"Health",8=>"Hotel",9=>"News",10=>"Non-Profit",11=>"Organization",12=>"Public Figure",13=>"Real Estate",14=>"School",15=>"Shopping",16=>"Sports",17=>"Technology",18=>"Website");
}
foreach ($pageCats as $category_id => $category_name) {?>';

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed create-page.phtml categories fallback\n";

// Step 3: Also fix create-group.phtml with the same approach
$file2 = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$content2 = file_get_contents($file2);

$old2 = '<?php foreach ($wo[\'group_categories\'] as $category_id => $category_name) {?>';
$new2 = '<?php 
$groupCats = $wo[\'group_categories\'] ?? array();
if (empty($groupCats)) {
    $groupCats = array(1=>"General",2=>"Education",3=>"Entertainment",4=>"Music",5=>"Sports",6=>"Technology",7=>"Business",8=>"Gaming",9=>"Health",10=>"News",11=>"Travel",12=>"Art",13=>"Photography",14=>"Fashion",15=>"Food",16=>"Science",17=>"Charity",18=>"Other");
}
foreach ($groupCats as $category_id => $category_name) {?>';

$content2 = str_replace($old2, $new2, $content2);
file_put_contents($file2, $content2);
echo "Fixed create-group.phtml categories fallback\n";
