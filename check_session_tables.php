<?php
$db = new mysqli("localhost", "facesofnaija_user", "FacesDB_2026!", "facesofnaija");
if ($db->connect_error) { die("DB error"); }
$r = $db->query("SHOW TABLES LIKE 'wo_app_sessions'");
echo "wo_app_sessions exists: " . ($r && $r->num_rows > 0 ? "yes" : "NO") . "\n";
$r2 = $db->query("SHOW TABLES LIKE 'wo_tokens'");
echo "wo_tokens exists: " . ($r2 && $r2->num_rows > 0 ? "yes" : "NO") . "\n";
if ($r2 && $r2->num_rows > 0) {
    $tokens = $db->query("SELECT COUNT(*) as cnt FROM wo_tokens");
    if ($tokens) { $row = $tokens->fetch_assoc(); echo "Tokens count: " . $row["cnt"] . "\n"; }
}
// Check what T_APP_SESSIONS constant resolves to
$r3 = $db->query("SHOW TABLES LIKE '%sessions%'");
echo "Session-like tables:\n";
while ($row = $r3->fetch_assoc()) {
    echo "  " . $row["Tables_in_facesofnaija (%sessions%)"] . "\n";
}
$db->close();
