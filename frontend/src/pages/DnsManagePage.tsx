import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import Layout from '../components/Layout';
import { useProviders, useDomains, useRecords, useAddRecord, useUpdateRecord, useDeleteRecord } from '../hooks/useProvider';
import { useUserProviderConfigs, useAddProviderConfig, useDeleteProviderConfig, useToggleProviderConfig } from '../hooks/useUserProvider';
import type { ProviderCredentials, DnsRecordInfo, ProviderFieldMeta } from '../types/provider';
import { RECORD_TYPES } from '../types/dns';

export default function DnsManagePage() {
  const { t } = useTranslation();
  const { data: providers = [], isLoading: loadingProviders } = useProviders();
  const { data: savedConfigs = [], isLoading: loadingSavedConfigs } = useUserProviderConfigs();
  const addConfigMutation = useAddProviderConfig();
  const deleteConfigMutation = useDeleteProviderConfig();
  const toggleConfigMutation = useToggleProviderConfig();

  const [credentials, setCredentials] = useState<ProviderCredentials | null>(null);
  const [selectedDomain, setSelectedDomain] = useState<string>('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingRecord, setEditingRecord] = useState<DnsRecordInfo | null>(null);

  // 获取当前选中服务商的字段元数据
  const selectedProvider = useMemo(() => {
    return providers.find(p => p.name === credentials?.providerName);
  }, [providers, credentials?.providerName]);

  const fieldMeta: ProviderFieldMeta = selectedProvider?.fieldMeta ?? {
    idLabel: null,
    secretLabel: null,
    extParamLabel: null,
    helpUrl: null,
    helpText: null,
  };

  // 当服务商变化时，检查是否需要 ID 字段
  const needsIdField = fieldMeta.idLabel !== null;
  // 检查是否需要额外参数字段
  const needsExtParam = fieldMeta.extParamLabel !== null;

  const { data: domains = [], isLoading: loadingDomains, refetch: refetchDomains } = useDomains(credentials, needsIdField);
  const recordsRequest = credentials && selectedDomain ? { ...credentials, domain: selectedDomain } : null;
  const { data: records = [], isLoading: loadingRecords } = useRecords(recordsRequest, needsIdField);

  const addMutation = useAddRecord();
  const updateMutation = useUpdateRecord();
  const deleteMutation = useDeleteRecord();

  useEffect(() => {
    if (domains.length > 0 && !selectedDomain) {
      setSelectedDomain(domains[0]);
    }
  }, [domains, selectedDomain]);

  // 检查当前服务商是否已保存
  const currentSavedConfig = useMemo(() => {
    return savedConfigs.find(c => c.providerName === credentials?.providerName);
  }, [savedConfigs, credentials?.providerName]);

  const handleConnect = () => {
    const hasRequiredFields = credentials?.providerName &&
      (needsIdField ? credentials?.id : true) &&
      credentials?.secret;
    if (hasRequiredFields) {
      refetchDomains();
    }
  };

  const handleSaveConfig = async () => {
    if (!credentials?.providerName || !credentials?.secret) return;
    try {
      await addConfigMutation.mutateAsync({
        providerName: credentials.providerName,
        apiId: credentials.id || '',
        apiSecret: credentials.secret,
        displayName: selectedProvider?.displayName || credentials.providerName,
        extraParams: credentials.extraParams,
      });
      alert(t('dnsManage.configSaved'));
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const handleDeleteConfig = async (configId: string) => {
    if (!confirm(t('dnsManage.confirmDeleteConfig'))) return;
    try {
      await deleteConfigMutation.mutateAsync(configId);
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const handleToggleConfig = async (configId: string) => {
    try {
      await toggleConfigMutation.mutateAsync(configId);
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const handleAddRecord = async (data: { subDomain: string; recordType: string; value: string; ttl: number }) => {
    if (!credentials || !selectedDomain) return;
    try {
      await addMutation.mutateAsync({
        ...credentials,
        domain: selectedDomain,
        ...data,
      });
      setShowAddModal(false);
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const handleUpdateRecord = async (recordId: string, value: string, ttl?: number) => {
    if (!credentials || !selectedDomain) return;
    try {
      await updateMutation.mutateAsync({
        ...credentials,
        domain: selectedDomain,
        recordId,
        value,
        ttl,
      });
      setEditingRecord(null);
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const handleDeleteRecord = async (recordId: string) => {
    if (!credentials || !selectedDomain) return;
    if (!confirm(t('dnsManage.confirmDelete'))) return;
    try {
      await deleteMutation.mutateAsync({
        ...credentials,
        domain: selectedDomain,
        recordId,
      });
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  return (
    <Layout>
      <div className="p-4 md:p-6 space-y-4 md:space-y-6">
        {/* Provider Configuration */}
        <div className="cyber-card cyber-corner p-4 md:p-5">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-1 h-5 bg-[var(--neon-cyan)]"></div>
            <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
              {t('dnsManage.providerConfig')}
            </h2>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 md:gap-4">
            <div>
              <label className="block text-xs text-[var(--text-muted)] mb-1">{t('dnsManage.provider')}</label>
              <select
                className="cyber-input w-full"
                value={credentials?.providerName || ''}
                onChange={(e) => setCredentials(prev => ({ ...prev!, providerName: e.target.value, id: '', secret: '', extraParams: {} }))}
                disabled={loadingProviders}
              >
                <option value="">{t('dnsManage.selectProvider')}</option>
                {providers.map((p) => (
                  <option key={p.name} value={p.name}>{p.displayName}</option>
                ))}
              </select>
            </div>

            {needsIdField && (
              <div>
                <label className="block text-xs text-[var(--text-muted)] mb-1">
                  {fieldMeta.idLabel || t('dnsManage.apiId')}
                </label>
                <input
                  type="text"
                  className="cyber-input w-full"
                  placeholder={fieldMeta.idLabel || 'Access Key / API ID'}
                  value={credentials?.id || ''}
                  onChange={(e) => setCredentials(prev => ({ ...prev!, id: e.target.value }))}
                />
              </div>
            )}

            <div>
              <label className="block text-xs text-[var(--text-muted)] mb-1">
                {fieldMeta.secretLabel || t('dnsManage.apiSecret')}
              </label>
              <input
                type="password"
                className="cyber-input w-full"
                placeholder={fieldMeta.secretLabel || 'Secret Key / API Token'}
                value={credentials?.secret || ''}
                onChange={(e) => setCredentials(prev => ({ ...prev!, secret: e.target.value }))}
              />
            </div>

            {needsExtParam && (
              <div>
                <label className="block text-xs text-[var(--text-muted)] mb-1">
                  {fieldMeta.extParamLabel}
                </label>
                <input
                  type="text"
                  className="cyber-input w-full"
                  placeholder={fieldMeta.extParamLabel || ''}
                  value={credentials?.extraParams?.extParam || ''}
                  onChange={(e) => setCredentials(prev => ({
                    ...prev!,
                    extraParams: { ...prev?.extraParams, extParam: e.target.value }
                  }))}
                />
              </div>
            )}

            <div className="flex items-end gap-2">
              <button
                className="cyber-btn cyber-btn-primary flex-1"
                onClick={handleConnect}
                disabled={!credentials?.providerName || (needsIdField && !credentials?.id) || !credentials?.secret || loadingDomains}
              >
                {loadingDomains ? t('common.loading') : t('dnsManage.connect')}
              </button>
              <button
                className="cyber-btn flex items-center justify-center px-3"
                onClick={handleSaveConfig}
                disabled={!credentials?.providerName || (needsIdField && !credentials?.id) || !credentials?.secret || addConfigMutation.isPending}
                title={t('dnsManage.saveConfig')}
              >
                {addConfigMutation.isPending ? (
                  <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                ) : (
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" />
                  </svg>
                )}
              </button>
              {fieldMeta.helpUrl && (
                <a
                  href={fieldMeta.helpUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="cyber-btn flex items-center justify-center px-3"
                  title={t('common.help') || 'Help'}
                >
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </a>
              )}
            </div>
          </div>

          {/* 帮助提示 */}
          {fieldMeta.helpText && (
            <p className="mt-3 text-xs text-[var(--text-muted)]">
              <span className="text-[var(--neon-orange)]">*</span> {fieldMeta.helpText}
            </p>
          )}

          {/* 已保存配置提示 */}
          {currentSavedConfig && (
            <p className="mt-3 text-xs text-[var(--neon-green)]">
              <svg className="w-3 h-3 inline mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
              {t('dnsManage.configAlreadySaved')}
            </p>
          )}
        </div>

        {/* Saved Configurations */}
        <div className="cyber-card cyber-corner p-4 md:p-5">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-1 h-5 bg-[var(--neon-green)]"></div>
            <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
              {t('dnsManage.savedConfigs')}
            </h2>
            <span className="text-xs text-[var(--text-muted)]">({savedConfigs.length})</span>
          </div>

          {loadingSavedConfigs ? (
            <div className="py-4">
              <div className="loading-bar mb-3"></div>
              <p className="text-center text-[var(--text-muted)] text-sm">{t('common.loading')}</p>
            </div>
          ) : savedConfigs.length === 0 ? (
            <div className="py-6 text-center text-[var(--text-muted)] text-sm">
              {t('dnsManage.noSavedConfigs')}
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
              {savedConfigs.map((config) => (
                <div
                  key={config.id}
                  className={`p-3 border transition-all ${
                    config.isActive
                      ? 'border-[var(--neon-green)] bg-[var(--neon-green-dim)]'
                      : 'border-[var(--border-color)] bg-[var(--bg-tertiary)]'
                  }`}
                >
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex items-center gap-2">
                      <span className={`w-2 h-2 rounded-full ${config.isActive ? 'bg-[var(--neon-green)]' : 'bg-[var(--text-muted)]'}`}></span>
                      <span className="font-medium text-sm text-[var(--text-primary)]">{config.displayName}</span>
                    </div>
                    <span className="text-[10px] text-[var(--text-muted)] font-mono">{config.providerName}</span>
                  </div>
                  <div className="flex items-center justify-end gap-3 mt-2">
                    <button
                      onClick={() => handleToggleConfig(config.id)}
                      disabled={toggleConfigMutation.isPending}
                      className={`text-[10px] px-2 py-1 border transition-all ${
                        config.isActive
                          ? 'border-[var(--neon-green)] text-[var(--neon-green)]'
                          : 'border-[var(--text-muted)] text-[var(--text-muted)] hover:border-[var(--neon-green)] hover:text-[var(--neon-green)]'
                      }`}
                    >
                      {config.isActive ? t('dnsManage.enabled') : t('dnsManage.disabled')}
                    </button>
                    <button
                      onClick={() => handleDeleteConfig(config.id)}
                      disabled={deleteConfigMutation.isPending}
                      className="text-[var(--neon-red)] text-xs hover:underline"
                    >
                      {t('common.delete')}
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Domain Selection */}
        {domains.length > 0 && (
          <div className="cyber-card cyber-corner p-4 md:p-5">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-1 h-5 bg-[var(--neon-magenta)]"></div>
              <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                {t('dnsManage.domains')}
              </h2>
              <span className="text-xs text-[var(--text-muted)]">({domains.length})</span>
            </div>

            <div className="flex flex-wrap gap-2">
              {domains.map((domain) => (
                <button
                  key={domain}
                  className={`px-3 py-2 text-xs sm:text-sm border transition-all min-h-[44px] ${
                    selectedDomain === domain
                      ? 'border-[var(--neon-cyan)] bg-[var(--neon-cyan-dim)] text-[var(--neon-cyan)]'
                      : 'border-[var(--border-color)] text-[var(--text-secondary)] hover:border-[var(--neon-cyan)]'
                  }`}
                  onClick={() => setSelectedDomain(domain)}
                >
                  {domain}
                </button>
              ))}
            </div>
          </div>
        )}

        {/* DNS Records */}
        {selectedDomain && (
          <div className="cyber-card cyber-corner p-4 md:p-5">
            <div className="flex items-center justify-between mb-4 gap-2">
              <div className="flex items-center gap-2 md:gap-3 min-w-0">
                <div className="w-1 h-5 bg-[var(--neon-orange)] flex-shrink-0"></div>
                <h2 className="display text-[10px] md:text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase truncate">
                  {t('dnsManage.records')}
                </h2>
                <span className="text-[10px] text-[var(--neon-cyan)] truncate hidden sm:inline">
                  {selectedDomain}
                </span>
              </div>
              <button
                className="cyber-btn cyber-btn-primary text-xs px-3 py-2 flex-shrink-0"
                onClick={() => setShowAddModal(true)}
              >
                <span className="hidden sm:inline">+ {t('dnsManage.addRecord')}</span>
                <span className="sm:hidden">+</span>
              </button>
            </div>

            {loadingRecords ? (
              <div className="py-8">
                <div className="loading-bar mb-3"></div>
                <p className="text-center text-[var(--text-muted)] text-sm">{t('common.loading')}</p>
              </div>
            ) : records.length === 0 ? (
              <div className="py-8 text-center text-[var(--text-muted)]">
                {t('dnsManage.noRecords')}
              </div>
            ) : (
              <>
                {/* Desktop Table */}
                <div className="hidden md:block overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-[var(--border-color)]">
                        <th className="text-left py-2 px-3 text-[var(--text-muted)] font-normal">{t('dnsManage.subDomain')}</th>
                        <th className="text-left py-2 px-3 text-[var(--text-muted)] font-normal">{t('dnsManage.type')}</th>
                        <th className="text-left py-2 px-3 text-[var(--text-muted)] font-normal">{t('dnsManage.value')}</th>
                        <th className="text-left py-2 px-3 text-[var(--text-muted)] font-normal">TTL</th>
                        <th className="text-right py-2 px-3 text-[var(--text-muted)] font-normal">{t('dnsManage.actions')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {records.map((record) => (
                        <tr key={record.recordId} className="border-b border-[var(--border-color)] hover:bg-[var(--bg-tertiary)]">
                          <td className="py-2 px-3 text-[var(--neon-cyan)] font-mono">{record.subDomain || '@'}</td>
                          <td className="py-2 px-3">
                            <span className="px-2 py-0.5 bg-[var(--bg-tertiary)] border border-[var(--border-color)] text-xs">
                              {record.recordType}
                            </span>
                          </td>
                          <td className="py-2 px-3 font-mono text-[var(--text-secondary)] max-w-xs truncate">{record.value}</td>
                          <td className="py-2 px-3 text-[var(--text-muted)]">{record.ttl}s</td>
                          <td className="py-2 px-3 text-right">
                            <button
                              className="text-[var(--neon-cyan)] hover:underline mr-3"
                              onClick={() => setEditingRecord(record)}
                            >
                              {t('common.edit')}
                            </button>
                            <button
                              className="text-[var(--neon-red)] hover:underline"
                              onClick={() => handleDeleteRecord(record.recordId)}
                              disabled={deleteMutation.isPending}
                            >
                              {t('common.delete')}
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                {/* Mobile Card List */}
                <div className="md:hidden space-y-3">
                  {records.map((record) => (
                    <div key={record.recordId} className="bg-[var(--bg-tertiary)] border border-[var(--border-color)] p-3">
                      <div className="flex items-center justify-between mb-2">
                        <div className="flex items-center gap-2">
                          <span className="text-[var(--neon-cyan)] font-mono text-sm">{record.subDomain || '@'}</span>
                          <span className="px-2 py-0.5 bg-[var(--bg-primary)] border border-[var(--border-color)] text-[10px]">
                            {record.recordType}
                          </span>
                        </div>
                        <span className="text-[10px] text-[var(--text-muted)]">{record.ttl}s</span>
                      </div>
                      <div className="font-mono text-xs text-[var(--text-secondary)] break-all mb-3 bg-[var(--bg-primary)] p-2">
                        {record.value}
                      </div>
                      <div className="flex justify-end gap-4">
                        <button
                          className="text-[var(--neon-cyan)] text-xs py-1"
                          onClick={() => setEditingRecord(record)}
                        >
                          {t('common.edit')}
                        </button>
                        <button
                          className="text-[var(--neon-red)] text-xs py-1"
                          onClick={() => handleDeleteRecord(record.recordId)}
                          disabled={deleteMutation.isPending}
                        >
                          {t('common.delete')}
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              </>
            )}
          </div>
        )}

        {/* Add Record Modal */}
        {showAddModal && (
          <RecordModal
            title={t('dnsManage.addRecord')}
            onClose={() => setShowAddModal(false)}
            onSubmit={handleAddRecord}
            isLoading={addMutation.isPending}
          />
        )}

        {/* Edit Record Modal */}
        {editingRecord && (
          <RecordModal
            title={t('dnsManage.editRecord')}
            record={editingRecord}
            onClose={() => setEditingRecord(null)}
            onSubmit={(data) => handleUpdateRecord(editingRecord.recordId, data.value, data.ttl)}
            isLoading={updateMutation.isPending}
          />
        )}
      </div>
    </Layout>
  );
}

interface RecordModalProps {
  title: string;
  record?: DnsRecordInfo;
  onClose: () => void;
  onSubmit: (data: { subDomain: string; recordType: string; value: string; ttl: number }) => void;
  isLoading: boolean;
}

function RecordModal({ title, record, onClose, onSubmit, isLoading }: RecordModalProps) {
  const { t } = useTranslation();
  const [subDomain, setSubDomain] = useState(record?.subDomain || '');
  const [recordType, setRecordType] = useState(record?.recordType || 'A');
  const [value, setValue] = useState(record?.value || '');
  const [ttl, setTtl] = useState(record?.ttl || 600);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({ subDomain, recordType, value, ttl });
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="cyber-card p-4 md:p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
        <h3 className="text-lg font-semibold text-[var(--text-primary)] mb-4">{title}</h3>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs text-[var(--text-muted)] mb-1">{t('dnsManage.subDomain')}</label>
            <input
              type="text"
              className="cyber-input w-full"
              placeholder="www / @ / *"
              value={subDomain}
              onChange={(e) => setSubDomain(e.target.value)}
              disabled={!!record}
            />
          </div>

          <div>
            <label className="block text-xs text-[var(--text-muted)] mb-1">{t('dnsManage.type')}</label>
            <select
              className="cyber-input w-full"
              value={recordType}
              onChange={(e) => setRecordType(e.target.value)}
              disabled={!!record}
            >
              {RECORD_TYPES.map((type) => (
                <option key={type} value={type}>{type}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs text-[var(--text-muted)] mb-1">{t('dnsManage.value')}</label>
            <input
              type="text"
              className="cyber-input w-full"
              placeholder="1.2.3.4"
              value={value}
              onChange={(e) => setValue(e.target.value)}
              required
            />
          </div>

          <div>
            <label className="block text-xs text-[var(--text-muted)] mb-1">TTL ({t('common.seconds')})</label>
            <input
              type="number"
              className="cyber-input w-full"
              value={ttl}
              onChange={(e) => setTtl(parseInt(e.target.value) || 600)}
              min={60}
            />
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              className="cyber-btn flex-1"
              onClick={onClose}
              disabled={isLoading}
            >
              {t('common.cancel')}
            </button>
            <button
              type="submit"
              className="cyber-btn cyber-btn-primary flex-1"
              disabled={isLoading || !value}
            >
              {isLoading ? t('common.loading') : t('common.save')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
