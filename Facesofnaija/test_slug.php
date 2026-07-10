<?php
echo "starting\n";
require '/var/www/html/assets/init.php';
echo "loaded\n";
echo "Empty: " . Wo_SlugPost('') . "\n";
echo "Shared: " . Wo_SlugPost('shared') . "\n";
echo "Hello: " . Wo_SlugPost('Hello World') . "\n";
