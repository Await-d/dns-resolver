import { get, post, put, del } from './apiClient';
import type {
  DdnsTask,
  CreateDdnsTaskRequest,
  UpdateDdnsTaskRequest,
  DdnsIpResponse,
  DdnsUpdateRequest,
  DdnsUpdateResponse,
  IpSourceInfo,
} from '../types/ddns';

const API_BASE = '/api/v1/ddns';

export function fetchIpSources(): Promise<IpSourceInfo[]> {
  return get<IpSourceInfo[]>(`${API_BASE}/ip-sources`);
}

export function fetchCurrentIp(source?: string): Promise<DdnsIpResponse> {
  const url = source ? `${API_BASE}/ip?source=${encodeURIComponent(source)}` : `${API_BASE}/ip`;
  return get<DdnsIpResponse>(url);
}

export function updateDdns(request: DdnsUpdateRequest): Promise<DdnsUpdateResponse> {
  return post<DdnsUpdateResponse>(`${API_BASE}/update`, request);
}

export function fetchDdnsTasks(): Promise<DdnsTask[]> {
  return get<DdnsTask[]>(`${API_BASE}/tasks`);
}

export function createDdnsTask(request: CreateDdnsTaskRequest): Promise<DdnsTask> {
  return post<DdnsTask>(`${API_BASE}/tasks`, request);
}

export function updateDdnsTask(taskId: string, request: UpdateDdnsTaskRequest): Promise<void> {
  return put<void>(`${API_BASE}/tasks/${taskId}`, request);
}

export function deleteDdnsTask(taskId: string): Promise<void> {
  return del<void>(`${API_BASE}/tasks/${taskId}`);
}

export function enableDdnsTask(taskId: string): Promise<void> {
  return post<void>(`${API_BASE}/tasks/${taskId}/enable`);
}

export function disableDdnsTask(taskId: string): Promise<void> {
  return post<void>(`${API_BASE}/tasks/${taskId}/disable`);
}
