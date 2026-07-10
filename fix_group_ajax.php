<?php
$f = "/var/www/html/themes/facesofnaija/layout/group/create-group.phtml";
$c = file_get_contents($f);

// Replace $.post with $.ajax including headers
$old = "\$.post(Wo_Ajax_Requests_File() + '?f=groups&s=create_group', formData, function(data) {
       scrollToTop();
       if (data.status == 200) {
         window.location.href = data.location;
       } else {
         if (data.errors) {
           var errors = data.errors.join ? data.errors.join(\"<br>\") : data.errors;
           \$('.app-general-alert').html('<div class=\"alert alert-danger\">' + errors + '</div>');
         } else {
           \$('.app-general-alert').html('<div class=\"alert alert-danger\">An unknown error occurred</div>');
         }
         \$('.alert-danger').fadeIn(300);
       }
       \$('.create-group-form').find('.add_wow_loader').removeClass('btn-loading');
     }, 'json').fail(function() {
       \$('.app-general-alert').html('<div class=\"alert alert-danger\">Server error. Please try again.</div>');
       \$('.alert-danger').fadeIn(300);
       \$('.create-group-form').find('.add_wow_loader').removeClass('btn-loading');
     });";

$new = "\$.ajax({
         url: Wo_Ajax_Requests_File() + '?f=groups&s=create_group',
         type: 'POST',
         headers: { 'X-Requested-With': 'XMLHttpRequest' },
         data: formData,
         dataType: 'json',
         success: function(data) {
           scrollToTop();
           if (data.status == 200) {
             window.location.href = data.location;
           } else if (data.errors) {
             var errors = typeof data.errors === 'string' ? data.errors : data.errors.join('<br>');
             \$('.app-general-alert').html('<div class=\"alert alert-danger\">' + errors + '</div>');
             \$('.alert-danger').fadeIn(300);
           } else {
             \$('.app-general-alert').html('<div class=\"alert alert-danger\">Unknown error.</div>');
             \$('.alert-danger').fadeIn(300);
           }
           \$('.create-group-form').find('.add_wow_loader').removeClass('btn-loading');
         },
         error: function(xhr, status, error) {
           var msg = 'Server error (' + status + ').';
           msg += '<hr><pre>' + (xhr.responseText || '').substring(0,500) + '</pre>';
           \$('.app-general-alert').html('<div class=\"alert alert-danger\">' + msg + '</div>');
           \$('.alert-danger').fadeIn(300);
           \$('.create-group-form').find('.add_wow_loader').removeClass('btn-loading');
         }
       });";

$c = str_replace($old, $new, $c);
file_put_contents($f, $c);
echo "Fixed create-group.phtml\n";
