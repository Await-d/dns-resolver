import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import Layout from '../components/Layout';
import { useProviders, useDomains, useRecords, useAddRecord, useUpdateRecord, useDeleteRecord } from '../hooks/useProvider';
import type { ProviderCredentials, DnsRecordInfo } from '../types/provider';
import { RECORD_TYPES } from '../types/dns';

export default function DnsManagePage() {
  const { t } = useTranslation();
  const { data: providers = [], isLoading: loadingProviders } = useProviders();

  const [credentials, setCredentials] = useState<ProviderCredentials | null>(null);
  const [selectedDomain, setSelectedDomain] = useState<string>('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingRecord, setEditingRecord] = useState<DnsRecordInfo | null>(null);

  const { data: domains = [], isLoading: loadingDomains, refetch: refetchDomains } = useDomains(credentials);
  const recordsRequest = credentials && selectedDomain ? { ...credentials, domain: selectedDomain } : null;
  const { data: records = [], isLoading: loadingRecords } = useRecords(recordsRequest);

  const addMutation = useAddRecord();
  const updateMutation = useUpdateRecord();
  const deleteMutation = useDeleteRecord();

  useEffect(() => {
    if (domains.length > 0 && !selectedDomain) {
      setSelectedDomain(domains[0]);
    }
  }, [domains, selectedDomain]);

  const handleConnect = () => {
    if (credentials?.providerName && credentials?.id && credentials?.secret) {
      refetchDomains();
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
      <div className="p-4 md:p-6 space-y-6">
      {/* Provider Configuration */}
      <div className="cyber-card cyber-corner p-5">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-1 h-5 bg-[var(--neon-cyan)]"></div>
          <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
            {t('dnsManage.providerConfig')}
          </h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-xs text-[var(--text-muted)] mb-1">{t('dnsManage.provider')}</label>
            <select
              className="cyber-input w-full"
              value={credentials?.providerName || ''}
              onChange={(e) => setCredentials(prev => ({ ...prev!, providerName: e.target.value }))}
              disabled={loadingProviders}
            >
              <option value="">{t('dnsManage.selectProvider')}</option>
              {providers.map((p) => (
                <option key={p.name} value={p.name}>{p.displayName}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs text-[var(--text-muted)] mb-1">{t('dnsManage.apiId')}</label>
            <input
              type="text"
              className="cyber-input w-full"
              placeholder="Access Key / API ID"
              value={credentials?.id || ''}
              onChange={(e) => setCredentials(prev => ({ ...prev!, id: e.target.value }))}
            />
          </div>

          <div>
            <label className="block text-xs text-[var(--text-muted)] mb-1">{t('dnsManage.apiSecret')}</label>
            <input
              type="password"
              className="cyber-input w-full"
              placeholder="Secret Key / API Token"
              value={credentials?.secret || ''}
              onChange={(e) => setCredentials(prev => ({ ...prev!, secret: e.target.value }))}
            />
          </div>

          <div className="flex items-end">
            <button
              className="cyber-btn cyber-btn-primary w-full"
              onClick={handleConnect}
              disabled={!credentials?.providerName || !credentials?.id || !credentials?.secret || loadingDomains}
            >
              {loadingDomains ? t('common.loading') : t('dnsManage.connect')}
            </button>
          </div>
        </div>
      </div>

      {/* Domain Selection */}
      {domains.length > 0 && (
        <div className="cyber-card cyber-corner p-5">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-1 h-5 bg-[var(--neon-green)]"></div>
            <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
              {t('dnsManage.domains')}
            </h2>
            <span className="text-xs text-[var(--text-muted)]">({domains.length})</span>
          </div>

          <div className="flex flex-wrap gap-2">
            {domains.map((domain) => (
              <button
                key={domain}
                className={`px-3 py-1.5 text-sm border transition-all ${
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
        <div className="cyber-card cyber-corner p-5">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="w-1 h-5 bg-[var(--neon-magenta)]"></div>
              <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                {t('dnsManage.records')} - {selectedDomain}
              </h2>
            </div>
            <button
              className="cyber-btn cyber-btn-primary text-sm"
              onClick={() => setShowAddModal(true)}
            >
              + {t('dnsManage.addRecord')}
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
            <div className="overflow-x-auto">
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
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="cyber-card p-6 w-full max-w-md">
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
