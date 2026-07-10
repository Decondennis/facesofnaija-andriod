<?php
$target = "/var/www/html/fon_login.php";
$code = <<<'PHP'
<?php
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
header('Content-Type: application/json');
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') { http_response_code(200); exit; }
require_once(__DIR__ . '/assets/init.php');
$username = $_POST['username'] ?? $_POST['email'] ?? '';
$password = $_POST['password'] ?? '';
if (empty($username) || empty($password)) {
    echo json_encode(['api_status' => 400, 'error' => 'Username and password required']); exit;
}
// Try Wo_Login first
$login = Wo_Login($username, $password);
$user_id = 0;
if ($login === true) {
    $user_id = Wo_UserIdFromUsername($username);
    if (empty($user_id)) $user_id = Wo_UserIdFromEmail($username);
} else {
    $user_id = Wo_UserIdFromEmail($username);
    if (empty($user_id)) $user_id = Wo_UserIdFromUsername($username);
    if (!empty($user_id)) {
        $login = Wo_Login($username, $password);
    }
}
if (empty($user_id) || $login !== true) {
    echo json_encode(['api_status' => 400, 'error' => 'Invalid credentials']); exit;
}
// Create a proper app session
$time = time();
$token = sha1(rand(111111111, 999999999)) . md5(microtime()) . rand(11111111, 99999999) . md5(rand(5555, 9999));
$q = "INSERT INTO " . T_APP_SESSIONS . " (user_id, session_id, platform, time) VALUES ('" . intval($user_id) . "', '" . Wo_Secure($token) . "', 'phone', '" . $time . "')";
$r = mysqli_query($sqlConnect, $q) or die(json_encode(['api_status' => 500, 'error' => 'DB: ' . mysqli_error($sqlConnect), 'table' => T_APP_SESSIONS]));
echo json_encode(['api_status' => 200, 'user_id' => (string)$user_id, 'access_token' => $token, 'cookie' => $token]);
PHP;
file_put_contents($target, $code);
echo "Created /var/www/html/fon_login.php\n";
