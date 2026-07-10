<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) {
    echo "DB Error: " . $db->connect_error . "\n";
} else {
    // Show all config names
    $r = $db->query("SELECT name, value FROM wo_config ORDER BY name");
    if ($r) {
        while ($row = $r->fetch_assoc()) {
            echo $row["name"] . " = " . substr($row["value"] ?? "NULL", 0, 80) . "\n";
        }
    }
    $db->close();
}
