<?php
$logfile = '/tmp/app_api_debug.log';
$logdata = date('Y-m-d H:i:s') . ' | METHOD=' . ($_SERVER['REQUEST_METHOD'] ?? 'NA') . ' | GET=' . json_encode($_GET) . ' | POST=' . json_encode($_POST) . "\n";
file_put_contents($logfile, $logdata, FILE_APPEND);

require_once('assets/init.php');
$api_version  = '1.5.2';
$type         = '';
$applications = array(
    'phone',
    'windows_app'
);
if (!empty($_GET['report_errors'])) {
    ini_set('display_errors', 1);
    ini_set('display_startup_errors', 1);
    error_reporting(E_ALL);
}
$application = 'windows_app';
if (!empty($_GET['application'])) {
    if (in_array($_GET['application'], $applications)) {
        $application = Wo_Secure($_GET['application']);
    }
}
if (!empty($_GET['type'])) {
    $type = Wo_Secure($_GET['type']);
}

$logdata = date('Y-m-d H:i:s') . ' | application=' . $application . ' | type=' . $type . "\n";
file_put_contents($logfile, $logdata, FILE_APPEND);

if ($application == 'windows_app') {
    $logdata = date('Y-m-d H:i:s') . ' | BRANCH: windows_app | type=' . $type . "\n";
    file_put_contents($logfile, $logdata, FILE_APPEND);
    switch ($type) {
        case 'get_news_feed':
        case 'get_news_list':
            $logdata = date('Y-m-d H:i:s') . ' | WOULD_INCLUDE: api/windows_app/get_news_feed.php (does not exist!)\n';
            file_put_contents($logfile, $logdata, FILE_APPEND);
            include "api/$application/get_news_feed.php";
            break;
    }
} else if ($application == 'phone') {
    $logdata = date('Y-m-d H:i:s') . ' | BRANCH: phone\n';
    file_put_contents($logfile, $logdata, FILE_APPEND);
    switch ($type) {
        case 'get_news_feed':
        case 'get_news_list':
            $logdata = date('Y-m-d H:i:s') . ' | INCLUDING: api/phone/get_news_feed.php\n';
            file_put_contents($logfile, $logdata, FILE_APPEND);
            include "api/$application/get_news_feed.php";
            break;
    }
}
