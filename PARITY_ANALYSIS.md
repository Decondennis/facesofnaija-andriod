# Facesofnaija Android-Web Parity Analysis

## Executive Summary

The Android app (`facesofnaija-android`) has significant structural and functional gaps compared to the web repository (`facesofnaija-web`). While core functionality exists, there are critical misalignments in:

1. **API Endpoint Consistency** - Multiple endpoint variants and fallback strategies indicate uncertainty
2. **Authentication Flow** - Dual endpoints and credential key variations suggest API evolution without full client support
3. **Feed/Timeline Loading** - Incomplete feed fetching with multiple fallback strategies
4. **Data Model Alignment** - Serialization/deserialization gaps
5. **Error Handling** - Inconsistent error codes and messages across endpoints
6. **Network Resilience** - Poor handling of network transitions and server unavailability

---

## Detailed Parity Gaps

### 1. API Authentication (Critical)

**Web App (PHP)**: `api/v2/endpoints/auth.php`
- **Expected Request**: POST with `username`, `password`, `timezone`, `device_type`, `android_m_device_id`
- **Response Fields**: `api_status`, `access_token`, `user_id`, `timezone`, `membership`
- **Error Codes**: 3 (username not found), 5 (password incorrect), 6 (too many attempts), 7 (banned)
- **Flow**: Direct login → Session creation → Device registration

**Android App**: `SocialLoginBaseActivity.cs`
- **Current Implementation**:
  - Tries 2 endpoints: `http://172.236.19.52/api/auth` and `http://172.236.19.52/api-v2.php?type=auth`
  - Tries 2 credential keys: `username` and `email` (should be `username` only based on web)
  - Inconsistent error code mapping
  - No device_type field sent to server
  
**Gap**: Android sends `email` as credential key; web expects `username`. Device registration incomplete.

---

### 2. Feed/Timeline API

**Web App**: `api/v2/endpoints/posts.php`
- **Types Supported**:
  - `get_news_feed` - User's news feed (friend posts + community posts)
  - `get_timeline` - All recent posts (no filtering)
  - `get_user_posts` - Specific user's posts
  - `get_group_posts` - Specific group's posts
  - `get_random_videos` - Random video feed
  
- **Request Parameters**: `limit`, `id`, `after_post_id`, `filter`, `placement`
- **Response**: `api_status`, `data` (array of posts)
- **Media Fields**: `postFile`, `postFileThumb` (returned via `Wo_GetMedia()`)

**Android App**: `ApiPostAsync.cs`
- **Current Implementation**:
  - Uses `GetGlobalPost` with type parameter
  - Fallback: On failure, tries `get_timeline`, then `get_random_videos`
  - No consistent post model alignment
  - Media handling differs from web response

**Gap**: Fallback strategy suggests endpoint uncertainty. Post data model incomplete.

---

### 3. Post Creation

**Web App**: `api/v2/endpoints/new_post.php`
- **Required Fields**: `postText`, `user_file`, `type`, `privacy`, and various optional fields
- **Response**: Success with post_id, or error response

**Android App**: `AddPostActivity.cs`
- **Current Implementation**: 
  - Direct API call added (improvement)
  - Service-based fallback still present
  - Serialization may not match web expectations

**Gap**: Post model serialization alignment needed.

---

### 4. Data Models & Serialization

**Key Issues**:
1. **User Model**: Web sends via `Wo_GetUser()` with sanitized fields; Android expects raw model
2. **Post Model**: 
   - Web: `postFile` (media URL), `postFileThumb` (thumbnail URL)
   - Android: Likely expects different field names
3. **Media**: Web returns processed URLs; Android might expect raw paths
4. **Timestamps**: Format consistency (unix vs. ISO 8601)

---

### 5. Session & Device Management

**Web App**: Stores `access_token`, `user_id`, `device_type`, `platform` in sessions table
**Android App**: Stores `AccessToken`, `UserId`, `DeviceId` in local database

**Gap**: Device type not properly registered with server.

---

### 6. Error Handling

**Web App Error Codes**:
- 3: Username not found
- 5: Password incorrect
- 6: Too many login attempts
- 7: User banned
- 200: Success

**Android App**:
- Maps error codes but doesn't validate against all web codes
- Generic error handling for unknown codes

---

## Priority Fix List

### **Tier 1 (Critical - Blocking Core Features)**
1. **Fix Authentication Credential Key** - Use `username` instead of `email` in auth request
2. **Standardize Feed API Endpoints** - Remove uncertainty; use web's endpoints directly
3. **Align Post Data Models** - Ensure post fields match web API response
4. **Device Registration** - Send `device_type` field in auth request

### **Tier 2 (High - Improves Reliability)**
5. **Improve Error Code Mapping** - Handle all web-defined error codes
6. **Network Resilience** - Proper timeout/retry logic
7. **Session Management** - Align token storage and refresh

### **Tier 3 (Medium - Polish)**
8. **UI/UX Consistency** - Match loading/empty/error states with web
9. **Media Caching** - Implement proper offline/cache handling
10. **Input Validation** - Align with web-side validation rules

---

## Implementation Strategy

1. **Phase 1**: Fix critical auth issues (credential key, device registration)
2. **Phase 2**: Align feed endpoints and post data models
3. **Phase 3**: Implement consistent error handling
4. **Phase 4**: Add tests for parity verification

---

## Files Requiring Changes

### Critical
- `SocialLoginBaseActivity.cs` - Auth implementation
- `ApiPostAsync.cs` - Feed API endpoints
- `AddPostActivity.cs` - Post creation

### Important
- `AppSettings.cs` - API configuration
- `LoginActivity.cs` - Input handling
- Data model classes in `WoWonderClient`

### Reference
- `.github/copilot-instructions.md` - Server host configuration

