<?php
$file = "/var/www/html/api-v2.php";
$content = file_get_contents($file);

// After the access_token resolution block, add default $wo["user"] initialization
$search = '        $wo["user"] = Wo_UserData($user_id);
    }
}';
$replace = '        $wo["user"] = Wo_UserData($user_id);
    }
}
// Always ensure $wo["user"] has a valid structure for endpoints
if (!isset($wo["user"]) || !is_array($wo["user"])) {
    $wo["user"] = array("user_id" => 0);
}';

$content = str_replace($search, $replace, $content);
file_put_contents($file, $content);
echo "Fixed\n";
exec("php -l $file 2>&1", $out);
echo $out[0] . "\n";
