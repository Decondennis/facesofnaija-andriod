<?php
$files = array(
    '/var/www/html/api/phone/login.php',
    '/var/www/html/api/phone/new_post.php',
    '/var/www/html/api/phone/create_comment.php',
    '/var/www/html/api/phone/get_stories.php',
    '/var/www/html/api/phone/get_announcements.php',
);

foreach ($files as $f) {
    if (!file_exists($f)) continue;
    $c = file_get_contents($f);
    
    $c = str_replace(
        "if (empty(\$_POST['s'])) {",
        "if (empty(\$_POST['session']) && empty(\$_POST['s'])) {",
        $c
    );
    $c = str_replace(
        "if (empty(\$_POST['s']) || !isset(\$_POST['s'])) {",
        "if ((empty(\$_POST['session']) && empty(\$_POST['s'])) || (!isset(\$_POST['session']) && !isset(\$_POST['s']))) {",
        $c
    );
    $c2 = $c;
    $c2 = str_replace(
        "Wo_Secure(\$_POST['s'])",
        "Wo_Secure(!empty(\$_POST['session']) ? \$_POST['session'] : \$_POST['s'])",
        $c2
    );
    $c2 = str_replace(
        "Wo_Secure(\$_POST['s'], 0)",
        "Wo_Secure(!empty(\$_POST['session']) ? \$_POST['session'] : \$_POST['s'], 0)",
        $c2
    );
    file_put_contents($f, $c2);
    echo "Fixed: $f\n";
}
