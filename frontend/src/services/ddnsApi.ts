import type {
  DdnsTask,
  CreateDdnsTaskRequest,
  UpdateDdnsTaskRequest,
  DdnsIpResponse,
  DdnsUpdateRequest,
  DdnsUpdateResponse,
  DdnsApiResponse,
} from '../types/ddns';

const API_BASE = '/api/v1/ddns';

export async function fetchCurrentIp(): Promise<DdnsIpResponse> {
  const response = await fetch(`${API_BASE}/ip`);
  if (!response.ok) throw new Error('Failed to fetch IP');
  const data: DdnsApiResponse<DdnsIpResponse> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}

export async function updateDdns(request: DdnsUpdateRequest): Promise<DdnsUpdateResponse> {
  const response = await fetch(`${API_BASE}/update`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update DDNS');
  const data: DdnsApiResponse<DdnsUpdateResponse> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}

export async function fetchDdnsTasks(): Promise<DdnsTask[]> {
  const response = await fetch(`${API_BASE}/tasks`);
  if (!response.ok) throw new Error('Failed to fetch tasks');
  const data: DdnsApiResponse<DdnsTask[]> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data || [];
}

export async function createDdnsTask(request: CreateDdnsTaskRequest): Promise<DdnsTask> {
  const response = await fetch(`${API_BASE}/tasks`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create task');
  const data: DdnsApiResponse<DdnsTask> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
  return data.data!;
}

export async function updateDdnsTask(taskId: string, request: UpdateDdnsTaskRequest): Promise<void> {
  const response = await fetch(`${API_BASE}/tasks/${taskId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update task');
  const data: DdnsApiResponse<unknown> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
}

export async function deleteDdnsTask(taskId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/tasks/${taskId}`, {
    method: 'DELETE',
  });
  if (!response.ok) throw new Error('Failed to delete task');
  const data: DdnsApiResponse<unknown> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
}

export async function enableDdnsTask(taskId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/tasks/${taskId}/enable`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to enable task');
  const data: DdnsApiResponse<unknown> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
}

export async function disableDdnsTask(taskId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/tasks/${taskId}/disable`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to disable task');
  const data: DdnsApiResponse<unknown> = await response.json();
  if (!data.success) throw new Error(data.error || 'Failed');
}
