<?php
$file = '/var/www/html/assets/includes/functions_one.php';
$content = file_get_contents($file);
$old = "function Wo_SlugPost(\$string) {\r\n    \$slug = url_slug(\$string, array(\r\n        'delimiter' => '-',\r\n        'limit' => 80,\r\n        'lowercase' => true,\r\n        'replacements' => array(\r\n            '/\\b(an)\\b/i' => 'a',\r\n            '/\\b(example)\\b/i' => 'Test'\r\n        )\r\n    ));\r\n    return \$slug . '.html';\r\n}";
$new = "function Wo_SlugPost(\$string) {\r\n    if (empty(trim(\$string))) {\r\n        \$string = 'post';\r\n    }\r\n    \$slug = url_slug(\$string, array(\r\n        'delimiter' => '-',\r\n        'limit' => 80,\r\n        'lowercase' => true,\r\n        'replacements' => array(\r\n            '/\\b(an)\\b/i' => 'a',\r\n            '/\\b(example)\\b/i' => 'Test'\r\n        )\r\n    ));\r\n    return \$slug . '.html';\r\n}";
$result = str_replace($old, $new, $content);
if ($result === $content) {
    echo "ERROR: Old string not found!\n";
    echo "File size: " . strlen($content) . " bytes\n";
    exit(1);
}
file_put_contents($file, $result);
echo "SUCCESS: Wo_SlugPost updated\n";
