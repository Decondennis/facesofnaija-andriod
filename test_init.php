<?php
echo "BEFORE INIT\n";
ob_start();
require '/var/www/html/assets/init.php';
$init_out = ob_get_clean();
echo "AFTER INIT\n";
echo "Init output: " . ($init_out ?: "(none)") . "\n";
echo "Init output length: " . strlen($init_out) . "\n";
