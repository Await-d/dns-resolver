import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import LanguageSwitcher from './LanguageSwitcher';

interface MobileSidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export default function MobileSidebar({ isOpen, onClose }: MobileSidebarProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user, logout, providers, currentProvider, switchProvider } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
    onClose();
  };

  const handleProviderSwitch = (providerId: string) => {
    switchProvider(providerId);
    onClose();
  };

  return (
    <>
      <div
        className={`mobile-nav-overlay ${isOpen ? 'active' : ''}`}
        onClick={onClose}
      />

      <aside className={`mobile-sidebar ${isOpen ? 'active' : ''} flex flex-col`}>
        <div className="p-4 border-b border-[var(--border-color)]">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 border-2 border-[var(--neon-cyan)] flex items-center justify-center relative">
              <span className="text-[var(--neon-cyan)] text-xl font-bold display">D</span>
              <div className="absolute -top-1 -right-1 w-2 h-2 bg-[var(--neon-cyan)]"></div>
            </div>
            <div>
              <h1 className="display text-sm font-bold tracking-wider text-[var(--neon-cyan)]">
                DNS RESOLVER
              </h1>
              <p className="text-[10px] text-[var(--text-muted)]">{t('app.version')}</p>
            </div>
          </div>
        </div>

        {user && (
          <div className="p-4 border-b border-[var(--border-color)] bg-[var(--bg-tertiary)]">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-[var(--neon-cyan-dim)] border border-[var(--neon-cyan)] flex items-center justify-center">
                <span className="text-[var(--neon-cyan)] font-bold">
                  {user.username.charAt(0).toUpperCase()}
                </span>
              </div>
              <div className="flex-1">
                <div className="text-sm text-[var(--text-primary)]">{user.username}</div>
                <div className="text-[10px] text-[var(--text-muted)]">{user.role}</div>
              </div>
            </div>
          </div>
        )}

        <div className="flex-1 overflow-y-auto p-3">
          <div className="mb-3">
            <span className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider">
              {t('provider.title')}
            </span>
          </div>

          <div className="space-y-2">
            {providers.map((provider) => {
              const isActive = currentProvider?.id === provider.id;
              const isAvailable = provider.isActive;

              return (
                <button
                  key={provider.id}
                  onClick={() => isAvailable && handleProviderSwitch(provider.id)}
                  disabled={!isAvailable}
                  className={`w-full flex items-center gap-3 p-3 transition-all ${
                    isActive
                      ? 'bg-[var(--neon-cyan-dim)] border-l-2 border-[var(--neon-cyan)]'
                      : isAvailable
                      ? 'hover:bg-[var(--bg-tertiary)] border-l-2 border-transparent'
                      : 'opacity-40 cursor-not-allowed border-l-2 border-transparent'
                  }`}
                >
                  <span className="text-xl">{provider.icon}</span>
                  <div className="flex-1 text-left">
                    <div className={`text-sm ${isActive ? 'text-[var(--neon-cyan)]' : 'text-[var(--text-primary)]'}`}>
                      {provider.name}
                    </div>
                    <div className="text-[10px] text-[var(--text-muted)]">
                      {provider.ispCount} DNS
                    </div>
                  </div>
                  {isActive && (
                    <div className="w-2 h-2 rounded-full bg-[var(--neon-cyan)] shadow-[0_0_8px_var(--neon-cyan)]"></div>
                  )}
                </button>
              );
            })}
          </div>
        </div>

        <div className="border-t border-[var(--border-color)] p-3 space-y-3">
          <LanguageSwitcher />

          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-2 p-3 text-[var(--text-secondary)] hover:text-[var(--neon-red)] hover:bg-[var(--neon-red-dim)] transition-all"
          >
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
            </svg>
            <span className="text-sm">{t('header.logout')}</span>
          </button>
        </div>
      </aside>
    </>
  );
}
