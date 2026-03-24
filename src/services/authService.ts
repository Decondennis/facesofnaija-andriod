import apiClient from '../config/api';
import * as SecureStore from 'expo-secure-store';

export interface LoginResponse {
  api_status: string;
  api_text: string;
  user_id?: string;
  cookie?: string;
  timezone?: string;
  errors?: { error_id: string; error_text: string };
}

export interface RegisterResponse {
  api_status: string;
  api_text: string;
  message?: string;
  success_type?: string;
  session_id?: string;
  user_id?: string;
  errors?: { error_id: string; error_text: string };
}

function generateSessionId(): string {
  return Math.random().toString(36).substring(2, 15) +
    Math.random().toString(36).substring(2, 15);
}

export async function login(username: string, password: string): Promise<LoginResponse> {
  const s = generateSessionId();
  const params = new URLSearchParams();
  params.append('username', username);
  params.append('password', password);
  params.append('s', s);

  const response = await apiClient.post<LoginResponse>(
    '?type=user_login&application=phone',
    params.toString()
  );

  if (response.data.api_status === '200' && response.data.user_id) {
    await SecureStore.setItemAsync('user_id', response.data.user_id);
    await SecureStore.setItemAsync('session_id', s);
    if (response.data.cookie) {
      await SecureStore.setItemAsync('cookie', response.data.cookie);
    }
  }

  return response.data;
}

export async function register(
  username: string,
  email: string,
  password: string,
  confirmPassword: string,
  gender: string = 'male'
): Promise<RegisterResponse> {
  const s = generateSessionId();
  const params = new URLSearchParams();
  params.append('username', username);
  params.append('email', email);
  params.append('password', password);
  params.append('confirm_password', confirmPassword);
  params.append('gender', gender);
  params.append('s', s);

  const response = await apiClient.post<RegisterResponse>(
    '?type=user_registration&application=phone',
    params.toString()
  );

  if (response.data.api_status === '200' && response.data.user_id) {
    await SecureStore.setItemAsync('user_id', response.data.user_id);
    await SecureStore.setItemAsync('session_id', response.data.session_id || s);
  }

  return response.data;
}

export async function logout(userId: string, sessionId: string): Promise<void> {
  try {
    const params = new URLSearchParams();
    params.append('user_id', userId);
    params.append('s', sessionId);
    await apiClient.post('?type=logout&application=phone', params.toString());
  } finally {
    await SecureStore.deleteItemAsync('user_id');
    await SecureStore.deleteItemAsync('session_id');
    await SecureStore.deleteItemAsync('cookie');
  }
}

export async function getStoredCredentials(): Promise<{ userId: string; sessionId: string } | null> {
  const userId = await SecureStore.getItemAsync('user_id');
  const sessionId = await SecureStore.getItemAsync('session_id');
  if (userId && sessionId) {
    return { userId, sessionId };
  }
  return null;
}
