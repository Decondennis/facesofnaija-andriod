<?php
error_reporting(E_ALL);
ini_set('display_errors', 0);
ini_set('log_errors', 1);
try {
chdir(__DIR__ . '/../..');
require_once('assets/init.php');
require_once(__DIR__ . '/core/functions.php');

$json_error_data   = array();
$json_success_data = array();
$api_version = '1.5.2';

$type = $_GET['type'] ?? $_POST['type'] ?? '';
if (empty($type)) {
    $json_error_data = array(
        'api_status' => '400',
        'api_text' => 'failed',
        'api_version' => $api_version,
        'errors' => array(
            'error_id' => '1',
            'error_text' => 'Bad request, no type specified.'
        )
    );
    header("Content-type: application/json");
    echo json_encode($json_error_data, JSON_PRETTY_PRINT);
    exit();
}

if ($type == 'get_news_list' || $type == 'get_news_feed') {
    $access_token = $_GET['access_token'] ?? '';
    $user_id = 0;

    // Resolve user from access_token (V2 style)
    if (!empty($access_token)) {
        $session = $db->where('session_id', Wo_Secure($access_token))->getOne(T_APP_SESSIONS);
        if (!empty($session) && !empty($session->user_id)) {
            $user_id = $session->user_id;
        }
    }

    // Fallback to V1 session (s parameter)
    if (empty($user_id) && !empty($_POST['user_id']) && !empty($_POST['s'])) {
        $sid = Wo_Secure($_POST['s']);
        $uid = Wo_Secure($_POST['user_id']);
        if (Wo_CheckUserSessionID($uid, $sid, 'phone') !== false) {
            $user_id = $uid;
        }
    }

    // Last resort: use POST user_id directly
    if (empty($user_id) && !empty($_POST['user_id'])) {
        $user_id = Wo_Secure($_POST['user_id']);
    }

    if (empty($user_id)) {
        $json_error_data = array(
            'api_status' => '400',
            'api_text' => 'failed',
            'api_version' => $api_version,
            'errors' => array(
                'error_id' => '3',
                'error_text' => 'Could not resolve user.'
            )
        );
        header("Content-type: application/json");
        echo json_encode($json_error_data, JSON_PRETTY_PRINT);
        exit();
    }

    $user_login_data = Wo_UserData($user_id);
    if (empty($user_login_data)) {
        $json_error_data = array(
            'api_status' => '400',
            'api_text' => 'failed',
            'api_version' => $api_version,
            'errors' => array(
                'error_id' => '6',
                'error_text' => 'User not found.'
            )
        );
        header("Content-type: application/json");
        echo json_encode($json_error_data, JSON_PRETTY_PRINT);
        exit();
    }

    $wo['lang'] = Wo_LangsFromDB($user_login_data['language']);

    $offset = 0;
    if (!empty($_POST['offset'])) {
        $offset = Wo_Secure($_POST['offset']);
    }

    $limit = 20;
    if (!empty($_POST['limit']) && is_numeric($_POST['limit'])) {
        $limit = min(max((int)$_POST['limit'], 1), 50);
    }

    $posts_data = array();
    global $sqlConnect;

    $uid = intval($user_id);
    $lim = intval($limit);

    $query = "SELECT p.`id` FROM " . T_POSTS . " p WHERE (
        p.`user_id` = {$uid} OR
        p.`user_id` IN (
            SELECT `follower_id` FROM " . T_FOLLOWERS . " WHERE `user_id` = {$uid} AND `active` = 1
        )
    ) AND p.`active` = '1'";

    if (!empty($offset) && is_numeric($offset)) {
        $query .= " AND p.`id` < " . intval($offset) . " ";
    }

    $query .= " ORDER BY p.`id` DESC LIMIT {$lim}";

    $sql_query = mysqli_query($sqlConnect, $query);

    if ($sql_query && mysqli_num_rows($sql_query) > 0) {
        while ($fetched_post = mysqli_fetch_assoc($sql_query)) {
            $post_data = Wo_PostData($fetched_post["id"]);
            if (!empty($post_data)) {
                if (!empty($post_data['publisher'])) {
                    foreach ($non_allowed as $key => $value) {
                        unset($post_data['publisher'][$value]);
                    }
                }
                if (empty($post_data['photo_multi'])) {
                    $post_data['photo_multi'] = array();
                }
                if (empty($post_data['photo_album'])) {
                    $post_data['photo_album'] = array();
                }
                if (!empty($post_data['postFile'])) {
                    $post_data['postFile'] = Wo_GetMedia($post_data['postFile']);
                }
                if (!empty($post_data['postFileThumb'])) {
                    $post_data['postFileThumb'] = Wo_GetMedia($post_data['postFileThumb']);
                }

                $pid = $post_data['id'];

                // Calculate total post-level reaction count from wo_reactions
                $rc = $db->where('post_id', $pid)->where('comment_id', 0)->where('replay_id', 0)->where('message_id', 0)->where('story_id', 0)->getValue(T_REACTIONS, 'COUNT(*)');
                $total_reactions = max(0, (int)$rc);
                
                $post_data['like_count'] = (string)$total_reactions;
                $post_data['post_likes'] = (string)$total_reactions;
                
                // Check is_liked from either wo_likes or wo_reactions
                $is_liked_old = $db->where('user_id', $uid)->where('post_id', $pid)->has(T_LIKES);
                $is_liked_new = $db->where('user_id', $uid)->where('post_id', $pid)->where('comment_id', 0)->where('replay_id', 0)->where('message_id', 0)->where('story_id', 0)->has(T_REACTIONS);
                $post_data['is_liked'] = $is_liked_old || $is_liked_new;

                if (!isset($post_data['reaction']) || empty($post_data['reaction'])) {
                    $post_data['reaction'] = array(
                        'count' => $total_reactions,
                        'type' => '',
                        'is_reacted' => !empty($post_data['is_liked'])
                    );
                } else {
                    $post_data['reaction']['count'] = $total_reactions;
                }

                $posts_data[] = $post_data;
            }
        }
    }

    $json_success_data = array(
        'api_status' => '200',
        'api_text' => 'success',
        'api_version' => $api_version,
        'posts' => $posts_data
    );

    header("Content-type: application/json");
    echo json_encode($json_success_data, JSON_PRETTY_PRINT);
    exit();
}

header("Content-type: application/json");
echo json_encode($json_success_data);
exit();
} catch (\Throwable $e) {
    http_response_code(500);
    header('Content-Type: application/json');
    echo json_encode([
        'api_status' => 500,
        'error' => $e->getMessage(),
        'file' => $e->getFile(),
        'line' => $e->getLine()
    ]);
    exit();
}
