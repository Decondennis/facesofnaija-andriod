<?php
// Debug script to inject logging into get_news_feed.php and test it
$file = '/var/www/html/api/phone/get_news_feed.php';
$original = file_get_contents($file);

// Add debug lines after specific anchors
$lines = [
    '// Resolve user from access_token (V2 style)' => "error_log('[FNF_DEBUG] access_token=' . (\$_GET['access_token'] ?? '') . ' user_id=' . \$user_id . ' line=' . __LINE__);",
    '// Fallback to V1 session (s parameter)' => "error_log('[FNF_DEBUG] checking V1 session s=' . S_S . ' uid=' . \$uid . ' line=' . __LINE__);",
    '// Last resort: use POST user_id directly' => "error_log('[FNF_DEBUG] last resort s=' . S_S . ' POST[user_id]=' . \$_POST['user_id'] . ' line=' . __LINE__);",
    'if (empty($user_id)) {' => "error_log('[FNF_DEBUG] user_id is empty at line ' . __LINE__);",
    "if (!empty(\$post_data)) {" => "error_log('[FNF_DEBUG] post_data found, id=' . \$fetched_post['id'] . ' line=' . __LINE__);",
];

$injected = $original;
foreach ($lines as $anchor => $debugLine) {
    $injected = str_replace($anchor, $debugLine . "\n    " . $anchor, $injected);
}

file_put_contents($file, $injected);
echo "File modified. Running test...\n";

// Run the test
$_GET['application'] = 'phone';
$_GET['type'] = 'get_news_feed';
$_POST['user_id'] = '1';
$_POST['s'] = 'test';
$_POST['limit'] = '1';
$_POST['offset'] = '0';
$_SERVER['REQUEST_METHOD'] = 'POST';

ob_start();
require_once '/var/www/html/assets/init.php';
require_once '/var/www/html/api/phone/core/functions.php';
$api_version = '1.5.2';
include $file;
$output = ob_get_clean();
echo "HTTP Response:\n" . substr($output, 0, 500) . "\n";
