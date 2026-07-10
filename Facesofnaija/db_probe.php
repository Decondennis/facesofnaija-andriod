<?php
$mysqli = new mysqli('localhost', 'facesofnaija_user', 'FacesDB_2026!', 'facesofnaija');
if ($mysqli->connect_error) {
    echo 'connect_error=' . $mysqli->connect_error . PHP_EOL;
    exit(1);
}
$sql = "SELECT name, value FROM Wo_Config WHERE name IN ('header_cc','footer_cc','smooth_loading')";
$res = $mysqli->query($sql);
if (!$res) {
    echo 'query_error=' . $mysqli->error . PHP_EOL;
    exit(1);
}
while ($row = $res->fetch_assoc()) {
    echo $row['name'] . '=' . substr((string)$row['value'], 0, 600) . PHP_EOL;
}
