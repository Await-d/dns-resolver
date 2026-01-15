import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  fetchUserProviderConfigs,
  fetchAvailableProviders,
  addProviderConfig,
  updateProviderConfig,
  deleteProviderConfig,
  toggleProviderConfig,
  type UpdateProviderConfigRequest,
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
