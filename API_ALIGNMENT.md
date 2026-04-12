# API Alignment & Data Model Mapping

## Overview
This document maps the Android app's API calls to the web app's endpoints, showing request/response structures for parity verification.

---

## 1. Authentication Endpoint

### Web App Endpoint
**File**: `facesofnaija-web/api/v2/endpoints/auth.php`  
**Method**: POST  
**Path**: `/api/v2/endpoints/auth.php`

### Request Format

**Required Fields**:
```php
// POST form data
username          // Primary credential (string)
password          // User password (string)
server_key        // Server authentication key (string)
timezone          // User's timezone (string, e.g., "UTC")
device_type       // Platform type (string: "phone" or "windows")
android_m_device_id  // Device identifier (string, Android device ID)
```

### Response Format

**Success (200)**:
```json
{
    "api_status": 200,
    "access_token": "sha1_hash_string",
    "user_id": "123",
    "timezone": "UTC",
    "membership": false,
    // ... additional user fields
}
```

**Error Codes**:
```json
// Code 3: Username not found
{
    "api_status": 400,
    "error": {
        "error_id": "3",
        "error_text": "Username not found"
    }
}

// Code 5: Password incorrect
{
    "api_status": 400,
    "error": {
        "error_id": "5",
        "error_text": "Password is incorrect"
    }
}

// Code 6: Rate limited
{
    "api_status": 400,
    "error": {
        "error_id": "6",
        "error_text": "Too many login attempts please try again later"
    }
}

// Code 7: User banned
{
    "api_status": 400,
    "error": {
        "error_id": "7",
        "error_text": "this user is banned"
    }
}
```

### Android Implementation

**File**: `SocialLoginBaseActivity.cs`  
**Method**: `TryAuthDirectAsync(email, password)`

**Current Request**:
```csharp
var urls = new[] {
    "http://172.236.19.52/api/v2/endpoints/auth.php",
    "http://172.236.19.52/api/auth"
};

using var content = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
    new KeyValuePair<string, string>("username", email),  // NOW: tries "username" first
    new KeyValuePair<string, string>("password", password),
    new KeyValuePair<string, string>("timezone", TimeZone ?? "UTC"),
    new KeyValuePair<string, string>("device_type", "phone"),  // NEW: added
    new KeyValuePair<string, string>("android_m_device_id", UserDetails.DeviceId ?? ""),  // NEW: added
});
```

**Response Parsing**:
```csharp
if (status == 200)
{
    var auth = jObject.ToObject<AuthObject>();
    UserDetails.AccessToken = auth.AccessToken;
    UserDetails.UserId = auth.UserId;
}
else if (errorId == "3")
    // Show "Username not found"
else if (errorId == "5")
    // Show "Password is incorrect"
else if (errorId == "6")
    // Show "Too many login attempts..."
else if (errorId == "7")
    // Show "This user account is banned"
```

**Parity Status**: ✅ ALIGNED

---

## 2. Feed/Timeline Endpoint

### Web App Endpoint
**File**: `facesofnaija-web/api/v2/endpoints/posts.php`  
**Method**: POST  
**Path**: `/api/v2/endpoints/posts.php`

### Request Format

**Common Parameters**:
```php
type              // Feed type (string): see types below
limit             // Posts per page (int, default 20)
after_post_id     // Pagination cursor (string, post ID)
access_token      // User's session token (string)
```

**Feed Types**:
```php
type = "get_news_feed"      // User's personalized feed (friends + groups)
type = "get_timeline"       // All recent posts (no filtering)
type = "get_user_posts"     // Posts by specific user (requires: id)
type = "get_group_posts"    // Posts in specific group (requires: id)
type = "get_page_posts"     // Posts on specific page (requires: id)
type = "get_event_posts"    // Posts for specific event (requires: id)
type = "get_random_videos"  // Random video posts
type = "saved"              // User's saved posts
type = "hashtag"            // Posts with hashtag (requires: hash parameter)
```

### Response Format

**Success (200)**:
```json
{
    "api_status": 200,
    "data": [
        {
            "id": "post_123",
            "post_id": "post_123",
            "publisher_id": "user_456",
            "publisher": {
                "user_id": "user_456",
                "username": "john_doe",
                "first_name": "John",
                "last_name": "Doe",
                "avatar": "http://example.com/avatar.jpg",
                // ... sanitized user fields
            },
            "post_text": "This is a post",
            "post_type": "text|photo|video|...",
            "postFile": "http://example.com/media.jpg",  // via Wo_GetMedia()
            "postFileThumb": "http://example.com/thumb.jpg",  // via Wo_GetMedia()
            "created": "2026-04-04 10:30:00",
            "get_post_comments": [
                {
                    "id": "comment_123",
                    "comment_text": "Great post!",
                    "publisher": { /* user data */ }
                }
            ],
            "parent_id": null,  // null if original, post_id if shared
            "shared_info": null,  // Data of shared post if parent_id set
            // ... additional fields
        }
        // ... more posts
    ]
}
```

