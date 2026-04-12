<?php
// Faces of Naija - Posts API Endpoint
// Handles all post-related requests

include_once '../../includes/Config.php';
include_once '../../classes/User.php';
include_once '../../classes/Post.php';

// Enable CORS
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
header('Content-Type: application/json');

// Check authentication
$access_token = $_POST['access_token'] ?? $_GET['access_token'] ?? '';
if (empty($access_token)) {
    http_response_code(401);
    echo json_encode([
        'api_status' => 401,
        'message' => 'Unauthorized: Missing access token'
    ]);
    exit;
}

// Verify token and get user
$user = User::getByAccessToken($access_token);
if (!$user) {
    http_response_code(401);
    echo json_encode([
        'api_status' => 401,
        'message' => 'Unauthorized: Invalid access token'
    ]);
    exit;
}

// Get request parameters
$type = $_POST['type'] ?? $_GET['type'] ?? 'get_news_feed';
$limit = (int)($_POST['limit'] ?? $_GET['limit'] ?? 15);
$offset = $_POST['after_post_id'] ?? $_GET['after_post_id'] ?? '0';
$filter = $_POST['filter'] ?? $_GET['filter'] ?? 'all';
$post_type = $_POST['post_type'] ?? $_GET['post_type'] ?? 'all';

// Sanitize inputs
$limit = min($limit, 50); // Max 50 posts per request
$limit = max($limit, 1);  // Min 1 post

try {
    $response_data = [];
    
    switch ($type) {
        case 'get_news_feed':
            // Get news feed for current user (posts from friends + own posts)
            $posts = Post::getNewsFeed($user['id'], $limit, $offset, $filter, $post_type);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'get_timeline':
            // Get all public posts timeline
            $posts = Post::getPublicTimeline($limit, $offset, $post_type);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'get_user_posts':
            // Get posts from specific user
            $user_id = $_POST['user_id'] ?? $_GET['user_id'] ?? $user['id'];
            $posts = Post::getUserPosts($user_id, $limit, $offset);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'get_group_posts':
            // Get posts from specific group
            $group_id = $_POST['group_id'] ?? $_GET['group_id'] ?? '';
            if (empty($group_id)) {
                throw new Exception('Group ID is required');
            }
            $posts = Post::getGroupPosts($group_id, $limit, $offset);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'get_page_posts':
            // Get posts from specific page
            $page_id = $_POST['page_id'] ?? $_GET['page_id'] ?? '';
            if (empty($page_id)) {
                throw new Exception('Page ID is required');
            }
            $posts = Post::getPagePosts($page_id, $limit, $offset);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'get_event_posts':
            // Get posts from specific event
            $event_id = $_POST['event_id'] ?? $_GET['event_id'] ?? '';
            if (empty($event_id)) {
                throw new Exception('Event ID is required');
            }
            $posts = Post::getEventPosts($event_id, $limit, $offset);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'get_random_videos':
            // Get random video posts
            $posts = Post::getRandomVideos($limit, $offset);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'saved':
            // Get user's saved posts
            $posts = Post::getSavedPosts($user['id'], $limit, $offset);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        case 'hashtag':
            // Get posts with specific hashtag
            $hash = $_POST['hash'] ?? $_GET['hash'] ?? '';
            if (empty($hash)) {
                throw new Exception('Hashtag is required');
            }
            $posts = Post::getPostsByHashtag($hash, $limit, $offset);
            $response_data = [
                'api_status' => 200,
                'data' => $posts,
                'count' => count($posts),
                'offset' => $offset
            ];
            break;
            
        default:
            http_response_code(400);
            echo json_encode([
                'api_status' => 400,
                'message' => 'Invalid request type: ' . $type
            ]);
            exit;
    }
    
    // Return successful response
    http_response_code(200);
    echo json_encode($response_data);
    
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([
        'api_status' => 500,
        'message' => 'Server error: ' . $e->getMessage(),
        'data' => []
    ]);
}
?>
