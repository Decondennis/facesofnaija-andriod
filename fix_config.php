<?php
$f = "/var/www/html/config.php";
$c = file_get_contents($f);
$c = str_replace('$sql_db_user = "root";', '$sql_db_user = "facesofnaija_user";', $c);
$c = str_replace('$sql_db_pass = "";', '$sql_db_pass = "FacesDB_2026!";', $c);
$c = str_replace('$site_url = "http://facesofnaija-web.local";', '$site_url = "http://172.236.19.52";', $c);
file_put_contents($f, $c);
echo "Fixed\n";
