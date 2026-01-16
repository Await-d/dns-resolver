import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Sidebar from './Sidebar';
import MobileSidebar from './MobileSidebar';

interface LayoutProps {
  children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const { t } = useTranslation();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);

  return (
    <div className="h-screen overflow-hidden hex-pattern flex flex-col">
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
        className={`flex-1 flex flex-col overflow-hidden transition-all duration-300 md:ml-64 ${
          sidebarCollapsed ? 'md:ml-16' : 'md:ml-64'
        }`}
      >
        {/* Desktop Top Bar */}
        <header className="hidden md:flex flex-shrink-0 z-40 bg-[var(--bg-primary)]/90 backdrop-blur-sm border-b border-[var(--border-color)]">
          <div className="flex-1 px-6 py-3 flex items-center justify-end">
            <div className="flex items-center gap-2">
              <div className="pulse-dot success"></div>
              <span className="text-[var(--text-secondary)] text-sm">{t('header.systemOnline')}</span>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-y-auto overflow-x-hidden pb-20 md:pb-0">
          {children}
        </main>
      </div>
    </div>
  );
}
