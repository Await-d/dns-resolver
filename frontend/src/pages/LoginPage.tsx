import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../contexts/AuthContext';
import LanguageSwitcher from '../components/LanguageSwitcher';

export default function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { login } = useAuth();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const success = await login({ username, password });
      if (success) {
        navigate('/');
      } else {
        setError(t('login.error'));
      }
    } catch {
      setError(t('login.error'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="h-screen overflow-y-auto hex-pattern flex items-center justify-center p-4">
      {/* Background Effects */}
      <div className="fixed inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-[var(--neon-cyan)] opacity-5 blur-[100px] rounded-full"></div>
        <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-[var(--neon-magenta)] opacity-5 blur-[100px] rounded-full"></div>
      </div>

      {/* Language Switcher */}
      <div className="fixed top-4 right-4 z-50">
        <LanguageSwitcher />
      </div>

      {/* Login Card */}
      <div className="w-full max-w-md relative">
        {/* Decorative Lines */}
        <div className="absolute -top-20 left-1/2 -translate-x-1/2 w-px h-16 bg-gradient-to-b from-transparent via-[var(--neon-cyan)] to-transparent opacity-50"></div>

        <div className="cyber-card cyber-corner p-8 fade-in">
          {/* Header */}
          <div className="text-center mb-8">
            {/* Logo */}
            <div className="inline-flex items-center justify-center w-16 h-16 border-2 border-[var(--neon-cyan)] mb-4 relative">
              <span className="text-[var(--neon-cyan)] text-3xl font-bold display">D</span>
              <div className="absolute -top-1 -right-1 w-3 h-3 bg-[var(--neon-cyan)]"></div>
              <div className="absolute -bottom-1 -left-1 w-3 h-3 bg-[var(--neon-cyan)]"></div>
            </div>

            <h1
              className="display text-2xl font-bold tracking-wider text-[var(--neon-cyan)] glitch-text mb-2"
              data-text={t('login.title')}
            >
              {t('login.title')}
            </h1>
            <p className="text-sm text-[var(--text-muted)]">{t('login.subtitle')}</p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Username */}
            <div>
              <label className="flex items-center gap-2 mb-2">
                <span className="text-[var(--neon-cyan)] text-xs">▸</span>
                <span className="display text-xs font-semibold tracking-widest text-[var(--text-secondary)] uppercase">
                  {t('login.username')}
                </span>
              </label>
              <div className="relative">
                <input
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  placeholder={t('login.usernamePlaceholder')}
                  className="terminal-input pl-12"
                  disabled={isLoading}
                  autoComplete="username"
                />
                <span className="absolute left-4 top-1/2 -translate-y-1/2 text-[var(--neon-cyan)] opacity-60">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                </span>
              </div>
            </div>

            {/* Password */}
            <div>
              <label className="flex items-center gap-2 mb-2">
                <span className="text-[var(--neon-magenta)] text-xs">▸</span>
                <span className="display text-xs font-semibold tracking-widest text-[var(--text-secondary)] uppercase">
                  {t('login.password')}
                </span>
              </label>
              <div className="relative">
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder={t('login.passwordPlaceholder')}
                  className="terminal-input pl-12"
                  disabled={isLoading}
                  autoComplete="current-password"
                />
                <span className="absolute left-4 top-1/2 -translate-y-1/2 text-[var(--neon-magenta)] opacity-60">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </span>
              </div>
            </div>

            {/* Remember Me */}
            <div className="flex items-center justify-between">
              <label className="flex items-center gap-2 cursor-pointer group">
                <span
                  className={`w-4 h-4 border flex items-center justify-center transition-all ${
                    rememberMe
                      ? 'border-[var(--neon-cyan)] bg-[var(--neon-cyan)] text-[var(--bg-primary)]'
                      : 'border-[var(--border-color)] group-hover:border-[var(--neon-cyan)]'
                  }`}
                  onClick={() => setRememberMe(!rememberMe)}
                >
                  {rememberMe && (
                    <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                  )}
                </span>
                <span className="text-sm text-[var(--text-secondary)]">{t('login.rememberMe')}</span>
              </label>
            </div>

            {/* Error Message */}
            {error && (
              <div className="p-3 border border-[var(--neon-red)] bg-[var(--neon-red-dim)] text-[var(--neon-red)] text-sm flex items-center gap-2">
                <svg className="w-4 h-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {error}
              </div>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoading || !username || !password}
              className="neon-btn w-full"
            >
              {isLoading ? (
                <span className="flex items-center justify-center gap-2">
                  <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                  </svg>
                  {t('login.submitting')}
                </span>
              ) : (
                <span className="flex items-center justify-center gap-2">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                  </svg>
                  {t('login.submit')}
                </span>
              )}
            </button>
          </form>

          {/* Footer */}
          <div className="mt-8 pt-6 border-t border-[var(--border-color)] text-center">
            <p className="text-xs text-[var(--text-muted)]">
              <span className="text-[var(--neon-green)]">●</span> {t('login.secureAccess')}
            </p>
          </div>
        </div>

        {/* Decorative Lines */}
        <div className="absolute -bottom-20 left-1/2 -translate-x-1/2 w-px h-16 bg-gradient-to-t from-transparent via-[var(--neon-cyan)] to-transparent opacity-50"></div>
      </div>
    </div>
  );
}
