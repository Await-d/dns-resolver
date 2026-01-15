export interface ProviderFieldMeta {
  idLabel: string | null;
  secretLabel: string | null;
  extParamLabel: string | null;
  helpUrl: string | null;
  helpText: string | null;
}

export interface ProviderInfo {
  name: string;
  displayName: string;
  fieldMeta: ProviderFieldMeta;
}

export interface ProviderCredentials {
  providerName: string;
  id: string;
  secret: string;
  extraParams?: Record<string, string>;
}

export interface DnsRecordInfo {
  recordId: string;
  domain: string;
  subDomain: string;
  fullDomain: string;
  recordType: string;
  value: string;
  ttl: number;
  enabled?: boolean;
}

export interface AddRecordRequest extends ProviderCredentials {
  domain: string;
  subDomain: string;
  recordType: string;
  value: string;
  ttl?: number;
}

export interface UpdateRecordRequest extends ProviderCredentials {
  domain: string;
  recordId: string;
  value: string;
  ttl?: number;
}

export interface DeleteRecordRequest extends ProviderCredentials {
  domain: string;
  recordId: string;
}

export interface GetRecordsRequest extends ProviderCredentials {
  domain: string;
  subDomain?: string;
  recordType?: string;
}

export interface ProviderApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}
