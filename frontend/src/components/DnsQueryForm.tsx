import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { RecordType } from '../types/dns';
import { RECORD_TYPES } from '../types/dns';

interface DnsQueryFormProps {
  onSubmit: (domain: string, recordType: RecordType) => void;
  isLoading: boolean;
}

export default function DnsQueryForm({ onSubmit, isLoading }: DnsQueryFormProps) {
  const { t } = useTranslation();
  const [domain, setDomain] = useState('');
  const [recordType, setRecordType] = useState<RecordType>('A');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (domain.trim()) {
      onSubmit(domain.trim(), recordType);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Domain Input Row */}
      <div className="flex flex-col lg:flex-row gap-4">
        {/* Domain Field */}
        <div className="flex-1">
          <label className="flex items-center gap-2 mb-2">
            <span className="text-[var(--neon-cyan)] text-xs">▸</span>
            <span className="display text-xs font-semibold tracking-widest text-[var(--text-secondary)] uppercase">
              {t('query.domain')}
            </span>
          </label>
          <div className="relative">
            <input
              type="text"
              value={domain}
              onChange={(e) => setDomain(e.target.value)}
              placeholder={t('query.domainPlaceholder')}
              className="terminal-input pl-12"
              disabled={isLoading}
            />
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-[var(--neon-cyan)] text-sm opacity-60">
              $&gt;
            </span>
          </div>
        </div>

        {/* Record Type Field */}
        <div className="w-full lg:w-48">
          <label className="flex items-center gap-2 mb-2">
            <span className="text-[var(--neon-magenta)] text-xs">▸</span>
            <span className="display text-xs font-semibold tracking-widest text-[var(--text-secondary)] uppercase">
              {t('query.recordType')}
            </span>
          </label>
          <select
            value={recordType}
            onChange={(e) => setRecordType(e.target.value as RecordType)}
            className="terminal-select w-full"
            disabled={isLoading}
          >
            {RECORD_TYPES.map((type) => (
              <option key={type} value={type}>{type}</option>
            ))}
          </select>
        </div>

        {/* Submit Button */}
        <div className="flex items-end">
          <button
            type="submit"
            disabled={isLoading || !domain.trim()}
            className="neon-btn w-full lg:w-auto whitespace-nowrap"
          >
            {isLoading ? (
              <span className="flex items-center gap-2">
                <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                  <circle
                    className="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    strokeWidth="4"
                    fill="none"
                  />
                  <path
                    className="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  />
                </svg>
                {t('query.resolving')}
              </span>
            ) : (
              <span className="flex items-center gap-2">
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                {t('query.execute')}
              </span>
            )}
          </button>
        </div>
      </div>

      {/* Quick Tips */}
      <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-xs text-[var(--text-muted)]">
        <span className="flex items-center gap-1">
          <span className="text-[var(--neon-cyan)]">TIP:</span>
          {t('query.tip')}
        </span>
        <span className="hidden sm:inline text-[var(--border-color)]">|</span>
        <span className="flex items-center gap-2">
          <span className="text-[var(--text-muted)]">{t('query.commonTypes')}:</span>
          {['A', 'AAAA', 'CNAME', 'MX'].map((type) => (
            <button
              key={type}
              type="button"
              onClick={() => setRecordType(type as RecordType)}
              className={`px-2 py-0.5 border transition-all ${
                recordType === type
                  ? 'border-[var(--neon-cyan)] text-[var(--neon-cyan)] bg-[var(--neon-cyan-dim)]'
                  : 'border-[var(--border-color)] text-[var(--text-muted)] hover:border-[var(--text-secondary)] hover:text-[var(--text-secondary)]'
              }`}
            >
              {type}
            </button>
          ))}
        </span>
      </div>
    </form>
  );
}
