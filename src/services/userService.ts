import apiClient from '../config/api';

export interface UserProfile {
  user_id: string;
  name: string;
  username: string;
  email?: string;
  avatar: string;
  cover?: string;
  about?: string;
  followers_count?: string;
  following_count?: string;
  posts_count?: string;
  is_following?: string;
  url?: string;
}

export interface UserDataResponse {
  api_status: string;
  userData?: UserProfile;
  errors?: { error_id: string; error_text: string };
}

export interface UsersListResponse {
  api_status: string;
  users?: UserProfile[];
  errors?: { error_id: string; error_text: string };
}

export async function getUserData(
  userId: string,
  sessionId: string,
  profileId?: string
): Promise<UserDataResponse> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);
  params.append('user_profile_id', profileId || userId);

  const response = await apiClient.post<UserDataResponse>(
    '?type=get_user_data&application=phone',
    params.toString()
  );
  return response.data;
}

export async function getUsersList(
  userId: string,
  sessionId: string,
  limit: number = 20
): Promise<UsersListResponse> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);
  params.append('limit', String(limit));

  const response = await apiClient.post<UsersListResponse>(
    '?type=get_users_list&application=phone',
    params.toString()
  );
  return response.data;
}

export async function followUser(
  userId: string,
  sessionId: string,
  followId: string
): Promise<{ api_status: string }> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);
  params.append('follow_id', followId);

  const response = await apiClient.post<{ api_status: string }>(
    '?type=follow_user&application=phone',
    params.toString()
  );
  return response.data;
}

export async function searchUsers(
  userId: string,
  sessionId: string,
  query: string
): Promise<UsersListResponse> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);
  params.append('search_key', query);

  const response = await apiClient.post<UsersListResponse>(
    '?type=search_public_users&application=phone',
    params.toString()
  );
  return response.data;
}