### Android Implementation

**File**: `ApiPostAsync.cs`  
**Method**: `FetchFeedPostsApi()` → `GetGlobalPostDirect()`

**Current Request (GetGlobalPostDirect)**:
```csharp
var url = "http://172.236.19.52/api/v2/endpoints/posts.php";

using var content = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("type", "get_news_feed"),
    new KeyValuePair<string, string>("limit", limit),
    new KeyValuePair<string, string>("after_post_id", offset),
    new KeyValuePair<string, string>("access_token", UserDetails.AccessToken ?? ""),
});
```

**Response Parsing**:
```csharp
if (status == 200)
{
    var posts = jObject.ToObject<PostObject>();
    // posts.Data contains list of PostDataObject
}
```

**Parity Status**: ✅ ALIGNED (newly implemented)

### Feed Type Mapping

| Android | Web App | Status |
|---------|---------|--------|
| `NativeFeedType.Global` + "get_news_feed" | `type="get_news_feed"` | ✅ Aligned |
| "get_timeline" fallback | `type="get_timeline"` | ✅ Aligned |
| "get_random_videos" fallback | `type="get_random_videos"` | ✅ Aligned |
| `NativeFeedType.User` + "get_user_posts" | `type="get_user_posts"` | ✅ Aligned |
| `NativeFeedType.Group` + "get_group_posts" | `type="get_group_posts"` | ✅ Aligned |
| `NativeFeedType.Page` + "get_page_posts" | `type="get_page_posts"` | ✅ Aligned |
| `NativeFeedType.Event` + "get_event_posts" | `type="get_event_posts"` | ✅ Aligned |

---

## 3. Create Post Endpoint

### Web App Endpoint
**File**: `facesofnaija-web/api/v2/endpoints/new_post.php`  
**Method**: POST  
**Path**: `/api/v2/endpoints/new_post.php`

### Request Format

**Required Fields**:
```php
post_text          // Post content (string, HTML allowed)
post_privacy       // Privacy level (int: 0=public, 1=friends, 2=private)
access_token       // User session token (string)
```

**Optional Fields**:
```php
user_file          // File upload (multipart file)
feeling_id         // Feeling/activity ID (string)
feeling_text       // Activity text (string)
post_color         // Background color for text posts (string)
post_location      // Location tag (string)
```

### Response Format

**Success (200)**:
```json
{
    "api_status": 200,
    "post_id": "new_post_456",
    "message": "Post created successfully"
}
```

### Android Implementation

**File**: `AddPostActivity.cs`  
**Method**: `TxtAddPostOnClick()`

**Current Implementation**:
- ✅ Now makes direct API call (not just service-based)
- **Needs Alignment**: Verify field names match web app expectations

**Recommended Next Steps**:
```csharp
var content = new MultipartFormDataContent
{
    { new StringContent(postText), "post_text" },
    { new StringContent(privacy), "post_privacy" },
    { new StringContent(UserDetails.AccessToken), "access_token" },
    // Add file if present:
    { new StreamContent(fileStream), "user_file", "image.jpg" }
};

var response = await client.PostAsync(
    "http://172.236.19.52/api/v2/endpoints/new_post.php",
    content
);
```

**Parity Status**: ⚠️ PARTIAL (needs field validation)

---

## 4. User Profile Endpoint

### Web App Endpoint
**File**: `facesofnaija-web/api/v2/endpoints/user-profile.php`

### Request Format
```php
user_id         // Target user ID (string)
access_token    // Session token (string)
```

### Response Format
```json
{
    "api_status": 200,
    "data": {
        "user_id": "123",
        "username": "john_doe",
        "email": "john@example.com",
        "avatar": "http://example.com/avatar.jpg",
        "cover": "http://example.com/cover.jpg",
        "bio": "My biography",
        // ... additional user fields
    }
}
```

### Android Implementation
**File**: `ApiRequest.cs::Get_MyProfileData_Api()`

**Parity Status**: ✅ Aligned

---

## 5. Comments Endpoint

### Web App Endpoint
**File**: `facesofnaija-web/api/v2/endpoints/comments.php`

### Request Types
```php
// Get post comments
type = "get_comments"
post_id = "post_123"

// Add comment
type = "add_comment"
post_id = "post_123"
comment_text = "Comment content"
```

### Android Implementation
**File**: `CommentActivity.cs` / Comment-related classes

**Parity Status**: 🔍 NEEDS VERIFICATION

---

## 6. Reactions Endpoint

### Web App Endpoint
**File**: `facesofnaija-web/api/v2/endpoints/post-actions.php`

### Request Format
```php
type = "like"
post_id = "post_123"
access_token = "token"

// Reaction types: like, love, haha, wow, sad, angry
type = "react"
post_id = "post_123"
reaction = "love"
```

