import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import MobileBottomNav from '../components/MobileBottomNav';
import DnsQueryForm from '../components/DnsQueryForm';
import IspSelector from '../components/IspSelector';
import ResultTable from '../components/ResultTable';
import { useDnsCompare } from '../hooks/useDnsQuery';
import { useUserProviderConfigs } from '../hooks/useUserProvider';
import type { RecordType, ResolveResult, IspInfo } from '../types/dns';

export default function HomePage() {
  const { t } = useTranslation();

  // 使用用户配置的服务商列表
  const { data: userConfigs = [], isLoading: isLoadingConfigs } = useUserProviderConfigs();
  const { mutate: compare, isPending } = useDnsCompare();

  // 将用户配置转换为 IspInfo 格式
  const configuredProviders: IspInfo[] = userConfigs
    .filter(c => c.isActive)
    .map(c => ({
      id: c.providerName,
      name: c.providerName,
      displayName: c.displayName,
    }));

  const [mobileActiveTab, setMobileActiveTab] = useState<'query' | 'providers' | 'results'>('query');
  const [selectedIsps, setSelectedIsps] = useState<string[]>([]);
  const [results, setResults] = useState<ResolveResult[]>([]);

  useEffect(() => {
    if (configuredProviders.length > 0 && selectedIsps.length === 0) {
      setSelectedIsps(configuredProviders.slice(0, 4).map(p => p.id));
    }
  }, [configuredProviders.length, selectedIsps.length]);

  const handleSubmit = (domain: string, recordType: RecordType) => {
    if (selectedIsps.length === 0) {
      alert(t('providers.selected') + ': 0');
      return;
    }

    compare(
      { domain, recordType, ispList: selectedIsps },
      {
        onSuccess: (data) => {
          setResults(data.results);
          setMobileActiveTab('results');
        },
        onError: (error) => {
          alert(`${t('common.error')}: ${error.message}`);
        }
      }
    );
  };

  const handleResultEdit = (index: number, field: string, value: string) => {
    setResults(prev => {
      const newResults = [...prev];
      if (field.startsWith('record_')) {
        const recordIndex = parseInt(field.split('_')[1]);
        if (newResults[index].records[recordIndex]) {
          newResults[index] = {
            ...newResults[index],
            records: newResults[index].records.map((r, i) =>
              i === recordIndex ? { ...r, value } : r
            )
          };
        }
      } else if (field === 'ttl') {
        newResults[index] = {
          ...newResults[index],
          records: newResults[index].records.map((r, i) =>
            i === 0 ? { ...r, ttl: parseInt(value) || r.ttl } : r
          )
        };
      }
      return newResults;
    });
  };

  // 未配置服务商时显示提示
  const renderNoProvidersHint = () => (
    <div className="py-12 text-center">
      <div className="w-16 h-16 mx-auto mb-4 border-2 border-dashed border-[var(--border-color)] flex items-center justify-center">
        <svg className="w-8 h-8 text-[var(--text-muted)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
      </div>
      <p className="text-sm text-[var(--text-secondary)] mb-2">{t('providers.noConfigured')}</p>
      <p className="text-xs text-[var(--text-muted)] mb-4">{t('providers.noConfiguredHint')}</p>
      <Link
        to="/manage"
        className="inline-flex items-center gap-2 px-4 py-2 bg-[var(--neon-cyan-dim)] border border-[var(--neon-cyan)] text-[var(--neon-cyan)] text-sm hover:bg-[var(--neon-cyan)] hover:text-[var(--bg-primary)] transition-all"
      >
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
        {t('providers.goToConfig')}
      </Link>
    </div>
  );

  return (
    <Layout>
      <div className="p-4 md:p-6 pb-24 md:pb-6">
        {/* Desktop Layout */}
        <div className="hidden md:block">
          {/* Query & Stats */}
          <div className="grid grid-cols-1 xl:grid-cols-3 gap-6 mb-6">
            <div className="xl:col-span-2">
              <div className="cyber-card cyber-corner p-5 h-full">
                <div className="flex items-center gap-3 mb-4">
                  <div className="w-1 h-5 bg-[var(--neon-cyan)]"></div>
                  <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                    {t('query.title')}
                  </h2>
                </div>
                <DnsQueryForm onSubmit={handleSubmit} isLoading={isPending} />
              </div>
            </div>

            <div className="cyber-card cyber-corner p-5">
              <div className="flex items-center gap-3 mb-4">
                <div className="w-1 h-5 bg-[var(--neon-green)]"></div>
                <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                  {t('results.stats')}
                </h2>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="p-3 border border-[var(--border-color)] bg-[var(--bg-tertiary)]">
                  <div className="text-2xl font-bold text-[var(--neon-cyan)] font-mono">
                    {results.length}
                  </div>
                  <div className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider mt-1">
                    {t('results.totalQueries')}
                  </div>
                </div>
                <div className="p-3 border border-[var(--border-color)] bg-[var(--bg-tertiary)]">
                  <div className="text-2xl font-bold text-[var(--neon-green)] font-mono">
                    {results.filter(r => r.success).length}
                  </div>
                  <div className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider mt-1">
                    {t('results.success')}
                  </div>
                </div>
                <div className="p-3 border border-[var(--border-color)] bg-[var(--bg-tertiary)]">
                  <div className="text-2xl font-bold text-[var(--neon-orange)] font-mono">
                    {results.length > 0 ? Math.round(results.reduce((a, r) => a + r.queryTimeMs, 0) / results.length) : 0}
                  </div>
                  <div className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider mt-1">
                    {t('results.avgTime')} (ms)
                  </div>
                </div>
                <div className="p-3 border border-[var(--border-color)] bg-[var(--bg-tertiary)]">
                  <div className="text-2xl font-bold text-[var(--neon-magenta)] font-mono">
                    {selectedIsps.length}
                  </div>
                  <div className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider mt-1">
                    {t('providers.selected')}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* ISP Selection */}
          <div className="cyber-card cyber-corner p-5 mb-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-1 h-5 bg-[var(--neon-magenta)]"></div>
              <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                {t('providers.title')}
              </h2>
              <div className="flex-1"></div>
              <span className="text-xs text-[var(--text-muted)]">
                {selectedIsps.length} / {configuredProviders.length} {t('providers.selected')}
              </span>
            </div>
            {isLoadingConfigs ? (
              <div className="py-6">
                <div className="loading-bar mb-3"></div>
                <p className="text-center text-[var(--text-muted)] text-sm">{t('common.loading')}</p>
              </div>
            ) : configuredProviders.length === 0 ? (
              renderNoProvidersHint()
            ) : (
              <IspSelector
                isps={configuredProviders}
                selectedIsps={selectedIsps}
                onChange={setSelectedIsps}
                disabled={isPending}
              />
            )}
          </div>

          {/* Results */}
          <div className="cyber-card cyber-corner p-5">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-1 h-5 bg-[var(--neon-green)]"></div>
              <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                {t('results.title')}
              </h2>
              <div className="flex-1"></div>
              {results.length > 0 && (
                <span className="text-xs text-[var(--neon-green)]">
                  {results.filter(r => r.success).length} / {results.length} {t('results.success')}
                </span>
              )}
            </div>
            {isPending ? (
              <div className="py-12">
                <div className="loading-bar mb-4"></div>
                <p className="text-center text-[var(--text-muted)] text-sm typing-cursor">
                  {t('query.resolving')}
                </p>
              </div>
            ) : (
              <ResultTable results={results} onEdit={handleResultEdit} />
            )}
          </div>
        </div>

        {/* Mobile Layout - Tab Based */}
        <div className="md:hidden">
          {/* Query Tab */}
          {mobileActiveTab === 'query' && (
            <div className="space-y-4 fade-in">
              {/* Quick Stats */}
              <div className="grid grid-cols-4 gap-2">
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-lg font-bold text-[var(--neon-cyan)] font-mono">{results.length}</div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase">{t('results.totalQueries')}</div>
                </div>
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-lg font-bold text-[var(--neon-green)] font-mono">{results.filter(r => r.success).length}</div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase">{t('results.success')}</div>
                </div>
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-lg font-bold text-[var(--neon-orange)] font-mono">
                    {results.length > 0 ? Math.round(results.reduce((a, r) => a + r.queryTimeMs, 0) / results.length) : 0}
                  </div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase">ms</div>
                </div>
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-lg font-bold text-[var(--neon-magenta)] font-mono">{selectedIsps.length}</div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase">DNS</div>
                </div>
              </div>

              {/* Query Form */}
              <div className="cyber-card p-4">
                <div className="flex items-center gap-2 mb-3">
                  <div className="w-1 h-4 bg-[var(--neon-cyan)]"></div>
                  <h2 className="display text-[10px] font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                    {t('query.title')}
                  </h2>
                </div>
                <DnsQueryForm onSubmit={handleSubmit} isLoading={isPending} />
              </div>
            </div>
          )}

          {/* Providers Tab */}
          {mobileActiveTab === 'providers' && (
            <div className="cyber-card p-4 fade-in">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="w-1 h-4 bg-[var(--neon-magenta)]"></div>
                  <h2 className="display text-[10px] font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                    {t('providers.title')}
                  </h2>
                </div>
                <span className="text-[10px] text-[var(--text-muted)]">
                  {selectedIsps.length}/{configuredProviders.length}
                </span>
              </div>
              {isLoadingConfigs ? (
                <div className="py-6">
                  <div className="loading-bar mb-3"></div>
                  <p className="text-center text-[var(--text-muted)] text-sm">{t('common.loading')}</p>
                </div>
              ) : configuredProviders.length === 0 ? (
                renderNoProvidersHint()
              ) : (
                <IspSelector
                  isps={configuredProviders}
                  selectedIsps={selectedIsps}
                  onChange={setSelectedIsps}
                  disabled={isPending}
                />
              )}
            </div>
          )}

          {/* Results Tab */}
          {mobileActiveTab === 'results' && (
            <div className="cyber-card p-4 fade-in">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="w-1 h-4 bg-[var(--neon-green)]"></div>
                  <h2 className="display text-[10px] font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                    {t('results.title')}
                  </h2>
                </div>
                {results.length > 0 && (
                  <span className="text-[10px] text-[var(--neon-green)]">
                    {results.filter(r => r.success).length}/{results.length}
                  </span>
                )}
              </div>
              {isPending ? (
                <div className="py-8">
                  <div className="loading-bar mb-3"></div>
                  <p className="text-center text-[var(--text-muted)] text-sm typing-cursor">
                    {t('query.resolving')}
                  </p>
                </div>
              ) : (
                <MobileResultList results={results} />
              )}
            </div>
          )}
        </div>
      </div>

      {/* Mobile Bottom Navigation */}
      <MobileBottomNav
        activeTab={mobileActiveTab}
        onTabChange={setMobileActiveTab}
      />
    </Layout>
  );
}

