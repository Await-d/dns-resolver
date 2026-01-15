import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  fetchProviders,
  fetchDomains,
  fetchRecords,
  addRecord,
  updateRecord,
  deleteRecord,
} from '../services/providerApi';
import type {
  ProviderCredentials,
  GetRecordsRequest,
  AddRecordRequest,
  UpdateRecordRequest,
  DeleteRecordRequest,
} from '../types/provider';

export function useProviders() {
  return useQuery({
    queryKey: ['providers'],
    queryFn: fetchProviders,
  });
}

export function useDomains(credentials: ProviderCredentials | null) {
  return useQuery({
    queryKey: ['domains', credentials?.providerName, credentials?.id],
    queryFn: () => fetchDomains(credentials!),
    enabled: !!credentials?.providerName && !!credentials?.id && !!credentials?.secret,
  });
}

export function useRecords(request: GetRecordsRequest | null) {
  return useQuery({
    queryKey: ['records', request?.providerName, request?.domain],
    queryFn: () => fetchRecords(request!),
    enabled: !!request?.providerName && !!request?.domain && !!request?.id && !!request?.secret,
  });
}

export function useAddRecord() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: AddRecordRequest) => addRecord(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['records', variables.providerName, variables.domain],
      });
    },
  });
}

export function useUpdateRecord() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: UpdateRecordRequest) => updateRecord(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['records', variables.providerName, variables.domain],
      });
    },
  });
}

export function useDeleteRecord() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: DeleteRecordRequest) => deleteRecord(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['records', variables.providerName, variables.domain],
      });
    },
  });
}
