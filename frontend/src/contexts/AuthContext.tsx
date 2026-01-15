import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { User, ConfigProvider, AuthState, LoginCredentials } from '../types/auth';
import type { IspInfo } from '../types/dns';
import { authApi } from '../services/authApi';
import { fetchIsps } from '../services/dnsApi';

// ISP 图标和颜色映射
const ISP_STYLES: Record<string, { icon: string; color: string }> = {
  alidns: { icon: '☁️', color: 'var(--neon-orange)' },
  aliesa: { icon: '🌐', color: '#ff6a00' },
  baiducloud: { icon: '🔍', color: '#2932e1' },
  callback: { icon: '🔗', color: '#9d4edd' },
  cloudflare: { icon: '🛡️', color: '#f38020' },
  dnsla: { icon: '🌍', color: '#00a4ff' },
  dnspod: { icon: '🐧', color: '#00d4ff' },
  dynadot: { icon: '🔷', color: '#0066cc' },
  dynv6: { icon: '6️⃣', color: '#00cc66' },
  edgeone: { icon: '⚡', color: '#006eff' },
  eranet: { icon: '🌐', color: '#ff3366' },
  gcore: { icon: '🚀', color: '#ff6600' },
  godaddy: { icon: '🏠', color: '#1bdbdb' },
  huaweicloud: { icon: '🔴', color: '#e60012' },
  namecheap: { icon: '💰', color: '#de5833' },
  namesilo: { icon: '🏷️', color: '#0099cc' },
  nowcn: { icon: '🇨🇳', color: '#ff0000' },
  nsone: { icon: '1️⃣', color: '#7b68ee' },
  porkbun: { icon: '🐷', color: '#f472b6' },
  spaceship: { icon: '🚀', color: '#6366f1' },
  tencentcloud: { icon: '🐧', color: '#00a4ff' },
  trafficroute: { icon: '🛣️', color: '#22c55e' },
  vercel: { icon: '▲', color: '#000000' },
};

const DEFAULT_STYLE = { icon: '🌐', color: 'var(--neon-cyan)' };

function ispToProvider(isp: IspInfo): ConfigProvider {
  const style = ISP_STYLES[isp.id] || DEFAULT_STYLE;
  return {
    id: isp.id,
    name: isp.displayName || isp.name,
    description: isp.name,
    icon: style.icon,
    color: style.color,
    isActive: true,
    ispCount: 1,
  };
}

interface AuthContextType extends AuthState {
  login: (credentials: LoginCredentials) => Promise<boolean>;
  logout: () => void;
  providers: ConfigProvider[];
  switchProvider: (providerId: string) => void;
  changePassword: (currentPassword: string, newPassword: string) => Promise<void>;
  loginError: string | null;
  isLoadingProviders: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('dns_user');
    return saved ? JSON.parse(saved) : null;
  });

  const [loginError, setLoginError] = useState<string | null>(null);
  const [providers, setProviders] = useState<ConfigProvider[]>([]);
  const [isLoadingProviders, setIsLoadingProviders] = useState(true);

  const [currentProvider, setCurrentProvider] = useState<ConfigProvider | null>(null);

  // 从后端加载 ISP 列表并转换为 ConfigProvider
  useEffect(() => {
    const loadProviders = async () => {
      try {
        const isps = await fetchIsps();
        const loadedProviders = isps.map(ispToProvider);
        setProviders(loadedProviders);

        // 恢复之前选择的配置商，或选择第一个
        const savedId = localStorage.getItem('dns_provider');
        const savedProvider = loadedProviders.find(p => p.id === savedId);
        setCurrentProvider(savedProvider || loadedProviders[0] || null);
      } catch (error) {
        console.error('Failed to load ISP providers:', error);
        setProviders([]);
      } finally {
        setIsLoadingProviders(false);
      }
    };

    loadProviders();
  }, []);

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
        isLoadingProviders,
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
