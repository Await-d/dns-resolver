import { useState } from 'react';
import { useLocation, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Sidebar from './Sidebar';
import MobileSidebar from './MobileSidebar';

interface LayoutProps {
  children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const { t } = useTranslation();
  const location = useLocation();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);

  return (
    <div className="min-h-screen hex-pattern">
      {/* Desktop Sidebar */}
      <div className="desktop-sidebar hidden md:block">
        <Sidebar
          collapsed={sidebarCollapsed}
          onToggle={() => setSidebarCollapsed(!sidebarCollapsed)}
        />
      </div>

      {/* Mobile Sidebar */}
      <MobileSidebar
        isOpen={mobileSidebarOpen}
        onClose={() => setMobileSidebarOpen(false)}
      />

      {/* Mobile Header */}
      <header className="mobile-header md:hidden">
        <button
          onClick={() => setMobileSidebarOpen(true)}
          className="mobile-menu-btn"
        >
          <span></span>
          <span></span>
          <span></span>
        </button>

        <div className="flex items-center gap-2">
          <div className="w-8 h-8 border-2 border-[var(--neon-cyan)] flex items-center justify-center">
            <span className="text-[var(--neon-cyan)] text-sm font-bold display">D</span>
          </div>
          <span className="display text-sm font-bold text-[var(--neon-cyan)]">DNS</span>
        </div>

        <div className="w-8"></div>
      </header>

      {/* Main Content */}
      <div
        className={`transition-all duration-300 md:ml-64 ${
          sidebarCollapsed ? 'md:ml-16' : 'md:ml-64'
        }`}
      >
        {/* Desktop Top Bar with Navigation */}
        <header className="hidden md:block sticky top-0 z-40 bg-[var(--bg-primary)]/90 backdrop-blur-sm border-b border-[var(--border-color)]">
          <div className="px-6 py-3 flex items-center justify-between">
            <nav className="flex items-center gap-4">
              <Link
                to="/"
                className={`px-3 py-1.5 text-sm transition-all ${
                  location.pathname === '/'
                    ? 'text-[var(--neon-cyan)] border-b-2 border-[var(--neon-cyan)]'
                    : 'text-[var(--text-secondary)] hover:text-[var(--text-primary)]'
                }`}
              >
                {t('nav.dnsQuery')}
              </Link>
              <Link
                to="/manage"
                className={`px-3 py-1.5 text-sm transition-all ${
                  location.pathname === '/manage'
                    ? 'text-[var(--neon-cyan)] border-b-2 border-[var(--neon-cyan)]'
                    : 'text-[var(--text-secondary)] hover:text-[var(--text-primary)]'
                }`}
              >
                {t('nav.dnsManage')}
              </Link>
              <Link
                to="/ddns"
                className={`px-3 py-1.5 text-sm transition-all ${
                  location.pathname === '/ddns'
                    ? 'text-[var(--neon-cyan)] border-b-2 border-[var(--neon-cyan)]'
                    : 'text-[var(--text-secondary)] hover:text-[var(--text-primary)]'
                }`}
              >
                {t('nav.ddns')}
              </Link>
            </nav>
            <div className="flex items-center gap-2">
              <div className="pulse-dot success"></div>
              <span className="text-[var(--text-secondary)] text-sm">{t('header.systemOnline')}</span>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="pb-24 md:pb-6">
          {children}
        </main>
      </div>
    </div>
  );
}
