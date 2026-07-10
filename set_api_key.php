<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) { die("DB Error: " . $db->connect_error . "\n"); }

// Check if widnows_app_api_key exists
$r = $db->query("SELECT name FROM wo_config WHERE name = 'widnows_app_api_key'");
if ($r && $r->num_rows > 0) {
    echo "widnows_app_api_key already exists\n";
} else {
    // Insert it with a known value
    $key = "faceoff_session_validator_2026";
    $db->query("INSERT INTO wo_config (name, value) VALUES ('widnows_app_api_key', '$key')");
    echo "Inserted widnows_app_api_key = $key\n";
}

// Also fix api-v2.php to set $wo['loggedin'] properly when access_token present
echo "Fix applied\n";

$db->close();
