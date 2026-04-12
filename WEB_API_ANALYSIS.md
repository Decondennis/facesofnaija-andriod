# Facesofnaija Web API Analysis

## API Configuration

### Base URL
**Web App:** `http://localhost/facesofnaija-web` (configurable in config.php)
**Android App:** Should connect to deployed server URL

### Authentication
**Method:** Server Key + Session-based authentication
**Endpoint:** `app_api.php`
**Required Parameters:**
- `server_key`: API key configured in admin panel
- `application`: "windows_app" (for Android)
- `type`: API action type

### API Version
Current: `1.5.2`

---

## Available API Endpoints

### Authentication & User Management

#### 1. User Login
- **Type:** `user_login`
- **File:** `api/windows_app/login.php`
- **Method:** POST
- **Parameters:**
  - `server_key` (required)
  - `username` or `email`
  - `password`
- **Response:** User data + access_token

#### 2. Register User
- **Type:** `register_user`
- **File:** `api/windows_app/register_user.php`
- **Method:** POST
- **Parameters:**
  - `server_key` (required)
  - `username`
  - `email`
  - `password`
  - Additional profile data

#### 3. Social Login
- **Type:** `social-login`
- **File:** `api/windows_app/social-login.php`
- **Method:** POST
- **Support:** Facebook, Google login

#### 4. Logout
- **Type:** `logout`
- **File:** `api/windows_app/logout.php`
- **Method:** POST

#### 5. Two-Factor Authentication
- **Type:** `two-factor`
- **File:** `api/windows_app/two-factor.php`
- **Method:** POST

### User Data & Profile

#### 6. Get User Data
- **Type:** `get_user_data`
- **File:** `api/windows_app/get_user_data.php`
- **Method:** POST
- **Parameters:**
  - `user_id` (required)
- **Response:** Complete user profile data

#### 7. Get Multi Users
- **Type:** `get_multi_users`
- **File:** `api/windows_app/get_multi_users.php`
- **Method:** POST
- **Parameters:**
  - `user_id` (required)
  - `user_ids` (comma-separated)
- **Response:** Array of user data

#### 8. Get Users List
- **Type:** `get_users_list`
- **File:** `api/windows_app/get_users_list.php`
- **Method:** POST
- **Purpose:** Get list of users (search, discover)

#### 9. Update User Data
- **Type:** `update_user_data`
- **File:** `api/windows_app/update_user_data.php`
- **Method:** POST
- **Parameters:** User profile fields to update

#### 10. Update Profile Picture
- **Type:** `update_profile_picture`
- **File:** `api/windows_app/update_profile_picture.php`
- **Method:** POST
- **Parameters:** Image file

#### 11. Update User Last Seen
- **Type:** `update_user_lastseen` or `user_lastseen`
- **File:** `api/windows_app/update_user_lastseen.php`
- **Method:** POST
- **Purpose:** Update online status

#### 12. Search Public Users
- **Type:** `search_public_users`
- **File:** `api/windows_app/search_public_users.php`
- **Method:** POST
- **Parameters:**
  - `search_query`

#### 13. Get User Friends
- **Type:** `get_user_friends`
- **File:** `api/windows_app/get_user_friends.php`
- **Method:** POST

### Posts & Content

#### 14. Get User Posts
- **Type:** `get_user_posts`
- **File:** `api/windows_app/get_user_posts_friends.php`
- **Method:** POST
- **Parameters:**
  - `user_id` (required)
  - `user_profile_id` (required)
  - `filter_by` (optional: all, photos, videos, etc.)
  - `limit` (default: 3)
- **Response:** Array of PostDataObject

**Post Data Structure (from analysis):**
```json
{
  "post_id": "string",
  "user_id": "string",
  "postText": "string",
  "postFile": "string",  // Video/Image URL
  "postFileFull": "string",  // Full resolution URL
  "postRecord": "string",  // Audio file URL
  "postType": "string",  // text, image, video, audio, etc.
  "postTime": "timestamp",
  "postLikes": "count",
  "postComments": "count",
  "postShares": "count",
  "postViews": "count",
  // ... additional fields
}
```

### Messaging

#### 15. Get User Messages
- **Type:** `get_user_messages`
- **File:** `api/windows_app/get_user_messages.php`
- **Method:** POST
- **Parameters:**
  - `user_id` (required)
  - `recipient_id` (required)

