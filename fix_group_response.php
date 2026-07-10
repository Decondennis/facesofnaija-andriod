<?php
$file = "/var/www/html/xhr/groups.php";
$content = file_get_contents($file);

// Add fallback data response when group creation fails
$search = '            $register_group = Wo_RegisterGroup($re_group_data);
            if ($register_group) {';
$replace = '            $register_group = Wo_RegisterGroup($re_group_data);
            if ($register_group) {';

// After the if($register_group) block, add else to set error data
$search2 = '            $register_group = Wo_RegisterGroup($re_group_data);
            if ($register_group) {';
$endBlock = '            }';
// Find the closing of if($register_group)
$pos = strpos($content, $search2);
if ($pos !== false) {
    $blockStart = $pos;
    // Find the matching closing brace
    $level = 0;
    $inBlock = false;
    $blockEnd = $blockStart;
    for ($i = $blockStart; $i < strlen($content); $i++) {
        if ($content[$i] == '{') {
            $level++;
            $inBlock = true;
        } elseif ($content[$i] == '}') {
            $level--;
            if ($inBlock && $level == 0) {
                $blockEnd = $i + 1;
                break;
            }
        }
    }
    
    $before = substr($content, 0, $blockEnd);
    $after = substr($content, $blockEnd);
    
    $elseBlock = "\n        } else {\n";
    $elseBlock .= "            \$data = array(\n";
    $elseBlock .= "                'status' => 400,\n";
    $elseBlock .= "                'errors' => ['Group registration failed. Please try again.']\n";
    $elseBlock .= "            );\n";
    $elseBlock .= "        }\n";
    
    $content = $before . $elseBlock . $after;
    file_put_contents($file, $content);
    echo "Fixed group registration response handling\n";
} else {
    echo "Could not find registration block\n";
}
