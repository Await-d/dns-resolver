import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { ResolveResult } from '../types/dns';

interface ResultTableProps {
  results: ResolveResult[];
  onEdit?: (index: number, field: string, value: string) => void;
}

interface EditingCell {
  rowIndex: number;
  field: string;
  value: string;
}

export default function ResultTable({ results, onEdit }: ResultTableProps) {
  const { t } = useTranslation();
  const [editingCell, setEditingCell] = useState<EditingCell | null>(null);
  const [hoveredRow, setHoveredRow] = useState<number | null>(null);

  if (results.length === 0) {
    return (
      <div className="py-16 text-center">
        <div className="inline-flex flex-col items-center gap-4">
          <div className="w-16 h-16 border-2 border-dashed border-[var(--border-color)] flex items-center justify-center">
            <svg className="w-8 h-8 text-[var(--text-muted)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <div>
            <p className="text-[var(--text-secondary)] mb-1">{t('results.noData')}</p>
            <p className="text-xs text-[var(--text-muted)]">{t('results.noDataHint')}</p>
          </div>
        </div>
      </div>
    );
  }

  const successCount = results.filter(r => r.success).length;
  const avgTime = Math.round(results.reduce((acc, r) => acc + r.queryTimeMs, 0) / results.length);

  const handleDoubleClick = (rowIndex: number, field: string, currentValue: string) => {
    if (onEdit) {
      setEditingCell({ rowIndex, field, value: currentValue });
    }
  };

  const handleEditChange = (value: string) => {
    if (editingCell) {
      setEditingCell({ ...editingCell, value });
    }
  };

  const handleEditSave = () => {
    if (editingCell && onEdit) {
      onEdit(editingCell.rowIndex, editingCell.field, editingCell.value);
      setEditingCell(null);
    }
  };

  const handleEditCancel = () => {
    setEditingCell(null);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleEditSave();
    } else if (e.key === 'Escape') {
      handleEditCancel();
    }
  };

  const renderEditableCell = (rowIndex: number, field: string, value: string, displayValue: React.ReactNode) => {
    const isEditing = editingCell?.rowIndex === rowIndex && editingCell?.field === field;

    if (isEditing) {
      return (
        <input
          type="text"
          value={editingCell.value}
          onChange={(e) => handleEditChange(e.target.value)}
          onBlur={handleEditSave}
          onKeyDown={handleKeyDown}
          autoFocus
          className="terminal-input py-1 px-2 text-sm w-full"
        />
      );
    }

    return (
      <div
        onDoubleClick={() => handleDoubleClick(rowIndex, field, value)}
        className={`cursor-pointer hover:bg-[var(--neon-cyan-dim)] px-1 -mx-1 transition-all ${onEdit ? 'group' : ''}`}
        title={onEdit ? t('results.doubleClickEdit') : undefined}
      >
        {displayValue}
        {onEdit && hoveredRow === rowIndex && (
          <span className="ml-2 opacity-0 group-hover:opacity-50 text-[var(--neon-cyan)] text-xs">
            ✎
          </span>
        )}
      </div>
    );
  };

  return (
    <div className="space-y-4">
      {/* Stats Bar */}
      <div className="flex flex-wrap gap-4 pb-4 border-b border-[var(--border-color)]">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 border border-[var(--neon-green)] bg-[var(--neon-green-dim)] flex items-center justify-center">
            <span className="text-[var(--neon-green)] text-sm font-bold">{successCount}</span>
          </div>
          <div>
            <div className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider">{t('results.success')}</div>
            <div className="text-sm text-[var(--text-primary)]">{successCount} / {results.length}</div>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <div className="w-8 h-8 border border-[var(--neon-orange)] bg-[var(--neon-orange-dim)] flex items-center justify-center">
            <span className="text-[var(--neon-orange)] text-sm font-bold">{avgTime}</span>
          </div>
          <div>
            <div className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider">{t('results.avgTime')}</div>
            <div className="text-sm text-[var(--text-primary)]">{avgTime} ms</div>
          </div>
        </div>

        {results.length > 0 && results[0].domain && (
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 border border-[var(--neon-cyan)] bg-[var(--neon-cyan-dim)] flex items-center justify-center">
              <svg className="w-4 h-4 text-[var(--neon-cyan)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
              </svg>
            </div>
            <div>
              <div className="text-[10px] text-[var(--text-muted)] uppercase tracking-wider">{t('results.domain')}</div>
              <div className="text-sm text-[var(--neon-cyan)] font-mono">{results[0].domain}</div>
            </div>
          </div>
        )}

        {/* Legend */}
        <div className="ml-auto flex items-center gap-3 text-xs text-[var(--text-muted)]">
          <span className="flex items-center gap-1">
            <span className="w-2 h-2 bg-[var(--neon-green)] rounded-full"></span>
            {t('results.legend.fast')}
          </span>
          <span className="flex items-center gap-1">
            <span className="w-2 h-2 bg-[var(--neon-orange)] rounded-full"></span>
            {t('results.legend.medium')}
          </span>
          <span className="flex items-center gap-1">
            <span className="w-2 h-2 bg-[var(--neon-red)] rounded-full"></span>
            {t('results.legend.slow')}
          </span>
        </div>
      </div>

      {/* Results Table */}
      <div className="overflow-x-auto">
        <table className="data-table">
          <thead>
            <tr>
              <th className="w-[180px]">{t('results.provider')}</th>
              <th>{t('results.resolution')}</th>
              <th className="w-[80px]">{t('results.ttl')}</th>
              <th className="w-[140px]">{t('results.latency')}</th>
              <th className="w-[100px]">{t('results.status')}</th>
              {onEdit && <th className="w-[60px]"></th>}
            </tr>
          </thead>
          <tbody>
            {results.map((result, index) => (
              <tr
                key={index}
                onMouseEnter={() => setHoveredRow(index)}
                onMouseLeave={() => setHoveredRow(null)}
                className={hoveredRow === index ? 'bg-[var(--neon-cyan-dim)]/30' : ''}
              >
                {/* Provider */}
                <td>
                  <div className="flex items-center gap-3">
                    <div className={`w-1 h-8 ${result.success ? 'bg-[var(--neon-green)]' : 'bg-[var(--neon-red)]'}`}></div>
                    <div>
                      <div className="font-medium text-[var(--text-primary)] text-sm">{result.ispName}</div>
                      <div className="text-[10px] text-[var(--text-muted)] font-mono">{result.dnsServer}</div>
                    </div>
                  </div>
                </td>

                {/* Resolution */}
                <td>
                  {result.success ? (
                    <div className="space-y-1">
                      {result.records.length > 0 ? (
                        result.records.map((record, i) => (
                          <div key={i} className="flex items-center gap-2">
                            <span className="text-[var(--neon-cyan)] text-xs">▸</span>
                            {renderEditableCell(
                              index,
                              `record_${i}`,
                              record.value,
                              <code className="text-sm text-[var(--text-primary)] font-mono bg-[var(--bg-tertiary)] px-2 py-0.5">
                                {record.value}
                              </code>
                            )}
                          </div>
                        ))
                      ) : (
                        <span className="text-[var(--text-muted)] text-sm italic">{t('results.noRecords')}</span>
                      )}
                    </div>
                  ) : (
                    <div className="flex items-center gap-2">
                      <span className="text-[var(--neon-red)] text-xs">✕</span>
                      <span className="text-[var(--neon-red)] text-sm">{result.errorMessage}</span>
                    </div>
                  )}
                </td>

                {/* TTL */}
                <td>
                  {result.success && result.records.length > 0 ? (
                    renderEditableCell(
                      index,
                      'ttl',
                      String(result.records[0].ttl),
                      <div className="flex items-center gap-1">
                        <span className="text-[var(--text-primary)] font-mono text-sm">{result.records[0].ttl}</span>
                        <span className="text-[10px] text-[var(--text-muted)]">s</span>
                      </div>
                    )
                  ) : (
                    <span className="text-[var(--text-muted)]">—</span>
                  )}
                </td>

                {/* Latency */}
                <td>
                  <div className="flex items-center gap-2">
                    <div className="w-12 h-1.5 bg-[var(--bg-tertiary)] overflow-hidden">
                      <div
                        className={`h-full transition-all ${
                          result.queryTimeMs < 50
                            ? 'bg-[var(--neon-green)]'
                            : result.queryTimeMs < 150
                            ? 'bg-[var(--neon-orange)]'
                            : 'bg-[var(--neon-red)]'
                        }`}
                        style={{ width: `${Math.min(100, (result.queryTimeMs / 300) * 100)}%` }}
                      ></div>
                    </div>
                    <span className={`font-mono text-sm ${
                      result.queryTimeMs < 50
                        ? 'text-[var(--neon-green)]'
                        : result.queryTimeMs < 150
                        ? 'text-[var(--neon-orange)]'
                        : 'text-[var(--neon-red)]'
                    }`}>
                      {result.queryTimeMs}
                    </span>
                    <span className="text-[10px] text-[var(--text-muted)]">ms</span>
                  </div>
                </td>

                {/* Status */}
                <td>
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
                </td>

                {/* Actions */}
                {onEdit && (
                  <td>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => handleDoubleClick(index, 'record_0', result.records[0]?.value || '')}
                        className="p-1 text-[var(--text-muted)] hover:text-[var(--neon-cyan)] transition-all"
                        title={t('results.edit')}
                      >
                        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                        </svg>
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Footer */}
      <div className="pt-3 border-t border-[var(--border-color)] flex items-center justify-between text-xs text-[var(--text-muted)]">
        <span>{t('results.queryTime')} {new Date().toLocaleTimeString('zh-CN', { hour12: false })}</span>
        {onEdit && (
          <span className="text-[var(--neon-cyan)]">
            {t('results.doubleClickEdit')}
          </span>
        )}
      </div>
    </div>
  );
}
