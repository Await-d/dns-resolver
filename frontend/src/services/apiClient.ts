/**
 * 统一的 API 客户端工具模块
 */

// Token 存储 key
export const TOKEN_KEY = 'dns_token';

/**
 * 获取认证 Headers
 */
export function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem(TOKEN_KEY);
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

/**
 * 通用 API 响应类型
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

/**
 * 请求配置
 */
interface RequestOptions<T> {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
  /** 是否需要认证，默认 true */
  auth?: boolean;
  /** 自定义错误消息 */
  errorMessage?: string;
  /** 响应数据转换器 */
  transform?: (data: T) => T;
}

/**
 * 统一的请求方法
 */
export async function request<T>(
  url: string,
  options: RequestOptions<T> = {}
): Promise<T> {
  const {
    method = 'GET',
    body,
    auth = true,
    errorMessage = 'Request failed',
  } = options;

  const headers: HeadersInit = auth
    ? getAuthHeaders()
    : { 'Content-Type': 'application/json' };

  const response = await fetch(url, {
    method,
    headers,
    ...(body ? { body: JSON.stringify(body) } : {}),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ error: errorMessage }));
    throw new Error(error.error || error.message || errorMessage);
  }

  const data: ApiResponse<T> = await response.json();

  if (!data.success) {
    throw new Error(data.error || errorMessage);
  }

  return data.data as T;
}

/**
 * GET 请求
 */
export function get<T>(url: string, options?: Omit<RequestOptions<T>, 'method' | 'body'>) {
  return request<T>(url, { ...options, method: 'GET' });
}

/**
 * POST 请求
 */
export function post<T>(url: string, body?: unknown, options?: Omit<RequestOptions<T>, 'method' | 'body'>) {
  return request<T>(url, { ...options, method: 'POST', body });
}

/**
 * PUT 请求
 */
export function put<T>(url: string, body?: unknown, options?: Omit<RequestOptions<T>, 'method' | 'body'>) {
  return request<T>(url, { ...options, method: 'PUT', body });
}

/**
 * DELETE 请求
 */
export function del<T>(url: string, options?: Omit<RequestOptions<T>, 'method' | 'body'>) {
  return request<T>(url, { ...options, method: 'DELETE' });
}
