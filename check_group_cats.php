<?php
$j = json_decode(file_get_contents("/tmp/settings.json"), true);
$gc = $j["config"]["group_categories"] ?? "NOT FOUND";
echo "Group cats: " . (is_array($gc) ? implode(", ", array_keys($gc)) : $gc) . "\n";
echo "Count: " . (is_array($gc) ? count($gc) : 0) . "\n";
