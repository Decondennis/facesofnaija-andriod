<?php
$file = "/var/www/html/api-v2.php";
$content = file_get_contents($file);

// Add require statement before the end of the file
$search = "\$api = \"api/v2/endpoints/\" . \$type . \".php\";\n\$pages_without_access_token = array(\"get-site-settings\",\"auth\",\"create-account\",\"social-login\",\"two-factor\",\"reset_password\");\nif (!file_exists(\$api)) { \$response_data = array(\"api_status\" => \"404\", \"errors\" => array(\"error_id\" => \"1\", \"error_text\" => \"Error: 404 API Type Not Found\")); echo json_encode(\$response_data, JSON_PRETTY_PRINT); exit(); }";

$replace = $search . "\nrequire(\$api);\n";

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Fixed api-v2.php - added require(\$api);\n";
