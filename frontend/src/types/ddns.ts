export interface DdnsTask {
  id: string;
  name: string;
  providerName: string;
  domain: string;
  recordId: string;
  subDomain?: string;
  ttl: number;
  intervalMinutes: number;
  enabled: boolean;
  lastKnownIp?: string;
  lastCheckTime?: string;
  lastUpdateTime?: string;
  lastError?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDdnsTaskRequest {
  name: string;
  providerName: string;
  providerId: string;
  providerSecret: string;
  domain: string;
  recordId: string;
  subDomain?: string;
  ttl?: number;
  intervalMinutes?: number;
  extraParams?: Record<string, string>;
}

export interface UpdateDdnsTaskRequest {
  enabled?: boolean;
  intervalMinutes?: number;
  providerId?: string;
  providerSecret?: string;
}

export interface DdnsIpResponse {
  ip: string;
  source: string;
}

export interface DdnsUpdateRequest {
  providerName: string;
  providerId: string;
  providerSecret: string;
  domain: string;
  recordId: string;
  lastKnownIp?: string;
  ttl?: number;
  forceUpdate?: boolean;
  extraParams?: Record<string, string>;
}

export interface DdnsUpdateResponse {
  updated: boolean;
  currentIp: string;
  previousIp?: string;
  message: string;
}

export interface DdnsApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}
