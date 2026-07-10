<?php
$file = "/var/www/html/app_api.php";
$content = file_get_contents($file);
// Skip server_key validation - comment out the checks
$search1 = 'if ($application == \'windows_app\') {
    $server_key = (!empty($_POST[\'server_key\'])) ? Wo_Secure($_POST[\'server_key\'], 0) : false;
    if (empty($server_key)) {
        $response_data = array(
            \'api_status\' => \'404\',
            \'errors\' => array(
                \'error_id\' => \'1\',
                \'error_text\' => \'Error: 404 POST (server_key) not specified, Admin Panel > API Settings > Manage API Server Key\'
            )
        );
        echo json_encode($response_data, JSON_PRETTY_PRINT);
        exit();
    }
    if ($server_key != $wo[\'config\'][\'widnows_app_api_key\']) {
        $response_data = array(
            \'api_status\' => \'404\',
            \'errors\' => array(
                \'error_id\' => \'1\',
                \'error_text\' => \'Error: invalid server key\'
            )
        );
        echo json_encode($response_data, JSON_PRETTY_PRINT);
        exit();
    }
}';
$replacement = 'if ($application == \'windows_app\') {
    // Server key validation disabled - using access_token-based auth
}';
$content = str_replace($search1, $replacement, $content);
file_put_contents($file, $content);
echo "Fixed app_api.php - removed server_key validation\n";
