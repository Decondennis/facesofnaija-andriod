<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
$r = $db->query("SELECT value FROM wo_config WHERE name = 'page_categories'");
echo "page_categories: ";
if ($r && $row = $r->fetch_assoc()) {
    echo "found, value starts with: " . substr($row["value"], 0, 50) . "\n";
    $decoded = json_decode($row["value"], true);
    echo "is_array: " . (is_array($decoded) ? "yes" : "no") . "\n";
    if (is_array($decoded)) echo "count: " . count($decoded) . "\n";
} else {
    echo "NOT FOUND\n";
}
$r2 = $db->query("SELECT value FROM wo_config WHERE name = 'group_categories'");
echo "group_categories: ";
if ($r2 && $row2 = $r2->fetch_assoc()) {
    echo "found, value starts with: " . substr($row2["value"], 0, 50) . "\n";
    $decoded2 = json_decode($row2["value"], true);
    echo "is_array: " . (is_array($decoded2) ? "yes" : "no") . "\n";
    if (is_array($decoded2)) echo "count: " . count($decoded2) . "\n";
} else {
    echo "NOT FOUND\n";
}
// Check how WoWonder loads page categories
$r3 = $db->query("SHOW CREATE TABLE wo_config");
if ($r3 && $row3 = $r3->fetch_assoc()) {
    echo "wo_config table exists\n";
}
$db->close();
