<?php
$file = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$content = file_get_contents($file);

$validation = "\n   // Client-side group name validation\n" .
"   \$('form.create-group-form').on('submit', function(e) {\n" .
"       var name = \$('#group_name').val().trim();\n" .
"       if (name.length < 5 || name.length > 32) {\n" .
"           \$('.app-general-alert').html('<div class=\"alert alert-danger\">Group URL must be between 5 and 32 characters</div>');\n" .
"           e.preventDefault();\n" .
"           return false;\n" .
"       }\n" .
"       if (!/^[\\w]+$/.test(name)) {\n" .
"           \$('.app-general-alert').html('<div class=\"alert alert-danger\">Invalid group name characters. Use only letters, numbers, and underscores.</div>');\n" .
"           e.preventDefault();\n" .
"           return false;\n" .
"       }\n" .
"   });\n";

$content = str_replace("</script>", $validation . "</script>", $content);
file_put_contents($file, $content);
echo "Added web validation script\n";
