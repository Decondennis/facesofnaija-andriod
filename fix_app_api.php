<?php
$file = '/var/www/html/app_api.php';
$content = file_get_contents($file);
$search = 'if (!empty($_GET[\'type\'])) {
    $type = Wo_Secure($_GET[\'type\']);
}
include_once(\'assets/libraries/twilio/vendor/autoload.php\');';
$replace = 'if (!empty($_GET[\'type\'])) {
    $type = Wo_Secure($_GET[\'type\']);
}
// Normalize session parameter: accept "session" from POST as alias for "s"
if (empty($_POST[\'s\']) && !empty($_POST[\'session\'])) {
    $_POST[\'s\'] = $_POST[\'session\'];
}
include_once(\'assets/libraries/twilio/vendor/autoload.php\');';
$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Done\n";