#### 16. Insert New Message / New Message
- **Type:** `insert_new_message` or `new_message`
- **File:** `api/windows_app/insert_new_message.php`
- **Method:** POST
- **Parameters:**
  - `user_id` (required)
  - `recipient_id` (required)
  - `text` or `file` attachment

#### 17. Delete Messages
- **Type:** `delete_messages`
- **File:** `api/windows_app/delete_messages.php`
- **Method:** POST

#### 18. Register Typing
- **Type:** `register_typing`
- **File:** `api/windows_app/register_typing.php`
- **Method:** POST
- **Purpose:** Show "typing..." indicator

#### 19. Remove Typing
- **Type:** `remove_typing`
- **File:** `api/windows_app/remove_typing.php`
- **Method:** POST

### Video Calling

#### 20. Create Video Call
- **Type:** `create_video_call`
- **File:** `api/windows_app/create_video_call.php`
- **Method:** POST

#### 21. Video Call Answer
- **Type:** `video_call_answer`
- **File:** `api/windows_app/video_call_answer.php`
- **Method:** POST

#### 22. Check for Answer
- **Type:** `check_for_answer`
- **File:** `api/windows_app/check_for_answer.php`
- **Method:** POST

### Social Actions

#### 23. Follow User
- **Type:** `follow_user`
- **File:** `api/windows_app/follow_user.php`
- **Method:** POST

#### 24. Block User
- **Type:** `block_user`
- **File:** `api/windows_app/block_user.php`
- **Method:** POST

### Settings & Configuration

#### 25. Get Settings
- **Type:** `get_settings`
- **File:** `api/windows_app/get_settings.php`
- **Method:** POST
- **Purpose:** Get app configuration

#### 26. Change Color
- **Type:** `change_color`
- **File:** `api/windows_app/change_color.php`
- **Method:** POST
- **Purpose:** Update theme color preference

#### 27. Active Account SMS
- **Type:** `active_account_sms`
- **File:** `api/windows_app/active_account_sms.php`
- **Method:** POST
- **Purpose:** SMS verification

#### 28. Check Hash
- **Type:** `check-hash`
- **File:** `api/windows_app/check-hash.php`
- **Method:** POST

---

## Android App Current Configuration

### From AppSettings.cs Analysis:

**App Name:** Facesofnaija
**Version:** 2.2
**Database:** Facesofnaija (local SQLite)
**Connectivity System:** 0 (Friend system)
**App Mode:** Default (Facebook-like)
**Main Color:** #82b53f

**API Integration:**
- Uses WoWonderClient library for API calls
- CustomApiModel.cs provides custom API extensions
- Triple DES encryption for secure data

---

## Required Android Updates for Web Alignment

### 1. API Base URL Configuration
**Current:** Likely pointing to demo/staging server
**Required:** Update to production facesofnaija-web URL

**File to Update:** `Facesofnaija/Helpers/Utils/InitializeWoWonder.cs` or similar
```csharp
public static string WebsiteUrl = "https://facesofnaija.com"; // Update this
public static string ServerKey = "YOUR_SERVER_KEY_HERE"; // From admin panel
```

### 2. Post Data Model Synchronization

**Required Fields Comparison:**

**Web API Post Model:**
- post_id
- user_id
- postText
- postFile (video/image URL)
- postFileFull (full resolution)
- postRecord (audio URL)
- postType (text/image/video/audio/shared/poll)
- postTime
- postLikes
- postComments
- postShares
- postViews
- postSticker
- album_name
- multi_image (JSON array)
- multi_image_post (boolean)
- recipient_id (for private posts)
- page_id / group_id (for community posts)

**Android Model:** Check `WoWonderClient.Classes.Posts.PostDataObject`
- Verify all fields match
- Update serialization attributes
- Add missing fields if any

### 3. Media URL Handling

**Video URLs:**
```
postFile: Server path (e.g., "/upload/videos/post_123.mp4")
postFileFull: Full URL (e.g., "https://facesofnaija.com/upload/videos/post_123.mp4")
```

**Audio URLs:**
```
postRecord: Server path or full URL to audio file
```

**Image URLs:**
```
postFile: Thumbnail
postFileFull: Full resolution
multi_image: JSON array of image URLs
```

**Android Implementation:**
- VideoPlayerController should handle both relative and absolute URLs
- Implement URL normalization (prepend base URL if relative)
- Support HTTPS streaming

### 4. API Request Format

