<?php
$file = "/var/www/html/api/phone/new_post.php";
$content = file_get_contents($file);

// Replace the session validation to also check T_APP_SESSIONS
$old = <<<'OLD'
        } else if ($wo['loggedin'] == false) {
            $json_error_data = array(
                'api_status' => '400',
                'api_text' => 'failed',
                'api_version' => $api_version,
                'errors' => array(
                    'error_id' => '6',
                    'error_text' => 'Session id is wrong.'
                )
            );
            header("Content-type: application/json");
            echo json_encode($json_error_data, JSON_PRETTY_PRINT);
            exit();
OLD;

$new = <<<'NEW'
        } else if ($wo['loggedin'] == false) {
            // Also check access_token from s/session against T_APP_SESSIONS
            $app_session_id = !empty($_POST['session']) ? $_POST['session'] : (!empty($_POST['s']) ? $_POST['s'] : '');
            if (!empty($app_session_id)) {
                $check_session = $db->where('session_id', Wo_Secure($app_session_id))->getOne(T_APP_SESSIONS);
                if (empty($check_session)) {
                    $json_error_data = array(
                        'api_status' => '400',
                        'api_text' => 'failed',
                        'api_version' => $api_version,
                        'errors' => array(
                            'error_id' => '6',
                            'error_text' => 'Session id is wrong.'
                        )
                    );
                    header("Content-type: application/json");
                    echo json_encode($json_error_data, JSON_PRETTY_PRINT);
                    exit();
                }
            } else {
                $json_error_data = array(
                    'api_status' => '400',
                    'api_text' => 'failed',
                    'api_version' => $api_version,
                    'errors' => array(
                        'error_id' => '6',
                        'error_text' => 'Session id is wrong.'
                    )
                );
                header("Content-type: application/json");
                echo json_encode($json_error_data, JSON_PRETTY_PRINT);
                exit();
            }
NEW;

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed phone new_post.php\n";
