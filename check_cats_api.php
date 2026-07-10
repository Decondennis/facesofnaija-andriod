<?php
$d = json_decode(file_get_contents("/tmp/site_settings.json"), true);
$gc = $d["config"]["group_categories"] ?? "";
$pc = $d["config"]["page_categories"] ?? "";
echo "group_categories type: " . gettype($gc) . "\n";
if (is_array($gc)) echo "  count: " . count($gc) . "\n  keys: " . implode(",", array_keys($gc)) . "\n";
echo "page_categories type: " . gettype($pc) . "\n";
if (is_array($pc)) echo "  count: " . count($pc) . "\n  keys: " . implode(",", array_keys($pc)) . "\n";
