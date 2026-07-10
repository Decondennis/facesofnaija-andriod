<?php
$f = "/var/www/html/api/phone/new_post.php";
$c = file_get_contents($f);

// Find the PARENT_ID_FIX_APPLIED marker
$marker = "// PARENT_ID_FIX_APPLIED";
$pos = strpos($c, $marker);
if ($pos !== false) {
    // Find the first line before the marker
    $startPos = strrpos(substr($c, 0, $pos), "'postTraveling'");
    if ($startPos !== false) {
        $endOfLine = strpos($c, "\n", $startPos);
        $before = substr($c, 0, $endOfLine + 1);
        $after = substr($c, $endOfLine + 1);
        
        // Remove all the injected code - find the end of the last injected block
        // Look for the pattern where normal code resumes after the injected blocks
        $patternEnd = strpos($after, "if (!empty(\$_POST['post_color'])");
        if ($patternEnd !== false) {
            $after = substr($after, $patternEnd);
        } else {
            // Fallback: just remove up to the next comment or section start
            $fallbackEnd = strpos($after, "if (");
            if ($fallbackEnd !== false) {
                $after = substr($after, $fallbackEnd);
            }
        }
        
        $c = $before . $after;
        file_put_contents($f, $c);
        echo "Fixed: Removed injected parent_id code\n";
    } else {
        echo "Could not find postTraveling marker\n";
    }
} else {
    echo "No PARENT_ID_FIX_APPLIED marker found\n";
}
