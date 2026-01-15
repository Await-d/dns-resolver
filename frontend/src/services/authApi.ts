const API_BASE = '/api/auth';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  expiresAt: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface CurrentUserResponse {
  userId: string;
  username: string;
  role: string;
}

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('dns_token');
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

export const authApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await fetch(`${API_BASE}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: '登录失败' }));
      throw new Error(error.message || '登录失败');
    }
    return response.json();
  },

  changePassword: async (data: ChangePasswordRequest): Promise<void> => {
    const response = await fetch(`${API_BASE}/change-password`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: '修改密码失败' }));
      throw new Error(error.message || '修改密码失败');
    }
  },

  getCurrentUser: async (): Promise<CurrentUserResponse> => {
    const response = await fetch(`${API_BASE}/me`, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) {
      throw new Error('获取用户信息失败');
    }
    return response.json();
  },
};
