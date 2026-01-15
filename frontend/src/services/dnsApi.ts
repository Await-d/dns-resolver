import type { ApiResponse, CompareResponse, IspInfo, ResolveResult } from '../types/dns';

const API_BASE = '/api/v1/dns';

export async function fetchIsps(): Promise<IspInfo[]> {
  const response = await fetch(`${API_BASE}/isps`);
  if (!response.ok) throw new Error('Failed to fetch ISPs');
  const data: ApiResponse<IspInfo[]> = await response.json();
  return data.data;
}

export async function resolveOnce(
  domain: string,
  recordType: string,
  dnsServer: string
): Promise<ResolveResult> {
  const response = await fetch(`${API_BASE}/resolve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ domain, recordType, dnsServer })
  });
  if (!response.ok) throw new Error('Failed to resolve');
  const data: ApiResponse<ResolveResult> = await response.json();
  return data.data;
}

export async function compareResolve(
  domain: string,
  recordType: string,
  ispList: string[]
): Promise<CompareResponse> {
  const response = await fetch(`${API_BASE}/compare`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ domain, recordType, ispList })
  });
  if (!response.ok) throw new Error('Failed to compare');
  const data: ApiResponse<CompareResponse> = await response.json();
  return data.data;
}
