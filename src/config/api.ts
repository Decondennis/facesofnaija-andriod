import axios from 'axios';

// Base URL of the FacesOfNaija web platform
// Change this to your server's URL
export const BASE_URL = 'http://facesofnaija-web.local';
export const API_ENDPOINT = `${BASE_URL}/app_api.php`;

const apiClient = axios.create({
  baseURL: API_ENDPOINT,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/x-www-form-urlencoded',
  },
});

export default apiClient;
