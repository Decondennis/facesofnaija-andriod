import sys, json
d = json.load(sys.stdin)
p = d['posts'][0]
# Print keys containing 'share' or 'view' or 'Share' or 'View' or 'count'
for k in sorted(p.keys()):
    kl = k.lower()
    if 'share' in kl or 'view' in kl or 'count' in kl:
        print(f'{k}: {repr(p[k])}')
print('---')
print(f'post_shares: {repr(p.get("post_shares","MISSING"))}')
print(f'postShare: {repr(p.get("postShare","MISSING"))}')
print(f'videoViews: {repr(p.get("videoViews","MISSING"))}')
print(f'views: {repr(p.get("views","MISSING"))}')
print(f'post_views: {repr(p.get("post_views","MISSING"))}')
