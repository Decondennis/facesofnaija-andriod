<?php
$file = "/var/www/html/api/v2/endpoints/new_post.php";
$content = file_get_contents($file);

// Remove error_reporting line
$content = str_replace('error_reporting(E_ALL); ini_set("display_errors", 1);' . "\n", '', $content);

// Remove the duplicate user resolution at the beginning (since api-v2.php now handles it)
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
    // Ensure user is resolved from api-v2.php
    if (empty($wo["user"]["user_id"]) && !empty($access_token)) {
        $uid = Wo_UserIdFromSession($access_token);
        if (!empty($uid)) {
            $user_login_data = Wo_UserData($uid);
            if (!empty($user_login_data)) {
                $wo["loggedin"] = true;
                $wo["user"] = $user_login_data;
            }
        }
    }';

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Fixed new_post.php\n";
exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
