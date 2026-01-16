import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import MobileBottomNav from '../components/MobileBottomNav';
import { useUserProviderConfigs, useDomainsByConfig, useRecordsByConfig, useAddDnsRecord, useUpdateDnsRecord, useDeleteDnsRecord } from '../hooks/useUserProvider';
import type { DnsRecordInfo } from '../services/userProviderApi';

// 记录类型颜色映射
const RECORD_TYPE_COLORS: Record<string, string> = {
  A: 'var(--neon-cyan)',
  AAAA: 'var(--neon-blue)',
  CNAME: 'var(--neon-green)',
  MX: 'var(--neon-orange)',
  TXT: 'var(--neon-magenta)',
  NS: 'var(--neon-yellow)',
  SRV: '#9d4edd',
  CAA: '#ff6b6b',
};

// 常用记录类型
const COMMON_RECORD_TYPES = ['A', 'AAAA', 'CNAME', 'MX', 'TXT', 'NS', 'SRV', 'CAA'];

// 分页配置
const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

export default function HomePage() {
  const { t } = useTranslation();

  // 获取用户配置的服务商列表
  const { data: userConfigs = [], isLoading: isLoadingConfigs } = useUserProviderConfigs();
  const activeConfigs = userConfigs.filter(c => c.isActive);

  // 选中的服务商和域名
  const [selectedConfigId, setSelectedConfigId] = useState<string | null>(null);
  const [selectedDomain, setSelectedDomain] = useState<string | null>(null);

  // 获取域名列表
  const { data: domains = [], isLoading: isLoadingDomains } = useDomainsByConfig(selectedConfigId);

  // 获取 DNS 记录
  const { data: records = [], isLoading: isLoadingRecords } = useRecordsByConfig(
    selectedConfigId,
    selectedDomain
  );

  // Mutations
  const addRecordMutation = useAddDnsRecord();
  const updateRecordMutation = useUpdateDnsRecord();
  const deleteRecordMutation = useDeleteDnsRecord();

  // 模态框状态
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<DnsRecordInfo | null>(null);
  const [formData, setFormData] = useState({
    subDomain: '',
    recordType: 'A',
    value: '',
    ttl: 600,
  });
  const [formError, setFormError] = useState<string | null>(null);

  // 表格交互状态
  const [searchQuery, setSearchQuery] = useState('');
  const [filterType, setFilterType] = useState<string>('all');
  const [sortField, setSortField] = useState<'subDomain' | 'recordType' | 'value' | 'ttl'>('subDomain');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  // 获取所有记录类型（用于筛选）
  const recordTypes = useMemo(() => {
    const types = new Set(records.map(r => r.recordType));
    return Array.from(types).sort();
  }, [records]);

  // 过滤和排序后的记录
  const filteredRecords = useMemo(() => {
    let result = [...records];

    // 搜索过滤
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      result = result.filter(r =>
        r.subDomain?.toLowerCase().includes(query) ||
        r.value.toLowerCase().includes(query) ||
        r.recordType.toLowerCase().includes(query)
      );
    }

    // 类型过滤
    if (filterType !== 'all') {
      result = result.filter(r => r.recordType === filterType);
    }

    // 排序
    result.sort((a, b) => {
      let aVal: string | number = '';
      let bVal: string | number = '';

      switch (sortField) {
        case 'subDomain':
          aVal = a.subDomain || '@';
          bVal = b.subDomain || '@';
          break;
        case 'recordType':
          aVal = a.recordType;
          bVal = b.recordType;
          break;
        case 'value':
          aVal = a.value;
          bVal = b.value;
          break;
        case 'ttl':
          aVal = a.ttl;
          bVal = b.ttl;
          break;
      }

      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return sortOrder === 'asc' ? aVal - bVal : bVal - aVal;
      }
      return sortOrder === 'asc'
        ? String(aVal).localeCompare(String(bVal))
        : String(bVal).localeCompare(String(aVal));
    });

    return result;
  }, [records, searchQuery, filterType, sortField, sortOrder]);

  // 分页后的记录
  const paginatedRecords = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return filteredRecords.slice(start, start + pageSize);
  }, [filteredRecords, currentPage, pageSize]);

  // 总页数
  const totalPages = Math.ceil(filteredRecords.length / pageSize);

  // 重置分页当过滤条件变化时
  useEffect(() => {
    setCurrentPage(1);
  }, [searchQuery, filterType, selectedDomain]);

  // 当配置列表加载完成后，自动选择第一个
  useEffect(() => {
    if (activeConfigs.length > 0 && !selectedConfigId) {
      setSelectedConfigId(activeConfigs[0].id);
    }
  }, [activeConfigs, selectedConfigId]);

  // 当域名列表加载完成后，自动选择第一个
  useEffect(() => {
    if (domains.length > 0 && !selectedDomain) {
      setSelectedDomain(domains[0]);
    }
  }, [domains, selectedDomain]);

  // 切换服务商时重置域名选择和过滤条件
  const handleConfigChange = (configId: string) => {
    setSelectedConfigId(configId);
    setSelectedDomain(null);
    setSearchQuery('');
    setFilterType('all');
    setCurrentPage(1);
  };

  // 切换排序
  const handleSort = (field: 'subDomain' | 'recordType' | 'value' | 'ttl') => {
    if (sortField === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortOrder('asc');
    }
  };

  // 复制到剪贴板
  const handleCopy = async (text: string, id: string) => {
    try {
      await navigator.clipboard.writeText(text);
      setCopiedId(id);
      setTimeout(() => setCopiedId(null), 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  };

  // 打开添加记录模态框
  const handleOpenAddModal = () => {
    setEditingRecord(null);
    setFormData({ subDomain: '', recordType: 'A', value: '', ttl: 600 });
    setFormError(null);
    setIsModalOpen(true);
  };

  // 打开编辑记录模态框
  const handleOpenEditModal = (record: DnsRecordInfo) => {
    setEditingRecord(record);
    setFormData({
      subDomain: record.subDomain || '',
      recordType: record.recordType,
      value: record.value,
      ttl: record.ttl,
    });
    setFormError(null);
    setIsModalOpen(true);
  };

  // 关闭模态框
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingRecord(null);
    setFormError(null);
  };

  // 提交表单
  const handleSubmit = async () => {
    if (!selectedConfigId || !selectedDomain) return;
    if (!formData.value.trim()) {
      setFormError(t('home.recordValuePlaceholder'));
      return;
    }

    try {
      if (editingRecord) {
        await updateRecordMutation.mutateAsync({
          configId: selectedConfigId,
          domain: selectedDomain,
          recordId: editingRecord.recordId,
          request: { value: formData.value, ttl: formData.ttl },
        });
      } else {
        await addRecordMutation.mutateAsync({
          configId: selectedConfigId,
          domain: selectedDomain,
          request: {
            subDomain: formData.subDomain || '@',
            recordType: formData.recordType,
            value: formData.value,
            ttl: formData.ttl,
          },
        });
      }
      handleCloseModal();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Failed');
    }
  };

  // 删除记录
  const handleDelete = async (record: DnsRecordInfo) => {
    if (!selectedConfigId || !selectedDomain) return;
    if (!confirm(t('home.confirmDelete'))) return;

    try {
      await deleteRecordMutation.mutateAsync({
        configId: selectedConfigId,
        domain: selectedDomain,
        recordId: record.recordId,
      });
    } catch (err) {
      console.error('Failed to delete:', err);
    }
  };

  // 获取当前选中的服务商信息
  const selectedConfig = activeConfigs.find(c => c.id === selectedConfigId);

  // 移动端标签页
  const [mobileActiveTab, setMobileActiveTab] = useState<'query' | 'providers' | 'results'>('query');

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

  // 渲染服务商选择器
  const renderProviderSelector = () => (
    <div className="flex flex-wrap gap-2">
      {activeConfigs.map(config => (
        <button
          key={config.id}
          onClick={() => handleConfigChange(config.id)}
          className={`px-3 py-1.5 text-xs border transition-all ${
            selectedConfigId === config.id
              ? 'bg-[var(--neon-cyan)] text-[var(--bg-primary)] border-[var(--neon-cyan)]'
              : 'bg-transparent text-[var(--text-secondary)] border-[var(--border-color)] hover:border-[var(--neon-cyan)] hover:text-[var(--neon-cyan)]'
          }`}
        >
          {config.displayName}
        </button>
      ))}
    </div>
  );

  // 渲染域名列表
  const renderDomainList = () => {
    if (isLoadingDomains) {
      return (
        <div className="py-4">
          <div className="loading-bar mb-2"></div>
          <p className="text-center text-[var(--text-muted)] text-xs">{t('common.loading')}</p>
        </div>
      );
    }

    if (domains.length === 0) {
      return (
        <div className="py-4 text-center text-[var(--text-muted)] text-xs">
          {t('dnsManage.domains')}: 0
        </div>
      );
    }

    return (
      <div className="flex flex-wrap gap-2">
        {domains.map(domain => (
          <button
            key={domain}
            onClick={() => setSelectedDomain(domain)}
            className={`px-3 py-1.5 text-xs border transition-all ${
              selectedDomain === domain
                ? 'bg-[var(--neon-green)] text-[var(--bg-primary)] border-[var(--neon-green)]'
                : 'bg-transparent text-[var(--text-secondary)] border-[var(--border-color)] hover:border-[var(--neon-green)] hover:text-[var(--neon-green)]'
            }`}
          >
            {domain}
          </button>
        ))}
      </div>
    );
  };

  // 渲染 DNS 记录表格
  const renderRecordsTable = () => {
    if (!selectedDomain) {
      return (
        <div className="py-8 text-center">
          <div className="w-12 h-12 mx-auto mb-3 border-2 border-dashed border-[var(--border-color)] flex items-center justify-center">
            <svg className="w-6 h-6 text-[var(--text-muted)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
            </svg>
          </div>
          <p className="text-sm text-[var(--text-secondary)]">{t('home.selectDomain')}</p>
        </div>
      );
    }

    if (isLoadingRecords) {
      return (
        <div className="py-8">
          <div className="loading-bar mb-4"></div>
          <p className="text-center text-[var(--text-muted)] text-sm">{t('common.loading')}</p>
        </div>
      );
    }

    if (records.length === 0) {
      return (
        <div className="py-8 text-center">
          <p className="text-sm text-[var(--text-muted)] mb-4">{t('dnsManage.noRecords')}</p>
          <button
            onClick={handleOpenAddModal}
            className="inline-flex items-center gap-2 px-4 py-2 bg-[var(--neon-cyan-dim)] border border-[var(--neon-cyan)] text-[var(--neon-cyan)] text-sm hover:bg-[var(--neon-cyan)] hover:text-[var(--bg-primary)] transition-all"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            {t('home.addRecord')}
          </button>
        </div>
      );
    }

    // 排序图标组件
    const SortIcon = ({ field }: { field: string }) => (
      <span className="ml-1">
        {sortField === field ? (sortOrder === 'asc' ? '↑' : '↓') : ''}
      </span>
    );

    return (
      <div className="flex flex-col h-full">
        {/* 工具栏 */}
        <div className="flex items-center gap-3 mb-3 pb-3 border-b border-[var(--border-color)]">
          {/* 搜索框 */}
          <div className="relative flex-1 max-w-md">
            <input
              type="text"
              placeholder={t('home.searchPlaceholder')}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="cyber-input w-full pl-9 py-2 text-sm"
            />
            <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--text-muted)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>

          {/* 统计信息 */}
          <div className="text-xs text-[var(--text-muted)] hidden md:block whitespace-nowrap">
            {filteredRecords.length !== records.length
              ? `${t('home.filtered')}: ${filteredRecords.length} / ${records.length}`
              : `${t('home.total')}: ${records.length}`}
          </div>

          {/* 类型筛选 */}
          <select
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
            className="cyber-input text-sm py-2 px-3"
            style={{ width: '120px' }}
          >
            <option value="all">{t('home.allTypes')}</option>
            {recordTypes.map(type => (<option key={type} value={type}>{type}</option>))}
          </select>

          {/* 每页数量 */}
          <select
            value={pageSize}
            onChange={(e) => setPageSize(Number(e.target.value))}
            className="cyber-input text-sm py-2 px-3"
            style={{ width: '80px' }}
          >
            {PAGE_SIZE_OPTIONS.map(size => (<option key={size} value={size}>{size}</option>))}
          </select>

          {/* 添加记录按钮 */}
          <button
            onClick={handleOpenAddModal}
            className="flex items-center gap-1.5 px-3 py-2 bg-[var(--neon-cyan-dim)] border border-[var(--neon-cyan)] text-[var(--neon-cyan)] text-sm hover:bg-[var(--neon-cyan)] hover:text-[var(--bg-primary)] transition-all whitespace-nowrap"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            <span className="hidden sm:inline">{t('home.addRecord')}</span>
          </button>
        </div>

        {/* Desktop Table */}
        <div className="hidden lg:block flex-1 overflow-auto">
          <table className="w-full text-sm">
            <thead className="sticky top-0 bg-[var(--bg-secondary)]">
              <tr className="border-b border-[var(--border-color)]">
                <th className="text-left py-2 px-3 text-[var(--text-muted)] font-medium text-xs uppercase tracking-wider cursor-pointer hover:text-[var(--neon-cyan)]" onClick={() => handleSort('subDomain')}>
                  {t('dnsManage.subDomain')}<SortIcon field="subDomain" />
                </th>
                <th className="text-left py-2 px-3 text-[var(--text-muted)] font-medium text-xs uppercase tracking-wider cursor-pointer hover:text-[var(--neon-cyan)]" onClick={() => handleSort('recordType')}>
                  {t('dnsManage.type')}<SortIcon field="recordType" />
                </th>
                <th className="text-left py-2 px-3 text-[var(--text-muted)] font-medium text-xs uppercase tracking-wider cursor-pointer hover:text-[var(--neon-cyan)]" onClick={() => handleSort('value')}>
                  {t('dnsManage.value')}<SortIcon field="value" />
                </th>
                <th className="text-left py-2 px-3 text-[var(--text-muted)] font-medium text-xs uppercase tracking-wider cursor-pointer hover:text-[var(--neon-cyan)] w-20" onClick={() => handleSort('ttl')}>
                  TTL<SortIcon field="ttl" />
                </th>
                <th className="w-24 text-right pr-3 text-[var(--text-muted)] font-medium text-xs uppercase tracking-wider">{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {paginatedRecords.map((record, index) => (
                <tr key={record.recordId || index} className="border-b border-[var(--border-color)] hover:bg-[var(--bg-tertiary)] transition-colors group">
                  <td className="py-2 px-3 font-mono text-[var(--neon-cyan)]">{record.subDomain || '@'}</td>
                  <td className="py-2 px-3">
                    <span className="px-2 py-0.5 border text-xs" style={{ borderColor: RECORD_TYPE_COLORS[record.recordType] || 'var(--border-color)', color: RECORD_TYPE_COLORS[record.recordType] || 'var(--text-secondary)' }}>
                      {record.recordType}
                    </span>
                  </td>
                  <td className="py-2 px-3 font-mono text-[var(--text-primary)] max-w-xs"><div className="truncate" title={record.value}>{record.value}</div></td>
                  <td className="py-2 px-3 text-[var(--text-muted)]">{record.ttl}s</td>
                  <td className="py-2 px-2 text-right">
                    <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-all">
                      <button onClick={() => handleCopy(record.value, record.recordId)} className="p-1.5 hover:text-[var(--neon-cyan)] transition-colors" title={t('home.copyValue')}>
                        {copiedId === record.recordId ? (
                          <svg className="w-4 h-4 text-[var(--neon-green)]" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" /></svg>
                        ) : (
                          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" /></svg>
                        )}
                      </button>
                      <button onClick={() => handleOpenEditModal(record)} className="p-1.5 hover:text-[var(--neon-orange)] transition-colors" title={t('home.editRecord')}>
                        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
                      </button>
                      <button onClick={() => handleDelete(record)} className="p-1.5 hover:text-[var(--neon-red)] transition-colors" title={t('home.deleteRecord')}>
                        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Mobile Cards */}
        <div className="lg:hidden flex-1 overflow-auto space-y-2">
          {paginatedRecords.map((record, index) => (
            <div key={record.recordId || index} className="p-3 border border-[var(--border-color)] bg-[var(--bg-tertiary)]">
              <div className="flex items-center justify-between mb-2">
                <span className="font-mono text-[var(--neon-cyan)] text-sm">{record.subDomain || '@'}</span>
                <span className="px-2 py-0.5 border text-xs" style={{ borderColor: RECORD_TYPE_COLORS[record.recordType] || 'var(--border-color)', color: RECORD_TYPE_COLORS[record.recordType] || 'var(--text-secondary)' }}>
                  {record.recordType}
                </span>
              </div>
              <div className="font-mono text-[var(--text-primary)] text-xs break-all mb-2 cursor-pointer hover:text-[var(--neon-cyan)]" onClick={() => handleCopy(record.value, record.recordId)}>
                {record.value}{copiedId === record.recordId && <span className="ml-2 text-[var(--neon-green)]">✓</span>}
              </div>
              <div className="flex items-center justify-between">
                <div className="text-[var(--text-muted)] text-xs">TTL: {record.ttl}s</div>
                <div className="flex items-center gap-2">
                  <button onClick={() => handleOpenEditModal(record)} className="p-1 text-[var(--text-muted)] hover:text-[var(--neon-orange)]">
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
                  </button>
                  <button onClick={() => handleDelete(record)} className="p-1 text-[var(--text-muted)] hover:text-[var(--neon-red)]">
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* 分页 */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between pt-3 mt-auto border-t border-[var(--border-color)]">
            <div className="text-xs text-[var(--text-muted)]">{t('home.page')} {currentPage} / {totalPages}</div>
            <div className="flex gap-1">
              <button onClick={() => setCurrentPage(1)} disabled={currentPage === 1} className="cyber-btn text-xs px-2 py-1 disabled:opacity-30">«</button>
              <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))} disabled={currentPage === 1} className="cyber-btn text-xs px-2 py-1 disabled:opacity-30">‹</button>
              <button onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))} disabled={currentPage === totalPages} className="cyber-btn text-xs px-2 py-1 disabled:opacity-30">›</button>
              <button onClick={() => setCurrentPage(totalPages)} disabled={currentPage === totalPages} className="cyber-btn text-xs px-2 py-1 disabled:opacity-30">»</button>
            </div>
          </div>
        )}
      </div>
    );
  };

  // 统计信息
  const stats = {
    providers: activeConfigs.length,
    domains: domains.length,
    records: records.length,
  };

  return (
    <Layout>
      <div className="p-4 md:p-6 h-full flex flex-col">
        {/* Desktop Layout */}
        <div className="hidden md:flex md:flex-col md:flex-1 md:min-h-0 md:gap-4 lg:gap-6">
          {/* Stats & Provider Selection */}
          <div className="grid grid-cols-1 xl:grid-cols-4 gap-4 lg:gap-6 flex-shrink-0">
            {/* Stats */}
            <div className="cyber-card cyber-corner p-4">
              <div className="flex items-center gap-3 mb-3">
                <div className="w-1 h-5 bg-[var(--neon-green)]"></div>
                <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                  {t('results.stats')}
                </h2>
              </div>
              <div className="grid grid-cols-3 gap-2">
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-xl font-bold text-[var(--neon-cyan)] font-mono">
                    {stats.providers}
                  </div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase tracking-wider mt-1">
                    {t('providers.title')}
                  </div>
                </div>
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-xl font-bold text-[var(--neon-green)] font-mono">
                    {stats.domains}
                  </div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase tracking-wider mt-1">
                    {t('dnsManage.domains')}
                  </div>
                </div>
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-xl font-bold text-[var(--neon-magenta)] font-mono">
                    {stats.records}
                  </div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase tracking-wider mt-1">
                    {t('dnsManage.records')}
                  </div>
                </div>
              </div>
            </div>

            {/* Provider Selection */}
            <div className="xl:col-span-3 cyber-card cyber-corner p-4">
              <div className="flex items-center gap-3 mb-3">
                <div className="w-1 h-5 bg-[var(--neon-cyan)]"></div>
                <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                  {t('providers.title')}
                </h2>
                {selectedConfig && (
                  <span className="text-xs text-[var(--neon-cyan)] ml-auto">
                    {selectedConfig.displayName}
                  </span>
                )}
              </div>
              {isLoadingConfigs ? (
                <div className="py-2">
                  <div className="loading-bar mb-2"></div>
                  <p className="text-center text-[var(--text-muted)] text-xs">{t('common.loading')}</p>
                </div>
              ) : activeConfigs.length === 0 ? (
                renderNoProvidersHint()
              ) : (
                renderProviderSelector()
              )}
            </div>
          </div>

          {/* Domain Selection */}
          {selectedConfigId && (
            <div className="cyber-card cyber-corner p-4 flex-shrink-0">
              <div className="flex items-center gap-3 mb-3">
                <div className="w-1 h-5 bg-[var(--neon-green)]"></div>
                <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                  {t('dnsManage.domains')}
                </h2>
                {selectedDomain && (
                  <span className="text-xs text-[var(--neon-green)] ml-auto">
                    {selectedDomain}
                  </span>
                )}
              </div>
              {renderDomainList()}
            </div>
          )}

          {/* DNS Records */}
          <div className="cyber-card cyber-corner p-4 lg:p-5 flex-1 min-h-0 flex flex-col">
            <div className="flex items-center gap-3 mb-3 flex-shrink-0">
              <div className="w-1 h-5 bg-[var(--neon-magenta)]"></div>
              <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                {t('dnsManage.records')}
              </h2>
              {records.length > 0 && (
                <span className="text-xs text-[var(--text-muted)] ml-auto">
                  {records.length} {t('dnsManage.records')}
                </span>
              )}
            </div>
            <div className="flex-1 min-h-0 overflow-y-auto">
              {renderRecordsTable()}
            </div>
          </div>
        </div>

        {/* Mobile Layout */}
        <div className="md:hidden flex-1 overflow-y-auto">
          {/* Providers Tab */}
          {mobileActiveTab === 'query' && (
            <div className="space-y-4 fade-in">
              {/* Quick Stats */}
              <div className="grid grid-cols-3 gap-2">
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-lg font-bold text-[var(--neon-cyan)] font-mono">{stats.providers}</div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase">{t('providers.title')}</div>
                </div>
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-lg font-bold text-[var(--neon-green)] font-mono">{stats.domains}</div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase">{t('dnsManage.domains')}</div>
                </div>
                <div className="p-2 border border-[var(--border-color)] bg-[var(--bg-tertiary)] text-center">
                  <div className="text-lg font-bold text-[var(--neon-magenta)] font-mono">{stats.records}</div>
                  <div className="text-[8px] text-[var(--text-muted)] uppercase">{t('dnsManage.records')}</div>
                </div>
              </div>

              {/* Provider Selection */}
              <div className="cyber-card p-4">
                <div className="flex items-center gap-2 mb-3">
                  <div className="w-1 h-4 bg-[var(--neon-cyan)]"></div>
                  <h2 className="display text-[10px] font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                    {t('providers.title')}
                  </h2>
                </div>
                {isLoadingConfigs ? (
                  <div className="py-4">
                    <div className="loading-bar mb-2"></div>
                    <p className="text-center text-[var(--text-muted)] text-xs">{t('common.loading')}</p>
                  </div>
                ) : activeConfigs.length === 0 ? (
                  renderNoProvidersHint()
                ) : (
                  renderProviderSelector()
                )}
              </div>

              {/* Domain Selection */}
              {selectedConfigId && (
                <div className="cyber-card p-4">
                  <div className="flex items-center gap-2 mb-3">
                    <div className="w-1 h-4 bg-[var(--neon-green)]"></div>
                    <h2 className="display text-[10px] font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                      {t('dnsManage.domains')}
                    </h2>
                  </div>
                  {renderDomainList()}
                </div>
              )}
            </div>
          )}

          {/* Providers Tab (for mobile navigation) */}
          {mobileActiveTab === 'providers' && (
            <div className="cyber-card p-4 fade-in">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="w-1 h-4 bg-[var(--neon-cyan)]"></div>
                  <h2 className="display text-[10px] font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                    {t('providers.title')}
                  </h2>
                </div>
                <span className="text-[10px] text-[var(--text-muted)]">
                  {activeConfigs.length}
                </span>
              </div>
              {isLoadingConfigs ? (
                <div className="py-6">
                  <div className="loading-bar mb-3"></div>
                  <p className="text-center text-[var(--text-muted)] text-sm">{t('common.loading')}</p>
                </div>
              ) : activeConfigs.length === 0 ? (
                renderNoProvidersHint()
              ) : (
                <div className="space-y-2">
                  {activeConfigs.map(config => (
                    <button
                      key={config.id}
                      onClick={() => {
                        handleConfigChange(config.id);
                        setMobileActiveTab('results');
                      }}
                      className={`w-full p-3 text-left border transition-all ${
                        selectedConfigId === config.id
                          ? 'bg-[var(--neon-cyan-dim)] border-[var(--neon-cyan)] text-[var(--neon-cyan)]'
                          : 'bg-transparent border-[var(--border-color)] text-[var(--text-secondary)] hover:border-[var(--neon-cyan)]'
                      }`}
                    >
                      <div className="font-medium text-sm">{config.displayName}</div>
                      <div className="text-xs text-[var(--text-muted)] mt-1">{config.providerName}</div>
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Results Tab */}
          {mobileActiveTab === 'results' && (
            <div className="cyber-card p-4 fade-in">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="w-1 h-4 bg-[var(--neon-magenta)]"></div>
                  <h2 className="display text-[10px] font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                    {t('dnsManage.records')}
                  </h2>
                </div>
                {records.length > 0 && (
                  <span className="text-[10px] text-[var(--neon-magenta)]">
                    {records.length}
                  </span>
                )}
              </div>
              {selectedDomain && (
                <div className="mb-3 text-xs text-[var(--text-muted)]">
                  {selectedConfig?.displayName} / {selectedDomain}
                </div>
              )}
              {renderRecordsTable()}
            </div>
          )}
        </div>
      </div>

      {/* Mobile Bottom Navigation */}
      <MobileBottomNav
        activeTab={mobileActiveTab}
        onTabChange={setMobileActiveTab}
      />

      {/* 添加/编辑记录模态框 */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="cyber-card cyber-corner w-full max-w-md p-6 relative">
            {/* 关闭按钮 */}
            <button
              onClick={handleCloseModal}
              className="absolute top-4 right-4 p-1 text-[var(--text-muted)] hover:text-[var(--text-primary)] transition-colors"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            {/* 标题 */}
            <div className="flex items-center gap-3 mb-6">
              <div className="w-1 h-6 bg-[var(--neon-cyan)]"></div>
              <h2 className="text-lg font-semibold text-[var(--text-primary)]">
                {editingRecord ? t('home.editRecord') : t('home.addRecord')}
              </h2>
            </div>

            {/* 表单 */}
            <div className="space-y-4">
              {/* 子域名 - 仅添加时显示 */}
              {!editingRecord && (
                <div>
                  <label className="block text-xs text-[var(--text-muted)] mb-1.5 uppercase tracking-wider">
                    {t('home.subDomain')}
                  </label>
                  <input
                    type="text"
                    value={formData.subDomain}
                    onChange={(e) => setFormData(prev => ({ ...prev, subDomain: e.target.value }))}
                    placeholder={t('home.subDomainPlaceholder')}
                    className="cyber-input w-full"
                  />
                </div>
              )}

              {/* 记录类型 - 仅添加时显示 */}
              {!editingRecord && (
                <div>
                  <label className="block text-xs text-[var(--text-muted)] mb-1.5 uppercase tracking-wider">
                    {t('home.recordType')}
                  </label>
                  <select
                    value={formData.recordType}
                    onChange={(e) => setFormData(prev => ({ ...prev, recordType: e.target.value }))}
                    className="cyber-input w-full"
                  >
                    {COMMON_RECORD_TYPES.map(type => (
                      <option key={type} value={type}>{type}</option>
                    ))}
                  </select>
                </div>
              )}

              {/* 编辑时显示只读信息 */}
              {editingRecord && (
                <div className="flex gap-4 p-3 bg-[var(--bg-tertiary)] border border-[var(--border-color)]">
                  <div>
                    <span className="text-xs text-[var(--text-muted)]">{t('home.subDomain')}: </span>
                    <span className="font-mono text-[var(--neon-cyan)]">{editingRecord.subDomain || '@'}</span>
                  </div>
                  <div>
                    <span className="text-xs text-[var(--text-muted)]">{t('home.recordType')}: </span>
                    <span className="font-mono text-[var(--neon-green)]">{editingRecord.recordType}</span>
                  </div>
                </div>
              )}

              {/* 记录值 */}
              <div>
                <label className="block text-xs text-[var(--text-muted)] mb-1.5 uppercase tracking-wider">
                  {t('home.recordValue')}
                </label>
                <input
                  type="text"
                  value={formData.value}
                  onChange={(e) => setFormData(prev => ({ ...prev, value: e.target.value }))}
                  placeholder={t('home.recordValuePlaceholder')}
                  className="cyber-input w-full"
                />
              </div>

              {/* TTL */}
              <div>
                <label className="block text-xs text-[var(--text-muted)] mb-1.5 uppercase tracking-wider">
                  {t('home.ttl')}
                </label>
                <input
                  type="number"
                  value={formData.ttl}
                  onChange={(e) => setFormData(prev => ({ ...prev, ttl: parseInt(e.target.value) || 600 }))}
                  placeholder={t('home.ttlPlaceholder')}
                  className="cyber-input w-full"
                  min={1}
                />
              </div>

              {/* 错误提示 */}
              {formError && (
                <div className="p-3 bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
                  {formError}
                </div>
              )}

              {/* 按钮 */}
              <div className="flex gap-3 pt-2">
                <button
                  onClick={handleCloseModal}
                  className="flex-1 cyber-btn py-2.5"
                >
                  {t('common.cancel')}
                </button>
                <button
                  onClick={handleSubmit}
                  disabled={addRecordMutation.isPending || updateRecordMutation.isPending}
                  className="flex-1 py-2.5 bg-[var(--neon-cyan)] text-[var(--bg-primary)] font-medium hover:bg-[var(--neon-cyan-bright)] transition-colors disabled:opacity-50"
                >
                  {(addRecordMutation.isPending || updateRecordMutation.isPending) ? t('common.loading') : t('common.save')}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}
