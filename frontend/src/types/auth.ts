export interface User {
  id: string;
  username: string;
  email?: string;
  avatar?: string;
  role: 'admin' | 'user';
}

export interface ConfigProvider {
  id: string;
  name: string;
  description: string;
  icon: string;
  color: string;
  isActive: boolean;
  ispCount: number;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  currentProvider: ConfigProvider | null;
}

export interface LoginCredentials {
  username: string;
  password: string;
}
