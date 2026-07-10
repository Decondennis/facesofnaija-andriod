<?php
require '/var/www/html/assets/init.php';
echo "Wo_CountShares(1450): " . Wo_CountShares(1450) . "\n";
echo "Wo_CountShares(1519): " . Wo_CountShares(1519) . "\n";
$p1450 = Wo_PostData(1450);
echo "post_shares from Wo_PostData(1450): " . ($p1450['post_shares'] ?? 'MISSING') . "\n";
