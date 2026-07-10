<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) {
    die("DB Error: " . $db->connect_error);
}
$r = $db->query("SELECT user_id, username, email, first_name, last_name FROM wo_users WHERE active = '1' AND banned = '0' LIMIT 5");
if ($r) {
    echo "Active users:\n";
    while ($row = $r->fetch_assoc()) {
        echo "  ID: {$row['user_id']}, Username: {$row['username']}, Email: {$row['email']}, Name: {$row['first_name']} {$row['last_name']}\n";
    }
    $r->free();
}
// Check wo_app_sessions table
$r = $db->query("SHOW TABLES LIKE '%app_sessions%'");
if ($r && $r->num_rows > 0) {
    $r2 = $db->query("SELECT COUNT(*) as cnt FROM wo_app_sessions");
    if ($r2) { $row = $r2->fetch_assoc(); echo "App sessions count: {$row['cnt']}\n"; }
}
// Check wo_tokens table
$r = $db->query("SHOW TABLES LIKE '%tokens%'");
if ($r && $r->num_rows > 0) {
    $r2 = $db->query("SELECT COUNT(*) as cnt FROM wo_tokens");
    if ($r2) { $row = $r2->fetch_assoc(); echo "Tokens count: {$row['cnt']}\n"; }
}
$db->close();
