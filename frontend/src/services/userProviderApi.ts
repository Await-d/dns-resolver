const API_BASE = '/api/v1/user/providers';

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('dns_token');
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

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

export async function fetchUserProviderConfigs(): Promise<UserProviderConfig[]> {
  const response = await fetch(API_BASE, {
    headers: getAuthHeaders(),
  });
  if (!response.ok) throw new Error('Failed to fetch user provider configs');
  const data: ApiResponse<UserProviderConfig[]> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data || [];
}

export async function fetchAvailableProviders(): Promise<AvailableProvider[]> {
  const response = await fetch(`${API_BASE}/available`, {
    headers: getAuthHeaders(),
  });
  if (!response.ok) throw new Error('Failed to fetch available providers');
  const data: ApiResponse<AvailableProvider[]> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data || [];
}

export async function addProviderConfig(request: AddProviderConfigRequest): Promise<UserProviderConfig> {
  const response = await fetch(API_BASE, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(request),
  });
  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.error || 'Failed to add provider config');
  }
  const data: ApiResponse<UserProviderConfig> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}

export async function updateProviderConfig(id: string, request: UpdateProviderConfigRequest): Promise<UserProviderConfig> {
  const response = await fetch(`${API_BASE}/${id}`, {
    method: 'PUT',
    headers: getAuthHeaders(),
    body: JSON.stringify(request),
  });
  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.error || 'Failed to update provider config');
  }
  const data: ApiResponse<UserProviderConfig> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}

export async function deleteProviderConfig(id: string): Promise<void> {
  const response = await fetch(`${API_BASE}/${id}`, {
    method: 'DELETE',
    headers: getAuthHeaders(),
  });
  if (!response.ok) throw new Error('Failed to delete provider config');
  const data: ApiResponse<boolean> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
}

export async function toggleProviderConfig(id: string): Promise<UserProviderConfig> {
  const response = await fetch(`${API_BASE}/${id}/toggle`, {
    method: 'POST',
    headers: getAuthHeaders(),
  });
  if (!response.ok) throw new Error('Failed to toggle provider config');
  const data: ApiResponse<UserProviderConfig> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}
