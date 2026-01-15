export interface DnsRecord {
  value: string;
  ttl: number;
  recordType: string;
}

export interface IspInfo {
  id: string;
  name: string;
  primaryDns: string;
  secondaryDns?: string;
}

export interface ResolveResult {
  domain: string;
  recordType: string;
  dnsServer: string;
  ispName: string;
  records: DnsRecord[];
  queryTimeMs: number;
  success: boolean;
  errorMessage?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
}

export interface CompareResponse {
  domain: string;
  recordType: string;
  results: ResolveResult[];
}

export type RecordType = 'A' | 'AAAA' | 'CNAME' | 'MX' | 'TXT' | 'NS' | 'SOA';

export const RECORD_TYPES: RecordType[] = ['A', 'AAAA', 'CNAME', 'MX', 'TXT', 'NS', 'SOA'];