**Standard Request:**
```http
POST /app_api.php?application=windows_app&type=get_user_posts HTTP/1.1
Content-Type: application/x-www-form-urlencoded

server_key=YOUR_KEY&user_id=123&user_profile_id=456&limit=20
```

**Response Format:**
```json
{
  "api_status": "200",
  "api_text": "success",
  "api_version": "1.5.2",
  "data": {
    "posts": [...],
    "continue": true
  }
}
```

**Error Format:**
```json
{
  "api_status": "400",
  "api_text": "failed",
  "api_version": "1.5.2",
  "errors": {
    "error_id": "3",
    "error_text": "Error message"
  }
}
```

### 5. Authentication Flow

**Login Process:**
1. Call `user_login` endpoint
2. Receive `access_token` and `user_id`
3. Store in SharedPreferences/SecureStorage
4. Include in all subsequent requests

**Session Management:**
- Call `update_user_lastseen` periodically (every 30 seconds)
- Handle token expiration (401 errors)
- Implement automatic re-login

### 6. Video/Audio Playback

**Video Formats Supported:** MP4, WebM, MOV
**Audio Formats Supported:** MP3, AAC, WAV

**Streaming:**
- Use MediaPlayer with HTTP/HTTPS URLs
- Implement buffering indicators
- Handle network interruptions
- Cache for offline playback (optional)

### 7. Missing Endpoints in Current App

Based on web API analysis, check if Android app has:
- ✅ User login/registration
- ✅ Get posts
- ✅ Messaging
- ✅ User profile
- ❓ Video calling (check implementation)
- ❓ Stories (check implementation)
- ❓ Live streaming (check implementation)
- ❓ Marketplace (check implementation)
- ❓ Jobs (check implementation)
- ❓ Events (check implementation)

---

## Testing Plan

### 1. Setup Local Web Server
```bash
# Option 1: Using XAMPP/WAMP
1. Install XAMPP
2. Copy facesofnaija-web to htdocs/
3. Create database: facesofnaija
4. Import database_init.sql
5. Configure config.php
6. Access: http://localhost/facesofnaija-web

# Option 2: Using PHP built-in server
cd facesofnaija-web
php -S localhost:8080
```

### 2. Configure Android App
```csharp
// Update in InitializeWoWonder.cs or AppSettings.cs
WebsiteUrl = "http://10.0.2.2:8080"; // For emulator (localhost)
// Or
WebsiteUrl = "http://YOUR_LOCAL_IP:8080"; // For physical device
ServerKey = "YOUR_SERVER_KEY"; // From admin panel
```

### 3. Test API Calls
1. **Login Test:**
   - Create test account in web app
   - Login from Android app
   - Verify token received

2. **Post Loading Test:**
   - Create posts with video/audio in web app
   - Load posts in Android app
   - Verify media URLs resolve correctly

3. **Video Playback Test:**
   - Click video post
   - Verify VideoPlayerController loads URL
   - Test play/pause/seek

4. **Audio Playback Test:**
   - Click audio post
   - Verify AudioPlayerController loads URL
   - Test play/pause/skip

### 4. Network Testing
- Test on WiFi
- Test on mobile data
- Test with slow connection
- Test with interrupted connection

---

## Implementation Priority

### Phase 1: Core API Integration (Week 1)
1. ✅ Update base URL configuration
2. ✅ Verify PostDataObject model matches web API
3. ✅ Test login/authentication
4. ✅ Test post loading

### Phase 2: Media Playback (Week 2)
1. ✅ Test video URL loading
2. ✅ Test audio URL loading
3. ✅ Implement URL normalization
4. ✅ Add buffering/loading states

### Phase 3: Feature Completeness (Week 3)
1. ❓ Verify all API endpoints implemented
2. ❓ Add missing features
3. ❓ Update UI to match web app

### Phase 4: Polish & Testing (Week 4)
1. ❓ End-to-end testing
2. ❓ Performance optimization
3. ❓ Bug fixes
4. ❓ Production deployment

---

## Next Steps

1. **Immediate:**
   - Configure Android app with local web server URL
   - Test login/authentication
   - Test post loading with media

2. **Short-term:**
   - Create video layout XML files
   - Test MediaPlayer with actual URLs
   - Verify data model compatibility

3. **Medium-term:**
   - Implement missing features
   - Sync UI/UX with web app
   - Comprehensive testing

---

*Analysis completed: January 2025*
*Web App: WoWonder-based (PHP/MySQL)*
*Android App: .NET 9 + Xamarin.Android*
