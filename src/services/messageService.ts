import apiClient from '../config/api';

export interface Message {
  id: string;
  from_id: string;
  to_id: string;
  text?: string;
  media?: string;
  time: string;
  time_text?: string;
  seen?: string;
  from?: {
    user_id: string;
    name: string;
    avatar: string;
  };
}

export interface MessagesResponse {
  api_status: string;
  messages?: Message[];
  errors?: { error_id: string; error_text: string };
}

export async function getMessages(
  userId: string,
  sessionId: string,
  toUserId?: string
): Promise<MessagesResponse> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);
  if (toUserId) {
    params.append('to_id', toUserId);
  }

  const response = await apiClient.post<MessagesResponse>(
    '?type=get_user_messages&application=phone',
    params.toString()
  );
  return response.data;
}

export async function sendMessage(
  userId: string,
  sessionId: string,
  toUserId: string,
  text: string
): Promise<{ api_status: string }> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);
  params.append('to_id', toUserId);
  params.append('message_text', text);

  const response = await apiClient.post<{ api_status: string }>(
    '?type=insert_new_message&application=phone',
    params.toString()
  );
  return response.data;
}
