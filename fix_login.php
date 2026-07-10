<?php
$f = "/var/www/html/api/phone/login.php";
$c = file_get_contents($f);
// Remove the session/s check for login - login doesn't need a session
$search = "    } else if (empty(\$_POST['session']) && empty(\$_POST['s'])) {
        \$json_error_data = array(
            'api_status' => '400',
            'api_text' => 'failed',
            'api_version' => \$api_version,
            'errors' => array(
                'error_id' => '5',
                'error_text' => 'No session sent.'
            )
        );
    }
    if (empty(\$json_error_data)) {";
$replace = "    }
    if (empty(\$json_error_data)) {";
$c = str_replace($search, $replace, $c);
file_put_contents($f, $c);
echo "Fixed\n";
