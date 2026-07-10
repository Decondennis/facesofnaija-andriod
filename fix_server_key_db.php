<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) { die("DB error: " . $db->connect_error); }

// Check if widnows_app_api_key exists
$r = $db->query("SELECT name FROM wo_config WHERE name = 'widnows_app_api_key'");
if ($r && $r->num_rows > 0) {
    echo "Key exists, updating value...\n";
    $db->query("UPDATE wo_config SET value = 'faceoff_session_validator_2026' WHERE name = 'widnows_app_api_key'");
} else {
    echo "Key doesn't exist, inserting...\n";
    $db->query("INSERT INTO wo_config (name, value) VALUES ('widnows_app_api_key', 'faceoff_session_validator_2026')");
}
echo "Done\n";
$db->close();
