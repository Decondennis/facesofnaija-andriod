<?php
// Fix xhr/pages.php - add auto-slugify for page_name
$file = "/var/www/html/xhr/pages.php";
$content = file_get_contents($file);

// The EXACT text in the file (12 spaces indent)
$old = '            if (!preg_match(\'/^[\\w]+$/\', $_POST[\'page_name\'])) {
                $errors[] = $error_icon . $wo[\'lang\'][\'page_name_invalid_characters\'];
            }';

$new = '            if (!preg_match(\'/^[\\w]+$/\', $_POST[\'page_name\'])) {
                $slug = preg_replace(\'/[^\\w]/_\', \'_\', $_POST[\'page_name\']);
                $slug = preg_replace(\'/[_]+/\', \'_\', $slug);
                $slug = trim($slug, \'_\');
                if (strlen($slug) < 5) {
                    $slug = $slug . str_repeat(\'_\', 5 - strlen($slug));
                }
                if (strlen($slug) > 32) {
                    $slug = substr($slug, 0, 32);
                }
                $_POST[\'page_name\'] = $slug;
            }';

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed xhr/pages.php\n";
exec("php -l $file 2>&1", $out);
echo implode("\n", $out) . "\n";

// Also check and fix api/v2/endpoints/create-page.php
$file2 = "/var/www/html/api/v2/endpoints/create-page.php";
if (file_exists($file2)) {
    $content2 = file_get_contents($file2);
    $old2 = "    } else if (!preg_match('/^[\\\\w]+\$/', \$_POST['page_name'])) {
        \$error_code    = 6;
        \$error_message = 'Invalid Page name characters';
    }";
    $new2 = "    } else {
        \$slug = preg_replace('/[^\\\\w]/', '_', \$_POST['page_name']);
        \$slug = preg_replace('/_+/', '_', \$slug);
        \$slug = trim(\$slug, '_');
        if (strlen(\$slug) < 5) {
            \$slug = \$slug . str_repeat('_', 5 - strlen(\$slug));
        }
        if (strlen(\$slug) > 32) {
            \$slug = substr(\$slug, 0, 32);
        }
        \$_POST['page_name'] = \$slug;
    }";
    $content2 = str_replace($old2, $new2, $content2);
    file_put_contents($file2, $content2);
    echo "Fixed api/v2/endpoints/create-page.php\n";
    exec("php -l $file2 2>&1", $out2);
    echo implode("\n", $out2) . "\n";
}
