<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) {
    echo "DB Error: " . $db->connect_error . "\n";
} else {
    $r = $db->query("SELECT widnows_app_api_key FROM wo_config LIMIT 1");
    if ($r) {
        $row = $r->fetch_assoc();
        echo "API Key: " . ($row["widnows_app_api_key"] ?? "NULL") . "\n";
    } else {
        echo "Query failed: " . $db->error . "\n";
    }
    $db->close();
}
