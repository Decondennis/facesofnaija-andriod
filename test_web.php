<?php
echo "HELLO FROM TEST\n";
echo "POST user_id: " . ($_POST['user_id'] ?? 'none') . "\n";
echo "GET application: " . ($_GET['application'] ?? 'none') . "\n";
echo "GET type: " . ($_GET['type'] ?? 'none') . "\n";
echo "REQUEST_METHOD: " . ($_SERVER['REQUEST_METHOD'] ?? 'none') . "\n";
