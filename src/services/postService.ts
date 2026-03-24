import apiClient from '../config/api';

export interface Post {
  id: string;
  user_id: string;
  // Note: 'Orginaltext' is the exact field name returned by the WoWonder API (intentional spelling)
  Orginaltext?: string;
  postFile?: string;
  time: string;
  time_text?: string;
  likes_count?: string;
  comment_count?: string;
  is_liked?: string;
  publisher?: {
    user_id: string;
    name: string;
    username: string;
    avatar: string;
  };
}

export interface PostsResponse {
  api_status: string;
  posts?: Post[];
  errors?: { error_id: string; error_text: string };
}

export interface NewPostResponse {
  api_status: string;
  api_text?: string;
  errors?: { error_id: string; error_text: string };
}

export async function getFeedPosts(
  userId: string,
  sessionId: string,
  limit: number = 10
): Promise<PostsResponse> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('user_profile_id', userId);
  params.append('s', sessionId);
  params.append('limit', String(limit));

  const response = await apiClient.post<PostsResponse>(
    '?type=get_user_posts&application=phone',
    params.toString()
  );
  return response.data;
}

export async function createPost(
  userId: string,
  sessionId: string,
  text: string
): Promise<NewPostResponse> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);
  params.append('postText', text);

  const response = await apiClient.post<NewPostResponse>(
    '?type=new_post&application=phone',
    params.toString()
  );
  return response.data;
}
