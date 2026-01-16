import { get, post } from './apiClient';
import type {
  ProviderInfo,
  ProviderCredentials,
  DnsRecordInfo,
  AddRecordRequest,
  UpdateRecordRequest,
  DeleteRecordRequest,
  GetRecordsRequest,
} from '../types/provider';

const API_BASE = '/api/v1/providers';

export async function fetchProviders(): Promise<ProviderInfo[]> {
  return get<ProviderInfo[]>(API_BASE);
}

export async function fetchDomains(credentials: ProviderCredentials): Promise<string[]> {
  return post<string[]>(`${API_BASE}/domains`, credentials);
}

export async function fetchRecords(request: GetRecordsRequest): Promise<DnsRecordInfo[]> {
  return post<DnsRecordInfo[]>(`${API_BASE}/records/list`, request);
}

export async function addRecord(request: AddRecordRequest): Promise<DnsRecordInfo> {
  return post<DnsRecordInfo>(`${API_BASE}/records/add`, request);
}

export async function updateRecord(request: UpdateRecordRequest): Promise<DnsRecordInfo> {
  return post<DnsRecordInfo>(`${API_BASE}/records/update`, request);
}

export async function deleteRecord(request: DeleteRecordRequest): Promise<boolean> {
  return post<boolean>(`${API_BASE}/records/delete`, request);
}
