<?php
echo "=== Applying server fixes ===\n\n";

// 1. Fix Wo_SlugPost in functions_one.php
$f1 = '/var/www/html/assets/includes/functions_one.php';
if (file_exists($f1)) {
    $c = file_get_contents($f1);
    if (strpos($c, 'if (empty(trim($string)))') !== false) {
        echo "[SKIP] Wo_SlugPost already patched\n";
    } else {
        $old = 'function Wo_SlugPost($string) {';
        $new = 'function Wo_SlugPost($string) { if (empty(trim($string))) { $string = "post"; }';
        if (strpos($c, $old) !== false) {
            $c = str_replace($old, $new, $c);
            file_put_contents($f1, $c);
            echo "[OK] Wo_SlugPost updated\n";
        } else {
            echo "[SKIP] Wo_SlugPost function not found\n";
        }
    }
} else {
    echo "[SKIP] functions_one.php not found\n";
}

// 2. Fix new_post.php - add parent_id handling
$f2 = '/var/www/html/api/phone/new_post.php';
if (file_exists($f2)) {
    $c = file_get_contents($f2);
    if (strpos($c, "// PARENT_ID_FIX_APPLIED") !== false) {
        echo "[SKIP] new_post.php already patched\n";
    } else {
        $search = "'postTraveling' => Wo_Secure(\$traveling),";
        $replace = "'postTraveling' => Wo_Secure(\$traveling), // PARENT_ID_FIX_APPLIED\n                if (!empty(\$_POST['parent_id'])) {\n                    \$pid = (int) \$_POST['parent_id'];\n                    \$visited = array(\$pid);\n                    while (\$pid > 0) {\n                        \$parent_check = Wo_PostData(\$pid);\n                        if (!empty(\$parent_check['parent_id']) && \$parent_check['parent_id'] > 0 && !in_array(\$parent_check['parent_id'], \$visited)) {\n                            \$pid = (int) \$parent_check['parent_id'];\n                            \$visited[] = \$pid;\n                        } else {\n                            break;\n                        }\n                    }\n                    \$post_data['parent_id'] = \$pid;\n                }";
        if (strpos($c, $search) !== false) {
            $c = str_replace($search, $replace, $c);
            file_put_contents($f2, $c);
            echo "[OK] new_post.php updated with parent_id handling\n";
        } else {
            echo "[SKIP] new_post.php pattern not found\n";
        }
    }
} else {
    echo "[SKIP] new_post.php not found\n";
}

// 3. Fix get_news_feed.php - include page/group posts
$f3 = '/var/www/html/api/phone/get_news_feed.php';
if (file_exists($f3)) {
    $c = file_get_contents($f3);
    if (strpos($c, 'p.page_id IN') !== false) {
        echo "[SKIP] get_news_feed.php already patched\n";
    } else {
        // Try multiple possible patterns
        $patterns = array(
            // Pattern with constants
            array("FROM T_FOLLOWERS WHERE", "FROM T_FOLLOWERS WHERE"),
            array("FROM Wo_Followers WHERE", "FROM Wo_Followers WHERE"),
        );
        
        $found = false;
        foreach ($patterns as $p) {
            $base = $p[0];
            // Build the full old query section
            $oldQ = ") AND p.`active` = '1'";
            $newQ = ") OR\n        p.`page_id` IN (\n            SELECT `page_id` FROM Wo_Pages WHERE `user_id` = {$uid}\n        ) OR\n        p.`group_id` IN (\n            SELECT `id` FROM Wo_Groups WHERE `user_id` = {$uid}\n        )\n    ) AND p.`active` = '1'";
            
            if (strpos($c, $base) !== false) {
                // Find the end of the WHERE clause
                $pos = strpos($c, ") AND p.`active` = '1'");
                if ($pos !== false) {
                    // Find the beginning - look backwards for "WHERE ("
                    $start = strrpos(substr($c, 0, $pos), "WHERE (");
                    if ($start !== false) {
                        $len = $pos + strlen(") AND p.`active` = '1'") - $start;
                        $oldSection = substr($c, $start, $len);
                        $newSection = "WHERE (\n        p.`user_id` = {$uid} OR\n        p.`user_id` IN (\n            SELECT `follower_id` FROM " . $base . " `user_id` = {$uid} AND `active` = 1\n        ) OR\n        p.`page_id` IN (\n            SELECT `page_id` FROM Wo_Pages WHERE `user_id` = {$uid}\n        ) OR\n        p.`group_id` IN (\n            SELECT `id` FROM Wo_Groups WHERE `user_id` = {$uid}\n        )\n    ) AND p.`active` = '1'";
                        
                        $c = str_replace($oldSection, $newSection, $c);
                        file_put_contents($f3, $c);
                        echo "[OK] get_news_feed.php updated\n";
                        $found = true;
                        break;
                    }
                }
            }
        }
        if (!$found) {
            echo "[SKIP] get_news_feed.php pattern not found\n";
        }
    }
} else {
    echo "[SKIP] get_news_feed.php not found\n";
}

echo "\n=== Fixes complete ===\n";
