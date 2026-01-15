import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../contexts/AuthContext';
import LanguageSwitcher from './LanguageSwitcher';

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

export default function Sidebar({ collapsed, onToggle }: SidebarProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user, logout, providers, currentProvider, switchProvider } = useAuth();
  const [showUserMenu, setShowUserMenu] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <aside
      className={`fixed left-0 top-0 h-full bg-[var(--bg-secondary)] border-r border-[var(--border-color)] z-50 transition-all duration-300 flex flex-col ${
        collapsed ? 'w-16' : 'w-64'
      }`}
    >
      <div className="p-4 border-b border-[var(--border-color)]">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 border-2 border-[var(--neon-cyan)] flex items-center justify-center relative flex-shrink-0">
            <span className="text-[var(--neon-cyan)] text-xl font-bold display">D</span>
            <div className="absolute -top-1 -right-1 w-2 h-2 bg-[var(--neon-cyan)]"></div>
          </div>
          {!collapsed && (
            <div className="overflow-hidden">
              <h1 className="display text-sm font-bold tracking-wider text-[var(--neon-cyan)] truncate">
                DNS RESOLVER
              </h1>
              <p className="text-[10px] text-[var(--text-muted)] truncate">
                {t('app.version')}
              </p>
            </div>
          )}
        </div>
      </div>

      <button
        onClick={onToggle}
        className="absolute -right-3 top-20 w-6 h-6 bg-[var(--bg-secondary)] border border-[var(--border-color)] flex items-center justify-center text-[var(--text-muted)] hover:text-[var(--neon-cyan)] hover:border-[var(--neon-cyan)] transition-all"
      >
        <svg
          className={`w-3 h-3 transition-transform ${collapsed ? '' : 'rotate-180'}`}
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
        </svg>
      </button>

      <div className="flex-1 overflow-y-auto p-3">
        {!collapsed && (
          <div className="mb-3">
            <span className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider">
              {t('provider.title')}
            </span>
          </div>
        )}

        <div className="space-y-2">
          {providers.map((provider) => {
            const isActive = currentProvider?.id === provider.id;
            const isAvailable = provider.isActive;

            return (
              <button
                key={provider.id}
                onClick={() => isAvailable && switchProvider(provider.id)}
                disabled={!isAvailable}
                title={collapsed ? provider.name : undefined}
                className={`w-full flex items-center gap-3 p-2 transition-all ${
                  collapsed ? 'justify-center' : ''
                } ${
                  isActive
                    ? 'bg-[var(--neon-cyan-dim)] border-l-2 border-[var(--neon-cyan)]'
                    : isAvailable
                    ? 'hover:bg-[var(--bg-tertiary)] border-l-2 border-transparent'
                    : 'opacity-40 cursor-not-allowed border-l-2 border-transparent'
                }`}
              >
                <span className="text-xl flex-shrink-0">{provider.icon}</span>
                {!collapsed && (
                  <div className="flex-1 text-left min-w-0 flex items-center gap-2">
                    <div className="flex-1 min-w-0">
                      <div className={`text-sm truncate ${isActive ? 'text-[var(--neon-cyan)]' : 'text-[var(--text-primary)]'}`}>
                        {provider.name}
                      </div>
                      <div className="text-[10px] text-[var(--text-muted)]">
                        {provider.ispCount} DNS
                      </div>
                    </div>
                    {isActive && (
                      <div className="w-2 h-2 rounded-full bg-[var(--neon-cyan)] shadow-[0_0_8px_var(--neon-cyan)] flex-shrink-0"></div>
                    )}
                  </div>
                )}
              </button>
            );
          })}
        </div>
      </div>

      <div className="border-t border-[var(--border-color)] p-3 space-y-2">
        {!collapsed && (
          <div className="mb-2">
            <LanguageSwitcher />
          </div>
        )}

        {user && (
          <div className="relative">
            <button
              onClick={() => setShowUserMenu(!showUserMenu)}
              className={`w-full flex items-center gap-3 p-2 hover:bg-[var(--bg-tertiary)] transition-all ${
                collapsed ? 'justify-center' : ''
              }`}
            >
              <div className="w-8 h-8 rounded-full bg-[var(--neon-cyan-dim)] border border-[var(--neon-cyan)] flex items-center justify-center flex-shrink-0">
                <span className="text-[var(--neon-cyan)] text-sm font-bold">
                  {user.username.charAt(0).toUpperCase()}
                </span>
              </div>
              {!collapsed && (
                <div className="flex-1 text-left min-w-0">
                  <div className="text-sm text-[var(--text-primary)] truncate">{user.username}</div>
                  <div className="text-[10px] text-[var(--text-muted)]">{user.role}</div>
                </div>
              )}
            </button>

            {showUserMenu && (
              <div className={`absolute bottom-full mb-1 ${collapsed ? 'left-full ml-1' : 'left-0 right-0'} bg-[var(--bg-secondary)] border border-[var(--border-color)] shadow-lg`}>
                <button
                  onClick={handleLogout}
                  className="w-full flex items-center gap-2 px-3 py-2 text-sm text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)] hover:text-[var(--neon-red)] transition-all"
                >
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                  </svg>
                  {!collapsed && t('header.logout')}
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </aside>
  );
}
