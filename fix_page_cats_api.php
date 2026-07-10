<?php
$file = "/var/www/html/api/v2/endpoints/get-site-settings.php";
$content = file_get_contents($file);

$old = "\$get_config['page_categories'] = \$wo['page_categories'];";
$new = "\$get_config['page_categories'] = !empty(\$wo['page_categories']) ? \$wo['page_categories'] : array(1=>'Business',2=>'Company',3=>'Artist',4=>'Brand',5=>'Entertainment',6=>'Food & Drink',7=>'Health',8=>'Hotel',9=>'News',10=>'Non-Profit',11=>'Organization',12=>'Public Figure',13=>'Real Estate',14=>'School',15=>'Shopping',16=>'Sports',17=>'Technology',18=>'Website');";

$content = str_replace($old, $new, $content);
file_put_contents($file, $content);
echo "Fixed\n";
exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
