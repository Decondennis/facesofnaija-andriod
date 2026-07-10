<?php
$file = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$content = file_get_contents($file);

// Replace ajaxForm with standard jQuery AJAX form submission
$old = <<<'HTM'
   $(function() {
     $('form.create-group-form').ajaxForm({
       url: Wo_Ajax_Requests_File() + '?f=groups&s=create_group',
       beforeSend: function() {
         $('.create-group-form').find('.add_wow_loader').addClass('btn-loading');
       },
       success: function(data) {
         scrollToTop();
         if (data.status == 200) {
           window.location.href = data.location;
         } else {
             var errors = data.errors.join("<br>");
             $('.app-general-alert').html('<div class="alert alert-danger">' + errors + '</div>');
             $('.alert-danger').fadeIn(300);
         }
         $('.create-group-form').find('.add_wow_loader').removeClass('btn-loading');
       }
     });
   });
HTM;

$new = <<<'HTM'
   $('form.create-group-form').on('submit', function(e) {
     e.preventDefault();
     var formData = $(this).serialize();
     $('.create-group-form').find('.add_wow_loader').addClass('btn-loading');
     $.post(Wo_Ajax_Requests_File() + '?f=groups&s=create_group', formData, function(data) {
       scrollToTop();
       if (data.status == 200) {
         window.location.href = data.location;
       } else {
         if (data.errors) {
           var errors = data.errors.join ? data.errors.join("<br>") : data.errors;
           $('.app-general-alert').html('<div class="alert alert-danger">' + errors + '</div>');
         } else {
           $('.app-general-alert').html('<div class="alert alert-danger">An unknown error occurred</div>');
         }
         $('.alert-danger').fadeIn(300);
       }
       $('.create-group-form').find('.add_wow_loader').removeClass('btn-loading');
     }, 'json').fail(function() {
       $('.app-general-alert').html('<div class="alert alert-danger">Server error. Please try again.</div>');
       $('.alert-danger').fadeIn(300);
       $('.create-group-form').find('.add_wow_loader').removeClass('btn-loading');
     });
   });
HTM;

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Replaced ajaxForm with standard jQuery POST\n";
