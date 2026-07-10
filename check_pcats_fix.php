<?php
$json = file_get_contents("/tmp/site_settings2.json");
$d = json_decode($json, true);
$pc = $d["config"]["page_categories"] ?? "MISSING";
echo "page_categories type: " . gettype($pc) . "\n";
if (is_array($pc)) {
    echo "count: " . count($pc) . "\n";
    foreach (array_slice($pc, 0, 5) as $k => $v) echo "  $k => $v\n";
}
