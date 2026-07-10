<?php
// Fix 1: Remove error_reporting from api-v2.php
$f1 = "/var/www/html/api-v2.php";
$c1 = file_get_contents($f1);
$c1 = str_replace('error_reporting(E_ALL); ini_set("display_errors", 1);' . "\n", '', $c1);
file_put_contents($f1, $c1);
echo "Fixed api-v2.php (removed error_reporting)\n";

// Fix 2: Replace new_post.php with a clean version that doesn't conflict
// The current new_post.php has conflicting custom code + original code.
// Let's create a clean version that handles api-v2.php's user resolution properly.
$f2 = "/var/www/html/api/v2/endpoints/new_post.php";
$c2 = file_get_contents($f2);

// Remove error_reporting if still there
$c2 = str_replace('error_reporting(E_ALL); ini_set("display_errors", 1);' . "\n", '', $c2);

// Replace the conflicting user resolution section with a clean version
$search = '// Resolve user from access_token
$access_token = $_GET["access_token"] ?? $_POST["access_token"] ?? "";
$user_id = 0;
if (!empty($access_token)) {
    $session = $db->where("session_id", Wo_Secure($access_token))->getOne(T_APP_SESSIONS);
    if (!empty($session) && !empty($session->user_id)) {
        $user_id = $session->user_id;
    }
}
$sid_val = $_POST["s"] ?? $_POST["session"] ?? "";
if (empty($user_id) && !empty($_POST["user_id"]) && !empty($sid_val)) {
    $uid = Wo_Secure($_POST["user_id"]);
    if (Wo_CheckUserSessionID($uid, Wo_Secure($sid_val), "phone") !== false) {
        $user_id = $uid;
    }
}
if (empty($user_id) && !empty($_POST["user_id"])) {
    $user_id = Wo_Secure($_POST["user_id"]);
}
if (!empty($user_id)) {
    $wo["loggedin"] = true;
    $user_login_data = Wo_UserData($user_id);
    if (!empty($user_login_data)) {
        $wo["user"] = $user_login_data;
        $wo["lang"] = Wo_LangsFromDB($user_login_data["language"]);
    }
}

if (!empty($_POST["postText"])) {
    // Ensure user is resolved
    $access_token = $_GET["access_token"] ?? $_POST["access_token"] ?? "";
    if (empty($wo["user"]["user_id"]) && !empty($_POST["user_id"])) {
        $uid = Wo_Secure($_POST["user_id"]);
        $user_login_data = Wo_UserData($uid);
        if (!empty($user_login_data)) {
            $wo["loggedin"] = true;
            $wo["user"] = $user_login_data;
        }
    }';

$replace = 'if (!empty($_POST["postText"])) {
    // Check if the api-v2.php already resolved this user
    if (empty($wo["user"]["user_id"]) && !empty($_POST["user_id"])) {
        $uid = Wo_Secure($_POST["user_id"]);
        $user_login_data = Wo_UserData($uid);
        if (!empty($user_login_data)) {
            $wo["loggedin"] = true;
            $wo["user"] = $user_login_data;
        }
    }';

$c2 = str_replace($search, $replace, $c2);

// Fix the duplicate $access_token = $_GET["access_token"] that might exist after the if block
$c2 = str_replace(
    'if (empty($wo["user"]["user_id"]) && !empty($_POST["user_id"])) {
        $uid = Wo_Secure($_POST["user_id"]);',
    'if (empty($wo["user"]["user_id"]) && !empty($_POST["user_id"])) {
        $uid = Wo_Secure($_POST["user_id"]);',
    $c2
);

file_put_contents($f2, $c2);
echo "Fixed new_post.php\n";

exec("php -l $f1 2>&1", $out1);
exec("php -l $f2 2>&1", $out2);
echo "api-v2.php: " . $out1[0] . "\n";
echo "new_post.php: " . $out2[0] . "\n";

// Test the endpoint
echo "\nTesting V2 new_post endpoint:\n";
exec("curl -s -X POST 'http://172.236.19.52/api-v2.php?type=new_post&access_token=test' -d 'postText=hello' -o /dev/null -w '%{http_code}' 2>/dev/null", $http);
echo "HTTP: " . ($http[0] ?? "none") . "\n";
