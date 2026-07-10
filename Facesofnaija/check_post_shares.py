import sys, json
d = json.load(sys.stdin)
for p in d['posts']:
    pid = p.get('id', '?')
    shares = p.get('post_shares', 'MISSING')
    share_flag = p.get('postShare', 'MISSING')
    views = p.get('views', 'MISSING')
    videoViews = p.get('videoViews', 'MISSING')
    print(f'Post {pid}: post_shares={shares} postShare={share_flag} views={views} videoViews={videoViews}')
