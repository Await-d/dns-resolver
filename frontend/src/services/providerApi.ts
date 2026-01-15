import type {
  ProviderInfo,
  ProviderCredentials,
  DnsRecordInfo,
  AddRecordRequest,
  UpdateRecordRequest,
  DeleteRecordRequest,
  GetRecordsRequest,
  ProviderApiResponse,
} from '../types/provider';

const API_BASE = '/api/v1/providers';

export async function fetchProviders(): Promise<ProviderInfo[]> {
  const response = await fetch(API_BASE);
  if (!response.ok) throw new Error('Failed to fetch providers');
  const data: ProviderApiResponse<ProviderInfo[]> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data || [];
}

export async function fetchDomains(credentials: ProviderCredentials): Promise<string[]> {
  const response = await fetch(`${API_BASE}/domains`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials),
  });
  if (!response.ok) throw new Error('Failed to fetch domains');
  const data: ProviderApiResponse<string[]> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data || [];
}

export async function fetchRecords(request: GetRecordsRequest): Promise<DnsRecordInfo[]> {
  const response = await fetch(`${API_BASE}/records/list`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to fetch records');
  const data: ProviderApiResponse<DnsRecordInfo[]> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data || [];
}

export async function addRecord(request: AddRecordRequest): Promise<DnsRecordInfo> {
  const response = await fetch(`${API_BASE}/records/add`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to add record');
  const data: ProviderApiResponse<DnsRecordInfo> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}

export async function updateRecord(request: UpdateRecordRequest): Promise<DnsRecordInfo> {
  const response = await fetch(`${API_BASE}/records/update`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update record');
  const data: ProviderApiResponse<DnsRecordInfo> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}

export async function deleteRecord(request: DeleteRecordRequest): Promise<void> {
  const response = await fetch(`${API_BASE}/records/delete`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to delete record');
  const data: ProviderApiResponse<boolean> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
}
