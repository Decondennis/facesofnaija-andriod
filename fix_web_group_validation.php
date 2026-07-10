<?php
$file = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$content = file_get_contents($file);

// Add client-side validation after the existing script block
$validation_script = '
   // Client-side validation for group_name
   $("form.create-group-form").on("submit", function(e) {
       var name = $("#group_name").val().trim();
       if (name.length < 5 || name.length > 32) {
           $(".app-general-alert").html(\'<div class="alert alert-danger">Group URL must be between 5 and 32 characters</div>\');
           e.preventDefault();
           return false;
       }
       if (!/^[\w]+$/.test(name)) {
           $(".app-general-alert").html(\'<div class="alert alert-danger">Invalid group name characters. Use only letters, numbers, and underscores.</div>\');
           e.preventDefault();
           return false;
       }
   });
';

$search = '   $(function() {
     $(\'form.create-group-form\').ajaxForm({';
$replace = $search . "\n" . $validation_script . "\n";

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Added web client-side validation\n";
