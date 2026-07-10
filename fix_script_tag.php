<?php
$f = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$c = file_get_contents($f);
// Remove the broken line with "// Client-side group name validation</script>"
$c = str_replace("// Client-side group name validation</script>", "</script>", $c);
file_put_contents($f, $c);
echo "Fixed script tag\n";
