<?php
$j = json_decode(file_get_contents("/tmp/site_settings2.json"), true);
$pc = $j["config"]["page_categories"];
echo "keys: " . json_encode(array_keys($pc)) . "\n";
echo "first entry: " . json_encode([array_key_first($pc) => $pc[array_key_first($pc)]]) . "\n";
