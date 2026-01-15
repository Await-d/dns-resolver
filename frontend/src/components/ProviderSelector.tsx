import { useTranslation } from 'react-i18next';
import { useAuth } from '../contexts/AuthContext';

interface ProviderSelectorProps {
  isOpen: boolean;
  onClose: () => void;
}

export default function ProviderSelector({ isOpen, onClose }: ProviderSelectorProps) {
  const { t } = useTranslation();
  const { providers, currentProvider, switchProvider } = useAuth();

  if (!isOpen) return null;

  const handleSwitch = (providerId: string) => {
    switchProvider(providerId);
    onClose();
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="fixed inset-0 flex items-center justify-center z-50 p-4 pointer-events-none">
        <div className="cyber-card cyber-corner p-6 w-full max-w-2xl pointer-events-auto fade-in">
          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-3">
              <div className="w-1 h-6 bg-[var(--neon-magenta)]"></div>
              <div>
                <h2 className="display text-lg font-semibold tracking-wider text-[var(--text-primary)]">
                  {t('provider.title')}
                </h2>
                <p className="text-xs text-[var(--text-muted)]">{t('provider.subtitle')}</p>
              </div>
            </div>
            <button
              onClick={onClose}
              className="w-8 h-8 border border-[var(--border-color)] flex items-center justify-center text-[var(--text-muted)] hover:border-[var(--neon-red)] hover:text-[var(--neon-red)] transition-all"
            >
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Current Provider */}
          {currentProvider && (
            <div className="mb-6 p-4 border border-[var(--neon-cyan)] bg-[var(--neon-cyan-dim)]">
              <div className="text-xs text-[var(--neon-cyan)] uppercase tracking-wider mb-2">
                {t('provider.current')}
              </div>
              <div className="flex items-center gap-3">
                <span className="text-2xl">{currentProvider.icon}</span>
                <div>
                  <div className="font-medium text-[var(--text-primary)]">{currentProvider.name}</div>
                  <div className="text-xs text-[var(--text-muted)]">{currentProvider.description}</div>
                </div>
              </div>
            </div>
          )}

          {/* Provider List */}
          <div className="space-y-3 max-h-80 overflow-y-auto">
            <div className="text-xs text-[var(--text-muted)] uppercase tracking-wider mb-2">
              {t('provider.available')}
            </div>
            {providers.map((provider) => {
              const isActive = currentProvider?.id === provider.id;
              const isAvailable = provider.isActive;

              return (
                <button
                  key={provider.id}
                  onClick={() => isAvailable && !isActive && handleSwitch(provider.id)}
                  disabled={!isAvailable || isActive}
                  className={`w-full p-4 border text-left transition-all flex items-center gap-4 ${
                    isActive
                      ? 'border-[var(--neon-cyan)] bg-[var(--neon-cyan-dim)] cursor-default'
                      : isAvailable
                      ? 'border-[var(--border-color)] hover:border-[var(--neon-cyan)] cursor-pointer'
                      : 'border-[var(--border-color)] opacity-50 cursor-not-allowed'
                  }`}
                >
                  {/* Icon */}
                  <span className="text-2xl">{provider.icon}</span>

                  {/* Info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className={`font-medium ${isActive ? 'text-[var(--neon-cyan)]' : 'text-[var(--text-primary)]'}`}>
                        {provider.name}
                      </span>
                      {isActive && (
                        <span className="px-2 py-0.5 text-xs bg-[var(--neon-cyan)] text-[var(--bg-primary)] font-semibold">
                          {t('provider.active')}
                        </span>
                      )}
                      {!isAvailable && (
                        <span className="px-2 py-0.5 text-xs border border-[var(--text-muted)] text-[var(--text-muted)]">
                          {t('provider.unavailable')}
                        </span>
                      )}
                    </div>
                    <div className="text-xs text-[var(--text-muted)] truncate mt-0.5">
                      {provider.description}
                    </div>
                  </div>

                  {/* Stats */}
                  <div className="text-right">
                    <div
                      className="text-sm font-mono"
                      style={{ color: provider.color }}
                    >
                      {provider.ispCount}
                    </div>
                    <div className="text-xs text-[var(--text-muted)]">
                      {t('provider.ispCount', { count: provider.ispCount }).replace(/\d+\s*/, '')}
                    </div>
                  </div>

                  {/* Status Indicator */}
                  <div
                    className={`w-2 h-2 rounded-full ${
                      isActive
                        ? 'bg-[var(--neon-cyan)] shadow-[0_0_8px_var(--neon-cyan)]'
                        : isAvailable
                        ? 'bg-[var(--neon-green)]'
                        : 'bg-[var(--text-muted)]'
                    }`}
                  ></div>
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </>
  );
}