// Mobile Result List Component
function MobileResultList({ results }: { results: ResolveResult[] }) {
  const { t } = useTranslation();

  if (results.length === 0) {
    return (
      <div className="py-8 text-center">
        <div className="w-12 h-12 mx-auto mb-3 border-2 border-dashed border-[var(--border-color)] flex items-center justify-center">
          <svg className="w-6 h-6 text-[var(--text-muted)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <p className="text-sm text-[var(--text-secondary)]">{t('results.noData')}</p>
        <p className="text-[10px] text-[var(--text-muted)] mt-1">{t('results.noDataHint')}</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {results.map((result, index) => (
        <div key={index} className="mobile-result-card">
          <div className="mobile-result-card-header">
            <div className="flex items-center gap-2">
              <div className={`w-1 h-6 ${result.success ? 'bg-[var(--neon-green)]' : 'bg-[var(--neon-red)]'}`}></div>
              <div>
                <div className="text-sm font-medium text-[var(--text-primary)]">{result.ispName}</div>
                <div className="text-[10px] text-[var(--text-muted)] font-mono">{result.dnsServer}</div>
              </div>
            </div>
            {result.success ? (
              <span className="status-badge success">
                <span className="pulse-dot success"></span>
                {t('results.online')}
              </span>
            ) : (
              <span className="status-badge error">
                <span className="pulse-dot error"></span>
                {t('results.failed')}
              </span>
            )}
          </div>

          <div className="mobile-result-card-body">
            <div className="mobile-result-card-item col-span-2">
              <span className="mobile-result-card-label">{t('results.resolution')}</span>
              {result.success && result.records.length > 0 ? (
                <div className="space-y-1">
                  {result.records.slice(0, 2).map((record, i) => (
                    <code key={i} className="mobile-result-card-value block text-[var(--neon-cyan)] bg-[var(--bg-primary)] px-2 py-1 text-[11px]">
                      {record.value}
                    </code>
                  ))}
                  {result.records.length > 2 && (
                    <span className="text-[10px] text-[var(--text-muted)]">+{result.records.length - 2} more</span>
                  )}
                </div>
              ) : result.success ? (
                <span className="mobile-result-card-value text-[var(--text-muted)]">{t('results.noRecords')}</span>
              ) : (
                <span className="mobile-result-card-value text-[var(--neon-red)]">{result.errorMessage}</span>
              )}
            </div>

            <div className="mobile-result-card-item">
              <span className="mobile-result-card-label">{t('results.ttl')}</span>
              <span className="mobile-result-card-value">
                {result.success && result.records.length > 0 ? `${result.records[0].ttl}s` : '—'}
              </span>
            </div>

            <div className="mobile-result-card-item">
              <span className="mobile-result-card-label">{t('results.latency')}</span>
              <span className={`mobile-result-card-value ${
                result.queryTimeMs < 50
                  ? 'text-[var(--neon-green)]'
                  : result.queryTimeMs < 150
                  ? 'text-[var(--neon-orange)]'
                  : 'text-[var(--neon-red)]'
              }`}>
                {result.queryTimeMs}ms
              </span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
