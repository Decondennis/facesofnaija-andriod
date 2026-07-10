<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) {
    echo "DB Error: " . $db->connect_error . "\n";
} else {
    $r = $db->query("SHOW COLUMNS FROM wo_config LIKE '%api%'");
    if ($r) {
        echo "API-related columns:\n";
        while ($row = $r->fetch_assoc()) {
            echo "  " . $row["Field"] . " (" . $row["Type"] . ")\n";
        }
        $r->free();
    }
    
    // Also try direct query
    $r = $db->query("SELECT * FROM wo_config LIMIT 5");
    if ($r) {
        echo "\nFirst 5 config rows:\n";
        $finfo = $r->fetch_fields();
        $cols = [];
        foreach ($finfo as $f) {
            $cols[] = $f->name;
        }
        echo "  Columns: " . implode(", ", $cols) . "\n";
        while ($row = $r->fetch_assoc()) {
            echo "  " . $row["name"] . " = " . substr($row["value"] ?? "", 0, 50) . "\n";
        }
    }
    
    // Try to find the api key
    $r = $db->query("SELECT name, value FROM wo_config WHERE name LIKE '%api%' OR name LIKE '%key%' OR name LIKE '%server%' LIMIT 10");
    if ($r) {
        echo "\n\nAPI/server key config:\n";
        while ($row = $r->fetch_assoc()) {
            echo "  " . $row["name"] . " = " . substr($row["value"] ?? "", 0, 60) . "\n";
        }
    }
    
    $db->close();
}
