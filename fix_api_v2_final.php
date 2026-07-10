<?php
$file = "/var/www/html/api-v2.php";
$content = <<<'APIPHP'
<?php
header_remove("Server");
header("Content-type: application/json");
require("assets/init.php");
require("api/v2/init.php");
$wo["loggedin"] = false;
$response_data = array();
$error_code = 0;
$error_message = "";
$type = (!empty($_GET["type"])) ? Wo_Secure($_GET["type"], 0) : false;
$access_token = (!empty($_GET["access_token"])) ? Wo_Secure($_GET["access_token"], 0) : (!empty($_POST["access_token"]) ? Wo_Secure($_POST["access_token"], 0) : false);

if (empty($type)) {
    $response_data = array("api_status" => "404", "errors" => array("error_id" => "1", "error_text" => "Error: 404 API Type not specified"));
    echo json_encode($response_data, JSON_PRETTY_PRINT);
    exit();
}

// Resolve user from access_token if provided
$user_id = 0;
if (!empty($access_token)) {
    $session = $db->where("session_id", Wo_Secure($access_token))->getOne(T_APP_SESSIONS);
    if (!empty($session) && !empty($session->user_id)) {
        $user_id = $session->user_id;
        $wo["loggedin"] = true;
        $wo["user"] = Wo_UserData($user_id);
    }
}

$api = "api/v2/endpoints/" . $type . ".php";
if (!file_exists($api)) {
    $response_data = array("api_status" => "404", "errors" => array("error_id" => "1", "error_text" => "Error: 404 API Type Not Found"));
    echo json_encode($response_data, JSON_PRETTY_PRINT);
    exit();
}
require($api);
APIPHP;
file_put_contents($file, $content);
echo "api-v2.php rewritten\n";
