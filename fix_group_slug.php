<?php
// Fix V2 API create-group.php - auto-slugify group_name
$file = "/var/www/html/api/v2/endpoints/create-group.php";
$content = file_get_contents($file);

// Replace the regex validation with auto-slugify logic
$old = "    } else if (!preg_match('/^[\\\w]+\$/', \$_POST['group_name'])) {
        \$error_code    = 6;
        \$error_message = 'Invalid group name characters';
    }";

$new = "    } else {
        // Auto-slugify: replace spaces with underscores, remove other invalid chars
        \$slug = preg_replace('/[^\\\w]/', '_', \$_POST['group_name']);
        \$slug = preg_replace('/_+/', '_', \$slug);
        \$slug = trim(\$slug, '_');
        if (strlen(\$slug) < 5) {
            \$slug = \$slug . str_repeat('_', 5 - strlen(\$slug));
        }
        if (strlen(\$slug) > 32) {
            \$slug = substr(\$slug, 0, 32);
        }
        \$_POST['group_name'] = \$slug;
    }";

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed V2 API create-group.php\n";

// Fix xhr/groups.php - same logic
$file2 = "/var/www/html/xhr/groups.php";
$content2 = file_get_contents($file2);

$old2 = "            if (!preg_match('/^[\\w]+\$/', \$_POST['group_name'])) {
                \$errors[] = \$error_icon . \$wo['lang']['group_name_invalid_characters'];
            }";

$new2 = "            if (!preg_match('/^[\\w]+\$/', \$_POST['group_name'])) {
                // Auto-slugify: replace spaces with underscores, remove other invalid chars
                \$slug = preg_replace('/[^\\\w]/', '_', \$_POST['group_name']);
                \$slug = preg_replace('/_+/', '_', \$slug);
                \$slug = trim(\$slug, '_');
                if (strlen(\$slug) < 5) {
                    \$slug = \$slug . str_repeat('_', 5 - strlen(\$slug));
                }
                if (strlen(\$slug) > 32) {
                    \$slug = substr(\$slug, 0, 32);
                }
                \$_POST['group_name'] = \$slug;
            }";

$content2 = str_replace($old2, $new2, $content2);
file_put_contents($file2, $content2);
echo "Fixed xhr/groups.php\n";

// Fix phone API create_group.php - same logic
$file3 = "/var/www/html/api/phone/create_group.php";
$content3 = file_get_contents($file3);

$old3 = "                if (!preg_match('/^[\\w]+\$/', \$_POST['group_name'])) {
                    \$errors[] = \$wo['lang']['group_name_invalid_characters'];
                }";

$new3 = "                if (!preg_match('/^[\\w]+\$/', \$_POST['group_name'])) {
                    // Auto-slugify
                    \$slug = preg_replace('/[^\\\w]/', '_', \$_POST['group_name']);
                    \$slug = preg_replace('/_+/', '_', \$slug);
                    \$slug = trim(\$slug, '_');
                    if (strlen(\$slug) < 5) {
                        \$slug = \$slug . str_repeat('_', 5 - strlen(\$slug));
                    }
                    if (strlen(\$slug) > 32) {
                        \$slug = substr(\$slug, 0, 32);
                    }
                    \$_POST['group_name'] = \$slug;
                }";

$content3 = str_replace($old3, $new3, $content3);
file_put_contents($file3, $content3);
echo "Fixed phone API create_group.php\n";
