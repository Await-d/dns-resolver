import { get, post } from './apiClient';
import type { CompareResponse, IspInfo, ResolveResult } from '../types/dns';

const API_BASE = '/api/v1/dns';

export function fetchIsps(): Promise<IspInfo[]> {
  return get<IspInfo[]>(`${API_BASE}/isps`, { auth: false });
}

export function resolveOnce(
  domain: string,
  recordType: string,
  dnsServer: string
): Promise<ResolveResult> {
  return post<ResolveResult>(`${API_BASE}/resolve`, { domain, recordType, dnsServer }, { auth: false });
}

export function compareResolve(
  domain: string,
  recordType: string,
  ispList: string[]
): Promise<CompareResponse> {
  return post<CompareResponse>(`${API_BASE}/compare`, { domain, recordType, ispList }, { auth: false });
}