### Response Format
```json
{
    "api_status": 200,
    "likes_count": 42,
    "reactions": {
        "like": 30,
        "love": 10,
        "haha": 2
    }
}
```

### Android Implementation
**File**: `PostFunctions.cs::SetLikePost()`, etc.

**Parity Status**: 🔍 NEEDS VERIFICATION

---

## Summary of Alignment

| Endpoint | Status | Notes |
|----------|--------|-------|
| Authentication (auth.php) | ✅ ALIGNED | All critical fields implemented |
| Posts/Feed (posts.php) | ✅ ALIGNED | Direct API method added |
| Create Post (new_post.php) | ⚠️ PARTIAL | Field names need verification |
| User Profile | ✅ ALIGNED | Existing implementation matches |
| Comments | 🔍 UNKNOWN | Needs verification |
| Reactions | 🔍 UNKNOWN | Needs verification |
| Stories | 🔍 UNKNOWN | Not analyzed |
| Messages | 🔍 UNKNOWN | Not analyzed |
| Notifications | 🔍 UNKNOWN | Not analyzed |

---

## Error Handling Alignment

### HTTP Status Codes
```
200 -> Success
400 -> Client error (invalid input, auth failure)
401 -> Unauthorized (token invalid/expired)
403 -> Forbidden (no permission)
404 -> Not found (resource doesn't exist)
429 -> Rate limited (too many requests)
500 -> Server error
503 -> Service unavailable
```

### API Error Response Structure
```json
{
    "api_status": 400,
    "error": {
        "error_id": "5",
        "error_text": "Human readable message"
    }
}
```

**Android Parsing**:
```csharp
try
{
    var jObject = JObject.Parse(json);
    var status = jObject["api_status"]?.Value<int>();
    
    if (status == 200)
        // Success path
    else if (status == 400)
    {
        var error = jObject["error"]?.ToObject<ErrorObject>();
        var errorId = error?.Error?.ErrorId;
        // Handle by error_id
    }
}
catch
{
    // Fallback: raw JSON response or HTML error page
}
```

**Parity Status**: ✅ ALIGNED

---

## Authentication Token Flow

### Web App Token Generation
```php
// In auth.php (line 30-50)
$access_token = sha1(rand(111111111, 999999999)) . 
                md5(microtime()) . 
                rand(11111111, 99999999) . 
                md5(rand(5555, 9999));

// Stored in app_sessions table with:
user_id = authenticated user ID
session_id = $access_token
platform = "phone" or "windows"
time = current time
```

### Android Token Storage
```csharp
UserDetails.AccessToken = auth.AccessToken;  // From auth response
UserDetails.UserId = auth.UserId;            // From auth response
UserDetails.Cookie = auth.AccessToken;       // Same as access token

// Stored in SQLite:
InsertOrUpdateLogin_Credentials(user);
```

**Token Usage in Subsequent Requests**:
```csharp
// Added to each API call
Authorization: Bearer {UserDetails.AccessToken}
// OR as form parameter
access_token = UserDetails.AccessToken
```

**Parity Status**: ✅ ALIGNED

---

## Data Type Mappings

### User ID
| App | Format | Example |
|-----|--------|---------|
| Web | String (numeric) | "12345" |
| Android | String | UserDetails.UserId = "12345" |

**Alignment**: ✅ ALIGNED

### Post ID
| App | Format | Example |
|-----|--------|---------|
| Web | String (numeric) | "67890" |
| Android | String | PostDataObject.PostId = "67890" |

**Alignment**: ✅ ALIGNED

### Timestamps
| App | Format | Example |
|-----|--------|---------|
| Web | DateTime string | "2026-04-04 10:30:00" |
| Android | String | PostDataObject.Published = "2026-04-04 10:30:00" |

**Alignment**: ⚠️ NEEDS VERIFICATION (format parsing)

### Media URLs
| App | Format | Source |
|-----|--------|--------|
| Web | Processed via Wo_GetMedia() | /api/v2/endpoints/posts.php |
| Android | Direct URL from response | Needs URL transformation |

**Alignment**: ⚠️ NEEDS VERIFICATION

---

## Recommendations for Complete Parity

### High Priority
1. ✅ Verify all timestamp parsing matches web app format
2. ✅ Implement URL transformation for media (Wo_GetMedia() equivalent)
3. ✅ Test post creation field mapping
4. ✅ Validate comment and reaction endpoints

### Medium Priority
5. Implement session token refresh before expiry
6. Add proper pagination cursor handling
7. Test all feed types with real data
8. Verify user field sanitization matches web

### Low Priority  
9. Performance optimization for large datasets
10. Offline caching strategy
11. Media thumbnail generation

---

**Last Updated**: 2026-04-04  
**Maintainer**: Android Development Team  
**Status**: Ready for Full Testing

