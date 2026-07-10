<?php
$response_data = array('api_status' => 400);

$session_id = Wo_Secure($_POST['s'] ?? $_GET['s'] ?? '');
$user_data = Wo_GetUserFromSessionID($session_id);
if (empty($user_data) || !is_array($user_data) || empty($user_data['user_id'])) {
    $response_data['message'] = 'Invalid session';
    echo json_encode($response_data);
    exit;
}

$post_id = $_POST['post_id'] ?? $_POST['id'] ?? 0;
$text = $_POST['text'] ?? $_POST['comment_text'] ?? '';

if (empty($post_id) || !is_numeric($post_id)) {
    $response_data['message'] = 'Invalid post id';
    echo json_encode($response_data);
    exit;
}

if (empty($text)) {
    $response_data['message'] = 'Comment text is empty';
    echo json_encode($response_data);
    exit;
}

$comment_data = array(
    'post_id' => Wo_Secure($post_id),
    'user_id' => Wo_Secure($user_data['user_id']),
    'text' => Wo_Secure($text),
    'time' => time()
);

if (!empty($_POST['page_id']) && is_numeric($_POST['page_id'])) {
    $comment_data['page_id'] = Wo_Secure($_POST['page_id']);
}

$insert_id = Wo_RegisterPostComment($comment_data);
if ($insert_id && is_numeric($insert_id)) {
    $comment = Wo_GetPostComment($insert_id);
    if ($comment && is_array($comment)) {
        $response_data = array(
            'api_status' => 200,
            'data' => $comment
        );
    } else {
        $response_data = array(
            'api_status' => 200,
            'data' => array('id' => $insert_id)
        );
    }
} else {
    $response_data['message'] = 'Failed to create comment';
}

echo json_encode($response_data);
?>