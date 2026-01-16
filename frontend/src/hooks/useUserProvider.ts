import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  fetchUserProviderConfigs,
  fetchAvailableProviders,
  addProviderConfig,
  updateProviderConfig,
  deleteProviderConfig,
  toggleProviderConfig,
  fetchDomainsByConfig,
  fetchRecordsByConfig,
  addDnsRecord,
  updateDnsRecord,
  deleteDnsRecord,
  type UpdateProviderConfigRequest,
  type AddDnsRecordRequest,
  type UpdateDnsRecordRequest,
} from '../services/userProviderApi';

export function useUserProviderConfigs() {
  return useQuery({
    queryKey: ['userProviderConfigs'],
    queryFn: fetchUserProviderConfigs,
  });
}

export function useAvailableProviders() {
  return useQuery({
    queryKey: ['availableProviders'],
    queryFn: fetchAvailableProviders,
  });
}

export function useAddProviderConfig() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: addProviderConfig,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['userProviderConfigs'] });
      queryClient.invalidateQueries({ queryKey: ['availableProviders'] });
    },
  });
}

export function useUpdateProviderConfig() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateProviderConfigRequest }) =>
      updateProviderConfig(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['userProviderConfigs'] });
      queryClient.invalidateQueries({ queryKey: ['availableProviders'] });
    },
  });
}

export function useDeleteProviderConfig() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteProviderConfig,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['userProviderConfigs'] });
      queryClient.invalidateQueries({ queryKey: ['availableProviders'] });
    },
  });
}

export function useToggleProviderConfig() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: toggleProviderConfig,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['userProviderConfigs'] });
      queryClient.invalidateQueries({ queryKey: ['availableProviders'] });
    },
  });
}

export function useDomainsByConfig(configId: string | null) {
  return useQuery({
    queryKey: ['domainsByConfig', configId],
    queryFn: () => fetchDomainsByConfig(configId!),
    enabled: !!configId,
  });
}

export function useRecordsByConfig(
  configId: string | null,
  domain: string | null,
  subDomain?: string,
  recordType?: string
) {
  return useQuery({
    queryKey: ['recordsByConfig', configId, domain, subDomain, recordType],
    queryFn: () => fetchRecordsByConfig(configId!, domain!, subDomain, recordType),
    enabled: !!configId && !!domain,
  });
}

export function useAddDnsRecord() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ configId, domain, request }: { configId: string; domain: string; request: AddDnsRecordRequest }) =>
      addDnsRecord(configId, domain, request),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['recordsByConfig', variables.configId, variables.domain] });
    },
  });
}

export function useUpdateDnsRecord() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ configId, domain, recordId, request }: { configId: string; domain: string; recordId: string; request: UpdateDnsRecordRequest }) =>
      updateDnsRecord(configId, domain, recordId, request),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['recordsByConfig', variables.configId, variables.domain] });
    },
  });
}

export function useDeleteDnsRecord() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ configId, domain, recordId }: { configId: string; domain: string; recordId: string }) =>
      deleteDnsRecord(configId, domain, recordId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['recordsByConfig', variables.configId, variables.domain] });
    },
  });
}
