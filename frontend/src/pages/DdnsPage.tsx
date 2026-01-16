import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import Layout from '../components/Layout';
import { useProviders } from '../hooks/useProvider';
import {
  useCurrentIp,
  useDdnsTasks,
  useCreateDdnsTask,
  useDeleteDdnsTask,
  useToggleDdnsTask,
  useIpSources,
} from '../hooks/useDdns';
import type { CreateDdnsTaskRequest, DdnsTask } from '../types/ddns';

export default function DdnsPage() {
  const { t } = useTranslation();
  const { data: ipSources = [] } = useIpSources();
  const [selectedSource, setSelectedSource] = useState<string | undefined>(() => {
    return localStorage.getItem('ddns_ip_source') || undefined;
  });
  const { data: currentIp, isLoading: loadingIp, refetch: refetchIp } = useCurrentIp(selectedSource);
  const { data: tasks = [], isLoading: loadingTasks } = useDdnsTasks();
  const { data: providers = [] } = useProviders();

  const createMutation = useCreateDdnsTask();
  const deleteMutation = useDeleteDdnsTask();
  const toggleMutation = useToggleDdnsTask();

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newTask, setNewTask] = useState<Partial<CreateDdnsTaskRequest>>({
    ttl: 600,
    intervalMinutes: 5,
  });

  useEffect(() => {
    if (selectedSource) {
      localStorage.setItem('ddns_ip_source', selectedSource);
    } else {
      localStorage.removeItem('ddns_ip_source');
    }
  }, [selectedSource]);

  const handleSourceChange = (source: string) => {
    setSelectedSource(source || undefined);
  };

  const handleCreate = async () => {
    if (!newTask.name || !newTask.providerName || !newTask.providerId || !newTask.domain || !newTask.recordId) {
      alert(t('ddns.fillRequired'));
      return;
    }
    try {
      await createMutation.mutateAsync(newTask as CreateDdnsTaskRequest);
      setShowCreateModal(false);
      setNewTask({ ttl: 600, intervalMinutes: 5 });
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const handleDelete = async (taskId: string) => {
    if (!confirm(t('ddns.confirmDelete'))) return;
    try {
      await deleteMutation.mutateAsync(taskId);
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const handleToggle = async (task: DdnsTask) => {
    try {
      await toggleMutation.mutateAsync({ taskId: task.id, enabled: !task.enabled });
    } catch (error) {
      alert(`${t('common.error')}: ${(error as Error).message}`);
    }
  };

  const formatTime = (dateStr?: string) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString('zh-CN');
  };

  return (
    <Layout>
      <div className="p-4 md:p-6 space-y-4 md:space-y-6">
        {/* Current IP Card */}
        <div className="cyber-card cyber-corner p-4 md:p-5">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="w-1 h-5 bg-[var(--neon-cyan)]"></div>
              <h2 className="display text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                {t('ddns.currentIp')}
              </h2>
            </div>
            <button
              onClick={() => refetchIp()}
              className="cyber-btn text-xs px-3 py-1"
              disabled={loadingIp}
            >
              {loadingIp ? t('common.loading') : t('common.refresh')}
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-[var(--bg-tertiary)] p-4 border border-[var(--border-color)]">
              <div className="text-xs text-[var(--text-muted)] mb-1">{t('ddns.publicIp')}</div>
              <div className="text-xl font-mono text-[var(--neon-cyan)]">
                {loadingIp ? '...' : currentIp?.ip || '-'}
              </div>
            </div>
            <div className="bg-[var(--bg-tertiary)] p-4 border border-[var(--border-color)]">
              <div className="text-xs text-[var(--text-muted)] mb-1">{t('ddns.ipSource')}</div>
              <select
                value={selectedSource || ''}
                onChange={(e) => handleSourceChange(e.target.value)}
                className="w-full bg-transparent text-sm text-[var(--text-secondary)] border-none outline-none cursor-pointer"
              >
                <option value="">{t('ddns.autoSelect')}</option>
                {ipSources.map((source) => (
                  <option key={source.id} value={source.id}>
                    {source.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="bg-[var(--bg-tertiary)] p-4 border border-[var(--border-color)]">
              <div className="text-xs text-[var(--text-muted)] mb-1">{t('ddns.currentSource')}</div>
              <div className="text-sm text-[var(--text-secondary)]">
                {loadingIp ? '...' : currentIp?.source || '-'}
              </div>
            </div>
          </div>
        </div>

        {/* Tasks List */}
        <div className="cyber-card cyber-corner p-4 md:p-5">
          <div className="flex items-center justify-between mb-4 gap-2">
            <div className="flex items-center gap-2 md:gap-3">
              <div className="w-1 h-5 bg-[var(--neon-magenta)] flex-shrink-0"></div>
              <h2 className="display text-[10px] md:text-xs font-semibold tracking-widest text-[var(--text-primary)] uppercase">
                {t('ddns.tasks')}
              </h2>
              <span className="text-[10px] md:text-xs text-[var(--text-muted)]">({tasks.length})</span>
            </div>
            <button
              onClick={() => setShowCreateModal(true)}
              className="cyber-btn cyber-btn-primary text-xs px-3 md:px-4 py-2 flex-shrink-0"
            >
              <span className="hidden sm:inline">+ {t('ddns.addTask')}</span>
              <span className="sm:hidden">+</span>
            </button>
          </div>

          {loadingTasks ? (
            <div className="py-8 text-center text-[var(--text-muted)]">{t('common.loading')}</div>
          ) : tasks.length === 0 ? (
            <div className="py-8 text-center text-[var(--text-muted)]">{t('ddns.noTasks')}</div>
          ) : (
            <>
              {/* Desktop Table */}
              <div className="hidden lg:block overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-[var(--border-color)]">
                      <th className="text-left py-3 px-2 text-xs text-[var(--text-muted)] font-medium">{t('ddns.taskName')}</th>
                      <th className="text-left py-3 px-2 text-xs text-[var(--text-muted)] font-medium">{t('ddns.provider')}</th>
                      <th className="text-left py-3 px-2 text-xs text-[var(--text-muted)] font-medium">{t('ddns.domain')}</th>
                      <th className="text-left py-3 px-2 text-xs text-[var(--text-muted)] font-medium">{t('ddns.lastIp')}</th>
                      <th className="text-left py-3 px-2 text-xs text-[var(--text-muted)] font-medium">{t('ddns.lastUpdate')}</th>
                      <th className="text-left py-3 px-2 text-xs text-[var(--text-muted)] font-medium">{t('ddns.status')}</th>
                      <th className="text-right py-3 px-2 text-xs text-[var(--text-muted)] font-medium">{t('common.actions')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {tasks.map((task) => (
                      <tr key={task.id} className="border-b border-[var(--border-color)] hover:bg-[var(--bg-tertiary)]">
                        <td className="py-3 px-2">
                          <div className="font-medium text-[var(--text-primary)]">{task.name}</div>
                          <div className="text-xs text-[var(--text-muted)]">
                            {t('ddns.interval')}: {task.intervalMinutes} {t('ddns.minutes')}
                          </div>
                        </td>
                        <td className="py-3 px-2 text-[var(--text-secondary)]">{task.providerName}</td>
                        <td className="py-3 px-2">
                          <div className="font-mono text-[var(--text-primary)]">
                            {task.subDomain ? `${task.subDomain}.` : ''}{task.domain}
                          </div>
                        </td>
                        <td className="py-3 px-2 font-mono text-[var(--neon-cyan)]">
                          {task.lastKnownIp || '-'}
                        </td>
                        <td className="py-3 px-2 text-xs text-[var(--text-muted)]">
                          {formatTime(task.lastUpdateTime)}
                          {task.lastError && (
                            <div className="text-[var(--neon-red)] mt-1" title={task.lastError}>
                              ⚠ {t('ddns.hasError')}
                            </div>
                          )}
                        </td>
                        <td className="py-3 px-2">
                          <button
                            onClick={() => handleToggle(task)}
                            className={`px-2 py-1 text-xs border ${
                              task.enabled
                                ? 'border-[var(--neon-green)] text-[var(--neon-green)]'
                                : 'border-[var(--text-muted)] text-[var(--text-muted)]'
                            }`}
                            disabled={toggleMutation.isPending}
                          >
                            {task.enabled ? t('ddns.enabled') : t('ddns.disabled')}
                          </button>
                        </td>
                        <td className="py-3 px-2 text-right">
                          <button
                            onClick={() => handleDelete(task.id)}
                            className="text-[var(--neon-red)] hover:underline text-xs"
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

              {/* Mobile/Tablet Card List */}
              <div className="lg:hidden space-y-3">
                {tasks.map((task) => (
                  <div key={task.id} className="bg-[var(--bg-tertiary)] border border-[var(--border-color)] p-3">
                    <div className="flex items-start justify-between mb-2">
                      <div className="min-w-0 flex-1">
                        <div className="font-medium text-[var(--text-primary)] text-sm truncate">{task.name}</div>
                        <div className="text-[10px] text-[var(--text-muted)]">
                          {task.providerName} · {t('ddns.interval')}: {task.intervalMinutes}{t('ddns.minutes')}
                        </div>
                      </div>
                      <button
                        onClick={() => handleToggle(task)}
                        className={`px-2 py-1 text-[10px] border flex-shrink-0 ml-2 ${
                          task.enabled
                            ? 'border-[var(--neon-green)] text-[var(--neon-green)]'
                            : 'border-[var(--text-muted)] text-[var(--text-muted)]'
                        }`}
                        disabled={toggleMutation.isPending}
                      >
                        {task.enabled ? t('ddns.enabled') : t('ddns.disabled')}
                      </button>
                    </div>

                    <div className="grid grid-cols-2 gap-2 mb-3 text-xs">
                      <div>
                        <div className="text-[10px] text-[var(--text-muted)] uppercase">{t('ddns.domain')}</div>
                        <div className="font-mono text-[var(--text-primary)] truncate">
                          {task.subDomain ? `${task.subDomain}.` : ''}{task.domain}
                        </div>
                      </div>
                      <div>
                        <div className="text-[10px] text-[var(--text-muted)] uppercase">{t('ddns.lastIp')}</div>
                        <div className="font-mono text-[var(--neon-cyan)]">{task.lastKnownIp || '-'}</div>
                      </div>
                    </div>

                    <div className="flex items-center justify-between text-[10px]">
                      <div className="text-[var(--text-muted)]">
                        {formatTime(task.lastUpdateTime)}
                        {task.lastError && (
                          <span className="text-[var(--neon-red)] ml-2">⚠ {t('ddns.hasError')}</span>
                        )}
                      </div>
                      <button
                        onClick={() => handleDelete(task.id)}
                        className="text-[var(--neon-red)] py-1"
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

        {/* Create Task Modal */}
        {showCreateModal && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="cyber-card cyber-corner p-6 w-full max-w-lg max-h-[90vh] overflow-y-auto">
              <div className="flex items-center justify-between mb-6">
                <h3 className="display text-sm font-semibold text-[var(--text-primary)]">
                  {t('ddns.createTask')}
                </h3>
                <button
                  onClick={() => setShowCreateModal(false)}
                  className="text-[var(--text-muted)] hover:text-[var(--text-primary)]"
                >
                  ✕
                </button>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-xs text-[var(--text-muted)] mb-1">{t('ddns.taskName')} *</label>
                  <input
                    type="text"
                    className="cyber-input w-full"
                    value={newTask.name || ''}
                    onChange={(e) => setNewTask({ ...newTask, name: e.target.value })}
                    placeholder={t('ddns.taskNamePlaceholder')}
                  />
                </div>

                <div>
                  <label className="block text-xs text-[var(--text-muted)] mb-1">{t('ddns.provider')} *</label>
                  <select
                    className="cyber-input w-full"
                    value={newTask.providerName || ''}
                    onChange={(e) => setNewTask({ ...newTask, providerName: e.target.value })}
                  >
                    <option value="">{t('ddns.selectProvider')}</option>
                    {providers.map((p) => (
                      <option key={p.name} value={p.name}>{p.displayName}</option>
                    ))}
                  </select>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs text-[var(--text-muted)] mb-1">API ID *</label>
                    <input
                      type="text"
                      className="cyber-input w-full"
                      value={newTask.providerId || ''}
                      onChange={(e) => setNewTask({ ...newTask, providerId: e.target.value })}
                    />
                  </div>
                  <div>
                    <label className="block text-xs text-[var(--text-muted)] mb-1">API Secret</label>
                    <input
                      type="password"
                      className="cyber-input w-full"
                      value={newTask.providerSecret || ''}
                      onChange={(e) => setNewTask({ ...newTask, providerSecret: e.target.value })}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs text-[var(--text-muted)] mb-1">{t('ddns.domain')} *</label>
                    <input
                      type="text"
                      className="cyber-input w-full"
                      value={newTask.domain || ''}
                      onChange={(e) => setNewTask({ ...newTask, domain: e.target.value })}
                      placeholder="example.com"
                    />
                  </div>
                  <div>
                    <label className="block text-xs text-[var(--text-muted)] mb-1">{t('ddns.subDomain')}</label>
                    <input
                      type="text"
                      className="cyber-input w-full"
                      value={newTask.subDomain || ''}
                      onChange={(e) => setNewTask({ ...newTask, subDomain: e.target.value })}
                      placeholder="www"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs text-[var(--text-muted)] mb-1">{t('ddns.recordId')} *</label>
                  <input
                    type="text"
                    className="cyber-input w-full"
                    value={newTask.recordId || ''}
                    onChange={(e) => setNewTask({ ...newTask, recordId: e.target.value })}
                    placeholder={t('ddns.recordIdPlaceholder')}
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs text-[var(--text-muted)] mb-1">TTL ({t('ddns.seconds')})</label>
                    <input
                      type="number"
                      className="cyber-input w-full"
                      value={newTask.ttl || 600}
                      onChange={(e) => setNewTask({ ...newTask, ttl: parseInt(e.target.value) || 600 })}
                    />
                  </div>
                  <div>
                    <label className="block text-xs text-[var(--text-muted)] mb-1">{t('ddns.interval')} ({t('ddns.minutes')})</label>
                    <input
                      type="number"
                      className="cyber-input w-full"
                      value={newTask.intervalMinutes || 5}
                      onChange={(e) => setNewTask({ ...newTask, intervalMinutes: parseInt(e.target.value) || 5 })}
                      min={1}
                    />
                  </div>
                </div>

                <div className="flex justify-end gap-3 pt-4">
                  <button
                    onClick={() => setShowCreateModal(false)}
                    className="cyber-btn text-xs px-4 py-2"
                  >
                    {t('common.cancel')}
                  </button>
                  <button
                    onClick={handleCreate}
                    className="cyber-btn cyber-btn-primary text-xs px-4 py-2"
                    disabled={createMutation.isPending}
                  >
                    {createMutation.isPending ? t('common.loading') : t('common.create')}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}
