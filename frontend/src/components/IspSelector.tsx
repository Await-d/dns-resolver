import { useTranslation } from 'react-i18next';
import type { IspInfo } from '../types/dns';

interface IspSelectorProps {
  isps: IspInfo[];
  selectedIsps: string[];
  onChange: (selected: string[]) => void;
  disabled?: boolean;
}

export default function IspSelector({ isps, selectedIsps, onChange, disabled }: IspSelectorProps) {
  const { t } = useTranslation();

  const handleToggle = (ispId: string) => {
    if (disabled) return;
    if (selectedIsps.includes(ispId)) {
      onChange(selectedIsps.filter(id => id !== ispId));
    } else {
      onChange([...selectedIsps, ispId]);
    }
  };

  const handleSelectAll = () => {
    if (disabled) return;
    if (selectedIsps.length === isps.length) {
      onChange([]);
    } else {
      onChange(isps.map(isp => isp.id));
    }
  };

  return (
    <div className="space-y-4">
      {/* Header with Select All */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <span className="text-xs text-[var(--text-muted)] uppercase tracking-wider">
            {t('providers.available')}
          </span>
        </div>
        <button
          type="button"
          onClick={handleSelectAll}
          disabled={disabled}
          className="group flex items-center gap-2 text-xs uppercase tracking-wider transition-all disabled:opacity-40"
        >
          <span className={`w-4 h-4 border flex items-center justify-center transition-all ${
            selectedIsps.length === isps.length
              ? 'border-[var(--neon-cyan)] bg-[var(--neon-cyan)] text-[var(--bg-primary)]'
              : 'border-[var(--border-color)] group-hover:border-[var(--neon-cyan)]'
          }`}>
            {selectedIsps.length === isps.length && (
              <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
              </svg>
            )}
          </span>
          <span className={`transition-colors ${
            selectedIsps.length === isps.length
              ? 'text-[var(--neon-cyan)]'
              : 'text-[var(--text-secondary)] group-hover:text-[var(--neon-cyan)]'
          }`}>
            {selectedIsps.length === isps.length ? t('providers.deselectAll') : t('providers.selectAll')}
          </span>
        </button>
      </div>

      {/* ISP Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 stagger-children">
        {isps.map((isp) => {
          const isSelected = selectedIsps.includes(isp.id);
          return (
            <button
              key={isp.id}
              type="button"
              onClick={() => handleToggle(isp.id)}
              disabled={disabled}
              className={`isp-chip text-left ${isSelected ? 'selected' : ''}`}
            >
              {/* Checkbox */}
              <span className={`w-4 h-4 border flex-shrink-0 flex items-center justify-center transition-all ${
                isSelected
                  ? 'border-[var(--neon-cyan)] bg-[var(--neon-cyan)] text-[var(--bg-primary)]'
                  : 'border-[var(--text-muted)]'
              }`}>
                {isSelected && (
                  <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                )}
              </span>

              {/* ISP Info */}
              <div className="flex-1 min-w-0">
                <div className={`font-medium truncate transition-colors ${
                  isSelected ? 'text-[var(--neon-cyan)]' : 'text-[var(--text-primary)]'
                }`}>
                  {isp.name}
                </div>
                <div className="text-xs text-[var(--text-muted)] truncate mt-0.5 font-mono">
                  {isp.primaryDns}
                </div>
              </div>

              {/* Status Indicator */}
              <div className={`w-2 h-2 rounded-full flex-shrink-0 transition-all ${
                isSelected
                  ? 'bg-[var(--neon-cyan)] shadow-[0_0_8px_var(--neon-cyan)]'
                  : 'bg-[var(--text-muted)]'
              }`}></div>
            </button>
          );
        })}
      </div>

      {/* Selection Summary */}
      {selectedIsps.length > 0 && (
        <div className="pt-4 border-t border-[var(--border-color)]">
          <div className="flex items-center gap-2 text-xs">
            <span className="text-[var(--text-muted)]">{t('providers.selected')}:</span>
            <div className="flex flex-wrap gap-2">
              {selectedIsps.slice(0, 5).map((id) => {
                const isp = isps.find(i => i.id === id);
                return isp ? (
                  <span
                    key={id}
                    className="px-2 py-0.5 bg-[var(--neon-cyan-dim)] border border-[var(--neon-cyan)] text-[var(--neon-cyan)]"
                  >
                    {isp.name}
                  </span>
                ) : null;
              })}
              {selectedIsps.length > 5 && (
                <span className="px-2 py-0.5 text-[var(--text-muted)]">
                  +{selectedIsps.length - 5} more
                </span>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
