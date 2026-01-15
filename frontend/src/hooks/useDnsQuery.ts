import { useQuery, useMutation } from '@tanstack/react-query';
import { fetchIsps, compareResolve } from '../services/dnsApi';
import type { CompareResponse } from '../types/dns';

export function useIsps() {
  return useQuery({
    queryKey: ['isps'],
    queryFn: fetchIsps,
    staleTime: Infinity
  });
}

export function useDnsCompare() {
  return useMutation<
    CompareResponse,
    Error,
    { domain: string; recordType: string; ispList: string[] }
  >({
    mutationFn: ({ domain, recordType, ispList }) =>
      compareResolve(domain, recordType, ispList)
  });
}
