<?php
$mysqli = new mysqli('localhost', 'facesofnaija_user', 'FacesDB_2026!', 'facesofnaija');
if ($mysqli->connect_error) {
    echo 'connect_error=' . $mysqli->connect_error . PHP_EOL;
    exit(1);
}
$mysqli->query("UPDATE Wo_Config SET value='0' WHERE name='smooth_loading'");
$res = $mysqli->query("SELECT name, value FROM Wo_Config WHERE name='smooth_loading'");
$row = $res ? $res->fetch_assoc() : null;
echo 'smooth_loading_now=' . ($row ? $row['value'] : 'unknown') . PHP_EOL;
