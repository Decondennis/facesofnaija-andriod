<?php
$file = "/var/www/html/themes/facesofnaija/layout/page/create-page.phtml";
$content = file_get_contents($file);

// Replace the entire submit handler with a cleaner version
$old = <<<'HTML'
   $(function() {
     $('form.create-page-form').on('submit', function(e) {
     e.preventDefault();
     var formData = $(this).serialize();
     $('.create-page-form').find('.add_wow_loader').addClass('btn-loading');
     $.post(Wo_Ajax_Requests_File() + '?f=pages&s=create_page', formData, function(data) {
       scrollToTop();
       if (data.status == 200) {
         window.location.href = data.location;
       } else {
         if (data.errors) {
           var errors = data.errors.join ? data.errors.join('<br>') : data.errors;
           $('.app-general-alert').html('<div class="alert alert-danger">' + errors + '</div>');
         } else {
           $('.app-general-alert').html('<div class="alert alert-danger">An unknown error occurred</div>');
         }
         $('.alert-danger').fadeIn(300);
       }
       $('.create-page-form').find('.add_wow_loader').removeClass('btn-loading');
     }, 'json').fail(function() {
       $('.app-general-alert').html('<div class="alert alert-danger">Server error. Please try again.</div>');
       $('.alert-danger').fadeIn(300);
       $('.create-page-form').find('.add_wow_loader').removeClass('btn-loading');
     });
   });
   });
HTML;

$new = <<<'HTML'
   $(function() {
     $('form.create-page-form').on('submit', function(e) {
       e.preventDefault();
       var formData = $(this).serialize() + '&hash_id=' + encodeURIComponent($('[name=hash_id]', this).val());
       $('.create-page-form').find('.add_wow_loader').addClass('btn-loading');
       $.ajax({
         url: Wo_Ajax_Requests_File() + '?f=pages&s=create_page',
         type: 'POST',
         data: formData,
         dataType: 'json',
         success: function(data) {
           scrollToTop();
           if (data.status == 200) {
             window.location.href = data.location;
           } else if (data.errors) {
             var errors = typeof data.errors === 'string' ? data.errors : data.errors.join('<br>');
             $('.app-general-alert').html('<div class="alert alert-danger">' + errors + '</div>');
             $('.alert-danger').fadeIn(300);
           } else {
             $('.app-general-alert').html('<div class="alert alert-danger">Unknown error. Check console.</div>');
             $('.alert-danger').fadeIn(300);
           }
           $('.create-page-form').find('.add_wow_loader').removeClass('btn-loading');
         },
         error: function(xhr, status, error) {
           var msg = 'Server error (' + status + '). Please try again.';
           if (xhr.responseText && xhr.responseText.length < 200) msg += '<br>' + xhr.responseText;
           $('.app-general-alert').html('<div class="alert alert-danger">' + msg + '</div>');
           $('.alert-danger').fadeIn(300);
           $('.create-page-form').find('.add_wow_loader').removeClass('btn-loading');
         }
       });
     });
   });
HTML;

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed create-page.phtml JS\n";
