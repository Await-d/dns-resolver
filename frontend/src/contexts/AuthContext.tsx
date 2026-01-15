import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { User, ConfigProvider, AuthState, LoginCredentials } from '../types/auth';
import { authApi } from '../services/authApi';

// Mock config providers
const MOCK_PROVIDERS: ConfigProvider[] = [
  {
    id: 'china-telecom',
    name: '中国电信',
    description: '电信骨干网DNS服务器配置',
    icon: '📡',
    color: 'var(--neon-cyan)',
    isActive: true,
    ispCount: 8
  },
  {
    id: 'china-unicom',
    name: '中国联通',
    description: '联通全国DNS节点配置',
    icon: '🌐',
    color: 'var(--neon-green)',
    isActive: true,
    ispCount: 6
  },
  {
    id: 'china-mobile',
    name: '中国移动',
    description: '移动DNS服务配置',
    icon: '📶',
    color: 'var(--neon-magenta)',
    isActive: true,
    ispCount: 5
  },
  {
    id: 'public-dns',
    name: '公共DNS',
    description: '国内外公共DNS服务',
    icon: '🔓',
    color: 'var(--neon-orange)',
    isActive: true,
    ispCount: 12
  },
  {
    id: 'enterprise',
    name: '企业专线',
    description: '企业级DNS解析服务',
    icon: '🏢',
    color: '#9d4edd',
    isActive: false,
    ispCount: 4
  }
];

interface AuthContextType extends AuthState {
  login: (credentials: LoginCredentials) => Promise<boolean>;
  logout: () => void;
  providers: ConfigProvider[];
  switchProvider: (providerId: string) => void;
  changePassword: (currentPassword: string, newPassword: string) => Promise<void>;
  loginError: string | null;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('dns_user');
    return saved ? JSON.parse(saved) : null;
  });

  const [loginError, setLoginError] = useState<string | null>(null);

  const [currentProvider, setCurrentProvider] = useState<ConfigProvider | null>(() => {
    const savedId = localStorage.getItem('dns_provider');
    return MOCK_PROVIDERS.find(p => p.id === savedId) || MOCK_PROVIDERS[0];
  });

  const [providers] = useState<ConfigProvider[]>(MOCK_PROVIDERS);

  useEffect(() => {
    if (user) {
      localStorage.setItem('dns_user', JSON.stringify(user));
    } else {
      localStorage.removeItem('dns_user');
    }
  }, [user]);

  useEffect(() => {
    if (currentProvider) {
      localStorage.setItem('dns_provider', currentProvider.id);
    }
  }, [currentProvider]);

  const login = async (credentials: LoginCredentials): Promise<boolean> => {
    setLoginError(null);
    try {
      const response = await authApi.login({
        username: credentials.username,
        password: credentials.password,
      });

      localStorage.setItem('dns_token', response.token);

      const loggedInUser: User = {
        id: response.username,
        username: response.username,
        email: `${response.username}@local`,
        role: response.role as 'admin' | 'user',
      };
      setUser(loggedInUser);
      return true;
    } catch (error) {
      const message = error instanceof Error ? error.message : '登录失败';
      setLoginError(message);
      return false;
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('dns_user');
    localStorage.removeItem('dns_token');
  };

  const changePassword = async (currentPassword: string, newPassword: string): Promise<void> => {
    await authApi.changePassword({ currentPassword, newPassword });
  };

  const switchProvider = (providerId: string) => {
    const provider = providers.find(p => p.id === providerId);
    if (provider && provider.isActive) {
      setCurrentProvider(provider);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        currentProvider,
        providers,
        login,
        logout,
        switchProvider,
        changePassword,
        loginError,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
