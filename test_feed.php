<?php
$ch = curl_init();
curl_setopt_array($ch, [
    CURLOPT_URL => 'http://172.236.19.52/api/phone/get_news_feed.php?type=get_news_feed&access_token=test',
    CURLOPT_POSTFIELDS => 'user_id=1&limit=2&offset=0',
    CURLOPT_RETURNTRANSFER => true,
]);
$r = curl_exec($ch);
$d = json_decode($r);
foreach ($d->posts ?? [] as $p) {
    $rc = isset($p->reaction) ? $p->reaction->count : 0;
    $il = !empty($p->is_liked) ? 1 : 0;
    echo "Post $p->id: like_count=$p->like_count post_likes=$p->post_likes is_liked=$il reaction_count=$rc\n";
}
