import apiClient from '../config/api';

export interface Notification {
  id: string;
  user_id: string;
  notifier_id: string;
  type: string;
  time: string;
  time_text?: string;
  is_seen?: string;
  notifier?: {
    user_id: string;
    name: string;
    avatar: string;
  };
  text?: string;
  url?: string;
}

export interface NotificationsResponse {
  api_status: string;
  notifications?: Notification[];
  count?: string;
  errors?: { error_id: string; error_text: string };
}

export async function getNotifications(
  userId: string,
  sessionId: string
): Promise<NotificationsResponse> {
  const params = new URLSearchParams();
  params.append('user_id', userId);
  params.append('s', sessionId);

  const response = await apiClient.post<NotificationsResponse>(
    '?type=get_notifications&application=phone',
    params.toString()
  );
  return response.data;
}
