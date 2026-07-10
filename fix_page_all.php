<?php
// 1. Fix web template - replace ajaxForm with $.post
$file = "/var/www/html/themes/facesofnaija/layout/page/create-page.phtml";
$content = file_get_contents($file);

$old = "\$('form.create-page-form').ajaxForm({
       url: Wo_Ajax_Requests_File() + '?f=pages&s=create_page',
       beforeSend: function() {
         \$('.create-page-form').find('.add_wow_loader').addClass('btn-loading');
       },
       success: function(data) {
         scrollToTop();
         if (data.status == 200) {
           window.location.href = data.location;
         } else {
             var errors = data.errors.join(\"<br>\");
             \$('.app-general-alert').html('<div class=\"alert alert-danger\">' + errors + '</div>');
             \$('.alert-danger').fadeIn(300);
         }
         \$('.create-page-form').find('.add_wow_loader').removeClass('btn-loading');
       }
     });";

$new = "\$('form.create-page-form').on('submit', function(e) {
     e.preventDefault();
     var formData = \$(this).serialize();
     \$('.create-page-form').find('.add_wow_loader').addClass('btn-loading');
     \$.post(Wo_Ajax_Requests_File() + '?f=pages&s=create_page', formData, function(data) {
       scrollToTop();
       if (data.status == 200) {
         window.location.href = data.location;
       } else {
         if (data.errors) {
           var errors = data.errors.join ? data.errors.join('<br>') : data.errors;
           \$('.app-general-alert').html('<div class=\"alert alert-danger\">' + errors + '</div>');
         } else {
           \$('.app-general-alert').html('<div class=\"alert alert-danger\">An unknown error occurred</div>');
         }
         \$('.alert-danger').fadeIn(300);
       }
       \$('.create-page-form').find('.add_wow_loader').removeClass('btn-loading');
     }, 'json').fail(function() {
       \$('.app-general-alert').html('<div class=\"alert alert-danger\">Server error. Please try again.</div>');
       \$('.alert-danger').fadeIn(300);
       \$('.create-page-form').find('.add_wow_loader').removeClass('btn-loading');
     });
   });";

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed create-page.phtml\n";

// 2. Fix xhr/pages.php - add auto-slugify for page_name and default $data
$file2 = "/var/www/html/xhr/pages.php";
$content2 = file_get_contents($file2);

// Add default $data after if ($s == 'create_page') {
$search = "if (\$s == 'create_page') {\n";
$replace = "if (\$s == 'create_page') {\n        \$data = array('status' => 400, 'errors' => array('Unknown error'));\n";
$content2 = str_replace($search, $replace, $content2);

// Replace the invalid page_name characters error with auto-slugify
$old2 = "            if (!preg_match('/^[\\\w]+\$/', \$_POST['page_name'])) {
                \$errors[] = \$error_icon . \$wo['lang']['page_name_invalid_characters'];
            }";
$new2 = "            if (!preg_match('/^[\\\w]+\$/', \$_POST['page_name'])) {
                \$slug = preg_replace('/[^\\\w]/', '_', \$_POST['page_name']);
                \$slug = preg_replace('/_+/', '_', \$slug);
                \$slug = trim(\$slug, '_');
                if (strlen(\$slug) < 5) {
                    \$slug = \$slug . str_repeat('_', 5 - strlen(\$slug));
                }
                if (strlen(\$slug) > 32) {
                    \$slug = substr(\$slug, 0, 32);
                }
                \$_POST['page_name'] = \$slug;
            }";
$content2 = str_replace($old2, $new2, $content2);

file_put_contents($file2, $content2);
echo "Fixed xhr/pages.php\n";

// 3. Check PHP syntax
exec("php -l $file2 2>&1", $out, $ret);
echo implode("\n", $out) . "\n";

// 4. Fix V2 API create-page.php similar to create-group.php
$file3 = "/var/www/html/api/v2/endpoints/create-page.php";
if (file_exists($file3)) {
    $content3 = file_get_contents($file3);
    $old3 = "    } else if (!preg_match('/^[\\\w]+\$/', \$_POST['page_name'])) {
        \$error_code    = 6;
        \$error_message = 'Invalid Page name characters';
    }";
    $new3 = "    } else {
        \$slug = preg_replace('/[^\\\w]/', '_', \$_POST['page_name']);
        \$slug = preg_replace('/_+/', '_', \$slug);
        \$slug = trim(\$slug, '_');
        if (strlen(\$slug) < 5) {
            \$slug = \$slug . str_repeat('_', 5 - strlen(\$slug));
        }
        if (strlen(\$slug) > 32) {
            \$slug = substr(\$slug, 0, 32);
        }
        \$_POST['page_name'] = \$slug;
    }";
    $content3 = str_replace($old3, $new3, $content3);
    file_put_contents($file3, $content3);
    echo "Fixed api/v2/endpoints/create-page.php\n";
    exec("php -l $file3 2>&1", $out3, $ret3);
    echo implode("\n", $out3) . "\n";
}

// 5. Fix phone API create_page.php
$file4 = "/var/www/html/api/phone/create_page.php";
if (file_exists($file4)) {
    $content4 = file_get_contents($file4);
    $old4 = "                if (!preg_match('/^[\\\w]+\$/', \$_POST['page_name'])) {
                    \$errors[] = \$wo['lang']['page_name_invalid_characters'];
                }";
    $new4 = "                if (!preg_match('/^[\\\w]+\$/', \$_POST['page_name'])) {
                    \$slug = preg_replace('/[^\\\w]/', '_', \$_POST['page_name']);
                    \$slug = preg_replace('/_+/', '_', \$slug);
                    \$slug = trim(\$slug, '_');
                    if (strlen(\$slug) < 5) {
                        \$slug = \$slug . str_repeat('_', 5 - strlen(\$slug));
                    }
                    if (strlen(\$slug) > 32) {
                        \$slug = substr(\$slug, 0, 32);
                    }
                    \$_POST['page_name'] = \$slug;
                }";
    $content4 = str_replace($old4, $new4, $content4);
    file_put_contents($file4, $content4);
    echo "Fixed api/phone/create_page.php\n";
    exec("php -l $file4 2>&1", $out4, $ret4);
    echo implode("\n", $out4) . "\n";
}
