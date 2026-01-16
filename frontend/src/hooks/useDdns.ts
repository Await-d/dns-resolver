import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  fetchCurrentIp,
  fetchDdnsTasks,
  createDdnsTask,
  updateDdnsTask,
  deleteDdnsTask,
  enableDdnsTask,
  disableDdnsTask,
  updateDdns,
  fetchIpSources,
} from '../services/ddnsApi';
import type { CreateDdnsTaskRequest, UpdateDdnsTaskRequest, DdnsUpdateRequest } from '../types/ddns';

export function useIpSources() {
  return useQuery({
    queryKey: ['ddns', 'ip-sources'],
    queryFn: fetchIpSources,
    staleTime: 1000 * 60 * 60, // Cache for 1 hour
  });
}

export function useCurrentIp(source?: string) {
  return useQuery({
    queryKey: ['ddns', 'ip', source],
    queryFn: () => fetchCurrentIp(source),
    refetchInterval: 60000, // Refresh every minute
  });
}

export function useDdnsTasks() {
  return useQuery({
    queryKey: ['ddns', 'tasks'],
    queryFn: fetchDdnsTasks,
    refetchInterval: 30000, // Refresh every 30 seconds
  });
}

export function useCreateDdnsTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateDdnsTaskRequest) => createDdnsTask(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ddns', 'tasks'] });
    },
  });
}

export function useUpdateDdnsTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, request }: { taskId: string; request: UpdateDdnsTaskRequest }) =>
      updateDdnsTask(taskId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ddns', 'tasks'] });
    },
  });
}

export function useDeleteDdnsTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (taskId: string) => deleteDdnsTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ddns', 'tasks'] });
    },
  });
}

export function useToggleDdnsTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, enabled }: { taskId: string; enabled: boolean }) =>
      enabled ? enableDdnsTask(taskId) : disableDdnsTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ddns', 'tasks'] });
    },
  });
}

export function useManualDdnsUpdate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: DdnsUpdateRequest) => updateDdns(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ddns', 'ip'] });
      queryClient.invalidateQueries({ queryKey: ['ddns', 'tasks'] });
    },
  });
}
