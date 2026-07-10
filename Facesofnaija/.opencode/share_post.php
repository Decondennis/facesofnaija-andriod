<?php
// Dedicated share-post endpoint for the WoWonder SDK
// Matches the SDK's SharePostToObject response format
require_once('../assets/init.php');

header("Content-type: application/json");

$access_token = $_GET['access_token'] ?? $_POST['access_token'] ?? '';
$type = $_POST['type'] ?? '';
$id = $_POST['id'] ?? $_POST['story_id'] ?? '';
$text = $_POST['text'] ?? '';
$user_id_param = $_POST['user_id'] ?? '';
$page_id_input = $_POST['page_id'] ?? '';
$group_id = $_POST['group_id'] ?? '';
$server_key = $_POST['server_key'] ?? '';

// Resolve user from access_token
$wo['loggedin'] = false;
if (!empty($access_token)) {
    $session = $db->where('session_id', Wo_Secure($access_token))->getOne(T_APP_SESSIONS);
    if (!empty($session) && !empty($session->user_id)) {
        $user_id_session = $session->user_id;
        $wo['loggedin'] = true;
        $wo['user'] = Wo_UserData($user_id_session);
    }
}
// Also try phone session fallback
if (!$wo['loggedin']) {
    $sid_val = $_POST['s'] ?? $_POST['session'] ?? '';
    if (!empty($_POST['user_id']) && !empty($sid_val)) {
        $uid = Wo_Secure($_POST['user_id']);
        if (Wo_CheckUserSessionID($uid, Wo_Secure($sid_val), 'phone') !== false) {
            $wo['loggedin'] = true;
            $wo['user'] = Wo_UserData($uid);
        }
    }
}

if (!$wo['loggedin'] || empty($wo['user']['user_id'])) {
    echo json_encode(array('api_status' => 400, 'error' => 'Authentication required'));
    exit;
}

if (empty($id)) {
    echo json_encode(array('api_status' => 400, 'error' => 'Post ID is required'));
    exit;
}

$logged_user_id = $wo['user']['user_id'];

if ($type == 'share_post_on_timeline' || $type == 'share_post_on_group' || $type == 'share_post_on_page') {
    $original_post = Wo_PostData($id);
    if (empty($original_post) || empty($original_post['id'])) {
        echo json_encode(array('api_status' => 400, 'error' => 'Original post not found'));
        exit;
    }

    if ($type == 'share_post_on_group' && !empty($group_id)) {
        $post_data = array(
            'user_id' => $logged_user_id,
            'group_id' => Wo_Secure($group_id),
            'postText' => Wo_Secure($text),
            'postPrivacy' => 0,
            'time' => time()
        );
        $post_id = Wo_RegisterPost($post_data);
        if ($post_id) {
            echo json_encode(array('api_status' => 200, 'post_id' => (string)$post_id, 'code' => 0));
        } else {
            echo json_encode(array('api_status' => 400, 'error' => 'Failed to create share post'));
        }
    } elseif ($type == 'share_post_on_page' && !empty($page_id_input)) {
        if (Wo_IsPageOnwer($page_id_input) === false && Wo_UserCanPostPage($page_id_input) === false) {
            echo json_encode(array('api_status' => 400, 'error' => 'Page access denied'));
            exit;
        }
        $post_data = array(
            'user_id' => $logged_user_id,
            'page_id' => Wo_Secure($page_id_input),
            'postText' => Wo_Secure($text),
            'postPrivacy' => 0,
            'time' => time()
        );
        $post_id = Wo_RegisterPost($post_data);
        if ($post_id) {
            echo json_encode(array('api_status' => 200, 'post_id' => (string)$post_id, 'code' => 0));
        } else {
            echo json_encode(array('api_status' => 400, 'error' => 'Failed to create share post'));
        }
    } else {
        // Share on timeline using postShare=1 mechanism
        $check_shared = "SELECT `id` FROM " . T_POSTS . " WHERE `post_id` = " . intval($id) . " AND `postShare` = 1 AND `user_id` = " . intval($logged_user_id);
        $check_result = mysqli_query($sqlConnect, $check_shared);
        if (mysqli_num_rows($check_result) > 0) {
            mysqli_query($sqlConnect, "DELETE FROM " . T_POSTS . " WHERE `post_id` = " . intval($id) . " AND `user_id` = " . intval($logged_user_id) . " AND `postShare` = 1");
            mysqli_query($sqlConnect, "DELETE FROM " . T_NOTIFICATION . " WHERE `post_id` = " . intval($id) . " AND `recipient_id` = " . intval($original_post['user_id']) . " AND `type` = 'share_post'");
            echo json_encode(array('api_status' => 200, 'action' => 'unshared'));
        } else {
            $insert_time = time();
            $insert_query = "INSERT INTO " . T_POSTS . " (`user_id`, `post_id`, `time`, `postShare`) VALUES (" . intval($logged_user_id) . ", " . intval($id) . ", " . $insert_time . ", 1)";
            mysqli_query($sqlConnect, $insert_query);
            $inserted_post_id = mysqli_insert_id($sqlConnect);

            $share_count = "SELECT COUNT(`id`) AS `shares` FROM " . T_POSTS . " WHERE `post_id` = " . intval($id) . " AND `postShare` = 1";
            $share_result = mysqli_query($sqlConnect, $share_count);
            $share_row = mysqli_fetch_assoc($share_result);
            $share_num = $share_row['shares'] ?? 0;
            mysqli_query($sqlConnect, "UPDATE " . T_POSTS . " SET `shares` = " . intval($share_num) . " WHERE `id` = " . intval($id));

            if ($inserted_post_id && $original_post['user_id'] != $logged_user_id) {
                $notification_data = array(
                    'recipient_id' => $original_post['user_id'],
                    'post_id' => $inserted_post_id,
                    'type' => 'share_post',
                    'url' => 'index.php?link1=post&id=' . $inserted_post_id
                );
                Wo_RegisterNotification($notification_data);
            }

            echo json_encode(array('api_status' => 200, 'action' => 'shared', 'post_id' => (string)$inserted_post_id, 'code' => 0));
        }
    }
} else {
    echo json_encode(array('api_status' => 400, 'error' => 'Invalid share type'));
}
