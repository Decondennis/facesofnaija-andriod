<?php
$file = "/var/www/html/api/v2/endpoints/new_post.php";
$content = file_get_contents($file);

// Add authentication check after the user resolution section
$search = 'if (!empty($_POST["postText"])) {
    // Check if the api-v2.php already resolved this user';
$replace = '// Require authenticated user
if (empty($wo["user"]["user_id"])) {
    $response_data = array("api_status" => 400, "error" => "Authentication required. Please log in.");
    echo json_encode($response_data);
    exit;
}

if (!empty($_POST["postText"])) {
    // Check if the api-v2.php already resolved this user';

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Added auth check\n";
exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";

// Test the endpoint with invalid token
exec("curl -s -X POST 'http://172.236.19.52/api-v2.php?type=new_post&access_token=test' -d 'postText=hello' -o /tmp/np_test.txt -w '%{http_code}' 2>/dev/null", $http);
echo "HTTP: " . ($http[0] ?? "none") . "\n";
exec("head -c 300 /tmp/np_test.txt", $body);
echo "Body: " . ($body[0] ?? "none") . "\n";
