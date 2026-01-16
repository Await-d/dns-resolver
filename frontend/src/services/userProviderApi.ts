import { get, post, put, del } from './apiClient';

const API_BASE = '/api/v1/user/providers';

export interface UserProviderConfig {
  id: string;
  providerName: string;
  displayName: string;
  isActive: boolean;
  createdAt: string;
  lastUsedAt?: string;
}

export interface AvailableProvider {
  name: string;
  displayName: string;
  isConfigured: boolean;
  configId?: string;
  isActive: boolean;
}

export interface AddProviderConfigRequest {
  providerName: string;
  apiId: string;
  apiSecret: string;
  displayName?: string;
  extraParams?: Record<string, string>;
}

export interface UpdateProviderConfigRequest {
  displayName: string;
  apiId: string;
  apiSecret: string;
  extraParams?: Record<string, string>;
}

export interface DnsRecordInfo {
  recordId: string;
  subDomain: string;
  recordType: string;
  value: string;
  ttl: number;
  status?: string;
}

export interface AddDnsRecordRequest {
  subDomain: string;
  recordType: string;
  value: string;
  ttl?: number;
}

export interface UpdateDnsRecordRequest {
  value: string;
  ttl?: number;
}

export function fetchUserProviderConfigs(): Promise<UserProviderConfig[]> {
  return get<UserProviderConfig[]>(API_BASE);
}

export function fetchAvailableProviders(): Promise<AvailableProvider[]> {
  return get<AvailableProvider[]>(`${API_BASE}/available`);
}

export function addProviderConfig(request: AddProviderConfigRequest): Promise<UserProviderConfig> {
  return post<UserProviderConfig>(API_BASE, request);
}

export function updateProviderConfig(id: string, request: UpdateProviderConfigRequest): Promise<UserProviderConfig> {
  return put<UserProviderConfig>(`${API_BASE}/${id}`, request);
}

export function deleteProviderConfig(id: string): Promise<boolean> {
  return del<boolean>(`${API_BASE}/${id}`);
}

export function toggleProviderConfig(id: string): Promise<UserProviderConfig> {
  return post<UserProviderConfig>(`${API_BASE}/${id}/toggle`);
}

export function fetchDomainsByConfig(configId: string): Promise<string[]> {
  return get<string[]>(`${API_BASE}/${configId}/domains`);
}

export function fetchRecordsByConfig(
  configId: string,
  domain: string,
  subDomain?: string,
  recordType?: string
): Promise<DnsRecordInfo[]> {
  const params = new URLSearchParams();
  if (subDomain) params.append('subDomain', subDomain);
  if (recordType) params.append('recordType', recordType);
  const queryString = params.toString();
  const url = `${API_BASE}/${configId}/domains/${encodeURIComponent(domain)}/records${queryString ? `?${queryString}` : ''}`;
  return get<DnsRecordInfo[]>(url);
}

export function addDnsRecord(
  configId: string,
  domain: string,
  request: AddDnsRecordRequest
): Promise<DnsRecordInfo> {
  const url = `${API_BASE}/${configId}/domains/${encodeURIComponent(domain)}/records`;
  return post<DnsRecordInfo>(url, request);
}

export function updateDnsRecord(
  configId: string,
  domain: string,
  recordId: string,
  request: UpdateDnsRecordRequest
): Promise<DnsRecordInfo> {
  const url = `${API_BASE}/${configId}/domains/${encodeURIComponent(domain)}/records/${encodeURIComponent(recordId)}`;
  return put<DnsRecordInfo>(url, request);
}

export function deleteDnsRecord(
  configId: string,
  domain: string,
  recordId: string
): Promise<boolean> {
  const url = `${API_BASE}/${configId}/domains/${encodeURIComponent(domain)}/records/${encodeURIComponent(recordId)}`;
  return del<boolean>(url);
}
