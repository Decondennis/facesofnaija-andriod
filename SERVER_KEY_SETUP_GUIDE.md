=== COMPLETE SOLUTION: Fix Server Key Issue ===

PROBLEM:
--------
The API is returning: "Error: 404 POST (server_key) not specified"
This means the mobile app server key is not configured in your database.

SOLUTION:
---------

STEP 1: Connect to your MySQL database
---------------------------------------
Using phpMyAdmin:
- Open: http://172.236.19.52/phpmyadmin
- Login with your MySQL credentials
- Select your WoWonder database (probably named 'wowonder' or 'facesofnaija' or similar)

STEP 2: Check current server key
---------------------------------
Run this SQL query in the SQL tab:

SELECT name, value FROM wo_config WHERE name = 'widnows_app_api_key';

This will show you if the key is currently set.

STEP 3: Set the server key
---------------------------
Run this SQL to set the mobile app API key:

UPDATE wo_config SET value = 'facesofnaija_mobile_2024' WHERE name = 'widnows_app_api_key';

-- If the above returns 0 rows affected, the column might not exist yet:
INSERT INTO wo_config (name, value) VALUES ('widnows_app_api_key', 'facesofnaija_mobile_2024');

STEP 4: Verify
--------------
Run this again to confirm:

SELECT name, value FROM wo_config WHERE name = 'widnows_app_api_key';

You should see: widnows_app_api_key | facesofnaija_mobile_2024

IMPORTANT NOTES:
----------------
1. The column name is 'widnows_app_api_key' (yes, it's misspelled - it's "widnows" not "windows")
2. This key is ONLY for mobile/desktop apps - it won't affect your web interface
3. The web interface uses session-based authentication, not API keys
4. This is a DIFFERENT key from 'website_api_key' (which is for web API access)

WHY THIS WON'T CONFLICT WITH WEB:
----------------------------------
- Web interface = Session-based login (cookies)
- Mobile API = Requires server_key in every request
- They use different authentication methods
- Different database columns
- Completely separate systems

AFTER SETTING THE KEY:
----------------------
The current app configuration has an encrypted version of a server key.
We need to match it or update the app to use the new key.

Next step: After you set the key in database, let me know and I'll 
update the app's configuration to match it.
