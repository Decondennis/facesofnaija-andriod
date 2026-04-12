<?php
// Posts API - real database implementation (WoWonder)

header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
header('Content-Type: application/json');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

define('DB_HOST', 'localhost');
define('DB_USER', 'facesofnaija_user');
define('DB_PASS', 'FacesDB_2026!');
define('DB_NAME', 'facesofnaija');
define('SITE_URL', 'http://172.236.19.52');

$type         = $_POST['type']          ?? $_GET['type']          ?? 'get_news_feed';
$limit        = (int)($_POST['limit']   ?? $_GET['limit']         ?? 15);
$after_id     = (int)($_POST['after_post_id'] ?? $_GET['after_post_id'] ?? 0);
$access_token = $_POST['access_token']  ?? $_GET['access_token']  ?? '';

$limit = max(1, min($limit, 50));

try {
    $db = new mysqli(DB_HOST, DB_USER, DB_PASS, DB_NAME);
    if ($db->connect_error) {
        throw new Exception('DB connection failed: ' . $db->connect_error);
    }
    $db->set_charset('utf8mb4');

    // Resolve current user from access_token (for is_liked)
    $current_user_id = 0;
    if (!empty($access_token)) {
        $stmt = $db->prepare('SELECT user_id FROM wo_tokens WHERE token = ? LIMIT 1');
        $stmt->bind_param('s', $access_token);
        $stmt->execute();
        $stmt->bind_result($current_user_id);
        $stmt->fetch();
        $stmt->close();
    }

    // Build paginated timeline query
    if ($after_id > 0) {
        $stmt = $db->prepare("
            SELECT p.id, p.user_id, p.page_id, p.group_id,
                   p.postText, p.postType, p.postFile, p.postPhoto,
                   p.postLink, p.postLinkTitle, p.postLinkContent, p.postLinkImage,
                   p.postYoutube, p.postVimeo, p.postShare, p.boosted,
                   p.live_time, p.live_ended, p.agora_resource_id, p.agora_sid,
                   p.time,
                   u.username, u.first_name, u.last_name, u.avatar, u.cover
            FROM wo_posts p
            INNER JOIN wo_users u ON u.user_id = p.user_id
            WHERE p.active = 1
              AND p.page_id  = 0
              AND p.group_id = 0
              AND p.event_id = 0
              AND p.id < ?
            ORDER BY p.id DESC
            LIMIT ?
        ");
        $stmt->bind_param('ii', $after_id, $limit);
    } else {
        $stmt = $db->prepare("
            SELECT p.id, p.user_id, p.page_id, p.group_id,
                   p.postText, p.postType, p.postFile, p.postPhoto,
                   p.postLink, p.postLinkTitle, p.postLinkContent, p.postLinkImage,
                   p.postYoutube, p.postVimeo, p.postShare, p.boosted,
                   p.live_time, p.live_ended, p.agora_resource_id, p.agora_sid,
                   p.time,
                   u.username, u.first_name, u.last_name, u.avatar, u.cover
            FROM wo_posts p
            INNER JOIN wo_users u ON u.user_id = p.user_id
            WHERE p.active = 1
              AND p.page_id  = 0
              AND p.group_id = 0
              AND p.event_id = 0
            ORDER BY p.id DESC
            LIMIT ?
        ");
        $stmt->bind_param('i', $limit);
    }

    $stmt->execute();
    $post_rows = $stmt->get_result()->fetch_all(MYSQLI_ASSOC);
    $stmt->close();

    if (empty($post_rows)) {
        echo json_encode(['api_status' => 200, 'data' => [], 'count' => 0, 'message' => 'Success']);
        exit;
    }

    $post_ids     = array_column($post_rows, 'id');
    $placeholders = implode(',', array_fill(0, count($post_ids), '?'));
    $types        = str_repeat('i', count($post_ids));

    // Bulk like counts
    $like_counts = array_fill_keys($post_ids, 0);
    $stmt = $db->prepare("SELECT post_id, COUNT(*) AS cnt FROM wo_likes WHERE post_id IN ($placeholders) GROUP BY post_id");
    $stmt->bind_param($types, ...$post_ids);
    $stmt->execute();
    foreach ($stmt->get_result()->fetch_all(MYSQLI_ASSOC) as $row) {
        $like_counts[$row['post_id']] = (int)$row['cnt'];
    }
    $stmt->close();

    // Bulk comment counts
    $comment_counts = array_fill_keys($post_ids, 0);
    $stmt = $db->prepare("SELECT post_id, COUNT(*) AS cnt FROM wo_comments WHERE post_id IN ($placeholders) GROUP BY post_id");
    $stmt->bind_param($types, ...$post_ids);
    $stmt->execute();
    foreach ($stmt->get_result()->fetch_all(MYSQLI_ASSOC) as $row) {
        $comment_counts[$row['post_id']] = (int)$row['cnt'];
    }
    $stmt->close();

    // Bulk reaction counts
    $reaction_counts = array_fill_keys($post_ids, ['like' => 0, 'love' => 0, 'haha' => 0, 'wow' => 0, 'sad' => 0, 'angry' => 0]);
    $stmt = $db->prepare("SELECT post_id, reaction, COUNT(*) AS cnt FROM wo_reactions WHERE post_id IN ($placeholders) AND comment_id IS NULL GROUP BY post_id, reaction");
    $stmt->bind_param($types, ...$post_ids);
    $stmt->execute();
    foreach ($stmt->get_result()->fetch_all(MYSQLI_ASSOC) as $row) {
        $rxn = strtolower($row['reaction']);
        if (array_key_exists($rxn, $reaction_counts[$row['post_id']])) {
            $reaction_counts[$row['post_id']][$rxn] = (int)$row['cnt'];
        }
    }
    $stmt->close();

    // Bulk is_liked for current user
    $liked_post_ids = [];
    if ($current_user_id > 0) {
        $stmt = $db->prepare("SELECT post_id FROM wo_likes WHERE user_id = ? AND post_id IN ($placeholders)");
        $stmt->bind_param('i' . $types, $current_user_id, ...$post_ids);
        $stmt->execute();
        foreach ($stmt->get_result()->fetch_all(MYSQLI_ASSOC) as $row) {
            $liked_post_ids[] = (int)$row['post_id'];
        }
        $stmt->close();
    }

    // Load original posts for shared posts (postShare = original post ID when postType = 'shared_post')
    $original_posts_map = [];
    $shared_ids = [];
    foreach ($post_rows as $p) {
        if ($p['postType'] === 'shared_post' && (int)$p['postShare'] > 0) {
            $shared_ids[] = (int)$p['postShare'];
        }
    }
    $shared_ids = array_unique($shared_ids);
    if (!empty($shared_ids)) {
        $sp = implode(',', array_fill(0, count($shared_ids), '?'));
        $st = str_repeat('i', count($shared_ids));
        $stmt = $db->prepare("
            SELECT p.id, p.user_id, p.postText, p.postType, p.postFile, p.postPhoto,
                   p.postLink, p.postLinkTitle, p.postLinkContent, p.postLinkImage,
                   p.postYoutube, p.postVimeo, p.time,
                   u.username, u.first_name, u.last_name, u.avatar, u.cover
            FROM wo_posts p
            INNER JOIN wo_users u ON u.user_id = p.user_id
            WHERE p.id IN ($sp) AND p.active = 1
        ");
        $stmt->bind_param($st, ...$shared_ids);
        $stmt->execute();
        foreach ($stmt->get_result()->fetch_all(MYSQLI_ASSOC) as $op) {
            $op_name   = trim($op['first_name'] . ' ' . $op['last_name']) ?: $op['username'];
            $op_avatar = !empty($op['avatar']) ? SITE_URL . '/' . ltrim($op['avatar'], '/') : '';
            $op_cover  = !empty($op['cover'])  ? SITE_URL . '/' . ltrim($op['cover'],  '/') : '';
            $op_file   = '';
            if (!empty($op['postFile'])) {
                $op_file = (strpos($op['postFile'], 'http') === 0) ? $op['postFile'] : SITE_URL . '/' . ltrim($op['postFile'], '/');
            } elseif (!empty($op['postPhoto'])) {
                $op_file = (strpos($op['postPhoto'], 'http') === 0) ? $op['postPhoto'] : SITE_URL . '/' . ltrim($op['postPhoto'], '/');
            }
            $op_pub = [
                'id' => (string)$op['user_id'], 'user_id' => (string)$op['user_id'],
                'name' => $op_name, 'username' => $op['username'],
                'avatar' => $op_avatar, 'cover' => $op_cover,
                'url' => SITE_URL . '/' . $op['username'],
            ];
            $original_posts_map[(int)$op['id']] = [
                'post_id' => (string)$op['id'], 'id' => (string)$op['id'],
                'user_id' => (string)$op['user_id'], 'page_id' => '0', 'group_id' => '0',
                'orginal_text' => $op['postText'] ?? '', 'postText' => $op['postText'] ?? '',
                'postType' => !empty($op['postType']) ? $op['postType'] : 'post',
                'postFile' => $op_file, 'postFile_full' => $op_file,
                'postLink' => $op['postLink'] ?? '', 'postLinkTitle' => $op['postLinkTitle'] ?? '',
                'postLinkImage' => !empty($op['postLinkImage']) ? SITE_URL . '/' . ltrim($op['postLinkImage'], '/') : '',
                'postLinkContent' => $op['postLinkContent'] ?? '',
                'postYoutube' => $op['postYoutube'] ?? '', 'postVimeo' => $op['postVimeo'] ?? '',
                'time' => (int)$op['time'],
                'comment_count' => '0', 'like_count' => '0', 'share_count' => '0',
                'is_liked' => false, 'color_id' => '0', 'poll_id' => '0',
                'blog_id' => '0', 'fund_id' => '0', 'event_id' => '0',
                'publisher' => $op_pub, 'user_data' => $op_pub,
                'shared_info' => ['shared_post_id' => '', 'shared_post_info_class' => null],
            ];
        }
        $stmt->close();
    }

    // Format posts
    $posts = [];
    foreach ($post_rows as $p) {
        $pid      = (int)$p['id'];
        $fullname = trim($p['first_name'] . ' ' . $p['last_name']);
        if ($fullname === '') $fullname = $p['username'];
        $avatar   = !empty($p['avatar']) ? SITE_URL . '/' . ltrim($p['avatar'], '/') : '';
        $cover    = !empty($p['cover'])  ? SITE_URL . '/' . ltrim($p['cover'],  '/') : '';

        $publisher = [
            'id'       => (string)$p['user_id'],
            'user_id'  => (string)$p['user_id'],
            'name'     => $fullname,
            'username' => $p['username'],
            'avatar'   => $avatar,
            'cover'    => $cover,
            'url'      => SITE_URL . '/' . $p['username'],
        ];

        $post_file = '';
        if (!empty($p['postFile'])) {
            $post_file = (strpos($p['postFile'], 'http') === 0)
                ? $p['postFile']
                : SITE_URL . '/' . ltrim($p['postFile'], '/');
        } elseif (!empty($p['postPhoto'])) {
            $post_file = (strpos($p['postPhoto'], 'http') === 0)
                ? $p['postPhoto']
                : SITE_URL . '/' . ltrim($p['postPhoto'], '/');
        }

        $posts[] = [
            // --- identity ---
            'post_id'            => (string)$pid,
            'id'                 => (string)$pid,
            'user_id'            => (string)$p['user_id'],
            'page_id'            => (string)$p['page_id'],
            'group_id'           => (string)$p['group_id'],

            // --- text (DLL expects orginal_text + postText) ---
            'orginal_text'       => $p['postText'] ?? '',
            'postText'           => $p['postText'] ?? '',

            // --- type (DLL expects postType camelCase) ---
            'postType'           => !empty($p['postType']) ? $p['postType'] : 'post',

            // --- file (DLL expects postFile + postFile_full) ---
            'postFile'           => $post_file,
            'postFile_full'      => $post_file,
            'postFileName'       => $p['postFileName'] ?? '',
            'postFileThumb'      => !empty($p['postFileThumb']) ? SITE_URL . '/' . ltrim($p['postFileThumb'], '/') : '',

            // --- link (DLL expects postLink camelCase) ---
            'postLink'           => $p['postLink'] ?? '',
            'postLinkTitle'      => $p['postLinkTitle'] ?? '',
            'postLinkImage'      => !empty($p['postLinkImage']) ? SITE_URL . '/' . ltrim($p['postLinkImage'], '/') : '',
            'postLinkContent'    => $p['postLinkContent'] ?? '',

            // --- media embeds (DLL expects camelCase) ---
            'postYoutube'        => $p['postYoutube'] ?? '',
            'postVimeo'          => $p['postVimeo'] ?? '',
            'postFacebook'       => $p['postFacebook'] ?? '',
            'postDeepsound'      => $p['postDeepsound'] ?? '',
            'postPlaytube'       => $p['postPlaytube'] ?? '',
            'postSticker'        => $p['postSticker'] ?? '',
            'postRecord'         => $p['postRecord'] ?? '',
            'postMap'            => $p['postMap'] ?? '',

            // --- counts / engagement ---
            'time'               => (int)$p['time'],
            'edited_time'        => (int)$p['time'],
            'comment_count'      => (string)($comment_counts[$pid] ?? 0),
            'like_count'         => (string)($like_counts[$pid] ?? 0),
            'share_count'        => (string)($p['postShare'] ?? 0),
            'is_liked'           => in_array($pid, $liked_post_ids),
            'is_post_boosted'    => $p['boosted'] ? '1' : '0',
            'boosted_status'     => $p['boosted'] ? '1' : '0',
            'boosted_time'       => '',
            'boosted_price'      => '0',
            'boosted_end_time'   => '',

            // --- live ---
            'is_still_live'      => ((int)$p['live_ended'] === 0 && (int)$p['live_time'] > 0),
            'live_time'          => ((int)$p['live_time'] > 0) ? (string)$p['live_time'] : null,
            'agora_resource_id'  => $p['agora_resource_id'] ?? '',
            'agora_uid'          => '',
            'agora_channel_name' => $p['agora_sid'] ?? '',

            // --- feature ids ---
            'color_id'           => '0',
            'poll_id'            => '0',
            'blog_id'            => '0',
            'fund_id'            => '0',
            'event_id'           => '0',

            // --- relations ---
            'publisher'          => $publisher,
            'user_data'          => $publisher,
            'shared_info'        => [
                'shared_post_id'         => ($p['postType'] === 'shared_post' && (int)$p['postShare'] > 0) ? (string)$p['postShare'] : '',
                'shared_post_info_class' => ($p['postType'] === 'shared_post' && (int)$p['postShare'] > 0) ? ($original_posts_map[(int)$p['postShare']] ?? null) : null,
            ],
            'reactions'          => $reaction_counts[$pid],
        ];
    }

    echo json_encode([
        'api_status' => 200,
        'data'       => $posts,
        'count'      => count($posts),
        'message'    => 'Success',
    ]);

} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([
        'api_status' => 500,
        'message'    => 'Server error: ' . $e->getMessage(),
        'data'       => [],
    ]);
}
?>
