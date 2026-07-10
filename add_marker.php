<?php
$file = '/var/www/html/api/phone/get_news_feed.php';
$orig = file_get_contents($file);
file_put_contents($file, "UNIQUE_MARKER_12345\n" . $orig);
echo "Added marker\n";
