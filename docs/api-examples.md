# DNS Resolver API 使用示例

本文档提供 DNS Resolver API 的详细使用示例。

## 基础信息

- **Base URL**: `http://localhost:5000`
- **API 版本**: v1
- **Content-Type**: `application/json`

---

## 1. DNS 解析查询

### 1.1 获取支持的运营商列表

```bash
curl -X GET http://localhost:5000/api/v1/dns/isps
```

**响应示例**:
```json
{
  "success": true,
  "data": [
    {
      "id": "telecom",
      "name": "中国电信",
      "primaryDns": "202.96.128.86",
      "secondaryDns": "202.96.128.166"
    },
    {
      "id": "unicom",
      "name": "中国联通",
      "primaryDns": "221.5.88.88",
      "secondaryDns": "221.6.4.66"
    },
    {
      "id": "mobile",
      "name": "中国移动",
      "primaryDns": "211.136.192.6",
      "secondaryDns": "211.136.112.50"
    },
    {
      "id": "aliyun",
      "name": "阿里 DNS",
      "primaryDns": "223.5.5.5",
      "secondaryDns": "223.6.6.6"
    },
    {
      "id": "tencent",
      "name": "腾讯 DNS",
      "primaryDns": "119.29.29.29",
      "secondaryDns": "119.28.28.28"
    },
    {
      "id": "baidu",
      "name": "百度 DNS",
      "primaryDns": "180.76.76.76",
      "secondaryDns": null
    },
    {
      "id": "google",
      "name": "Google DNS",
      "primaryDns": "8.8.8.8",
      "secondaryDns": "8.8.4.4"
    },
    {
      "id": "cloudflare",
      "name": "Cloudflare DNS",
      "primaryDns": "1.1.1.1",
      "secondaryDns": "1.0.0.1"
    }
  ]
}
```

### 1.2 单次 DNS 解析

```bash
curl -X POST http://localhost:5000/api/v1/dns/resolve \
  -H "Content-Type: application/json" \
  -d '{
    "domain": "www.baidu.com",
    "recordType": "A",
    "dnsServer": "8.8.8.8"
  }'
```

**请求参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| domain | string | 是 | 要解析的域名 |
| recordType | string | 是 | 记录类型 (A, AAAA, CNAME, MX, TXT, NS, SOA) |
| dnsServer | string | 是 | DNS 服务器地址 |

**响应示例**:
```json
{
  "success": true,
  "data": {
    "domain": "www.baidu.com",
    "recordType": "A",
    "dnsServer": "8.8.8.8",
    "records": [
      "180.101.50.242",
      "180.101.50.188"
    ],
    "queryTime": 45
  }
}
```

### 1.3 批量对比解析

```bash
curl -X POST http://localhost:5000/api/v1/dns/compare \
  -H "Content-Type: application/json" \
  -d '{
    "domain": "www.baidu.com",
    "recordType": "A",
    "ispList": ["telecom", "unicom", "mobile", "aliyun", "google"]
  }'
```

**请求参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| domain | string | 是 | 要解析的域名 |
| recordType | string | 是 | 记录类型 |
| ispList | string[] | 是 | 运营商 ID 列表 |

**响应示例**:
```json
{
  "success": true,
  "data": {
    "domain": "www.baidu.com",
    "recordType": "A",
    "results": [
      {
        "ispId": "telecom",
        "ispName": "中国电信",
        "dnsServer": "202.96.128.86",
        "records": ["180.101.50.242"],
        "queryTime": 32,
        "success": true
      },
      {
        "ispId": "unicom",
        "ispName": "中国联通",
        "dnsServer": "221.5.88.88",
        "records": ["180.101.50.188"],
        "queryTime": 28,
        "success": true
      },
      {
        "ispId": "google",
        "ispName": "Google DNS",
        "dnsServer": "8.8.8.8",
        "records": ["180.101.50.242", "180.101.50.188"],
        "queryTime": 45,
        "success": true
      }
    ]
  }
}
```

---

## 2. DNS 记录管理

### 2.1 获取支持的服务商列表

```bash
curl -X GET http://localhost:5000/api/v1/providers
```

**响应示例**:
```json
{
  "success": true,
  "data": [
    { "name": "alidns", "displayName": "阿里云 DNS" },
    { "name": "tencentcloud", "displayName": "腾讯云 DNS" },
    { "name": "cloudflare", "displayName": "Cloudflare" },
    { "name": "dnspod", "displayName": "DNSPod" },
    { "name": "godaddy", "displayName": "GoDaddy" },
    { "name": "namecheap", "displayName": "Namecheap" },
    { "name": "namesilo", "displayName": "NameSilo" },
    { "name": "huaweicloud", "displayName": "华为云 DNS" }
  ]
}
```

### 2.2 获取域名列表

```bash
curl -X POST http://localhost:5000/api/v1/providers/domains \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-token",
    "secret": ""
  }'
```

**请求参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| providerName | string | 是 | 服务商名称 |
| id | string | 是 | API ID/Token |
| secret | string | 否 | API Secret (部分服务商需要) |

**响应示例**:
```json
{
  "success": true,
  "data": [
    "example.com",
    "example.org",
    "mysite.net"
  ]
}
```

### 2.3 获取 DNS 记录列表

```bash
curl -X POST http://localhost:5000/api/v1/providers/records/list \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-token",
    "secret": "",
    "domain": "example.com",
    "subDomain": "",
    "recordType": ""
  }'
```

**请求参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| providerName | string | 是 | 服务商名称 |
| id | string | 是 | API ID/Token |
| secret | string | 否 | API Secret |
| domain | string | 是 | 主域名 |
| subDomain | string | 否 | 子域名 (空表示所有) |
| recordType | string | 否 | 记录类型 (空表示所有) |

**响应示例**:
```json
{
  "success": true,
  "data": [
    {
      "recordId": "abc123",
      "domain": "example.com",
      "subDomain": "www",
      "recordType": "A",
      "value": "1.2.3.4",
      "ttl": 300
    },
    {
      "recordId": "def456",
      "domain": "example.com",
      "subDomain": "mail",
      "recordType": "MX",
      "value": "mail.example.com",
      "ttl": 3600
    }
  ]
}
```

### 2.4 添加 DNS 记录

```bash
curl -X POST http://localhost:5000/api/v1/providers/records/add \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-token",
    "secret": "",
    "domain": "example.com",
    "subDomain": "test",
    "recordType": "A",
    "value": "1.2.3.4",
    "ttl": 300
  }'
```

**请求参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| providerName | string | 是 | 服务商名称 |
| id | string | 是 | API ID/Token |
| secret | string | 否 | API Secret |
| domain | string | 是 | 主域名 |
| subDomain | string | 是 | 子域名 |
| recordType | string | 是 | 记录类型 |
| value | string | 是 | 记录值 |
| ttl | int | 否 | TTL 秒数 (默认 300) |

**响应示例**:
```json
{
  "success": true,
  "data": {
    "recordId": "new-record-id",
    "message": "Record created successfully"
  }
}
```

### 2.5 更新 DNS 记录

```bash
curl -X POST http://localhost:5000/api/v1/providers/records/update \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-token",
    "secret": "",
    "domain": "example.com",
    "subDomain": "test",
    "recordType": "A",
    "recordId": "existing-record-id",
    "value": "5.6.7.8",
    "ttl": 600
  }'
```

### 2.6 删除 DNS 记录

```bash
curl -X POST http://localhost:5000/api/v1/providers/records/delete \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-token",
    "secret": "",
    "domain": "example.com",
    "subDomain": "test",
    "recordType": "A",
    "recordId": "record-to-delete"
  }'
```

---

## 3. DDNS 动态域名解析

### 3.1 获取公网 IP

```bash
# 获取 IPv4
curl -X GET "http://localhost:5000/api/v1/ddns/ip?ipType=IPv4"

# 获取 IPv6
curl -X GET "http://localhost:5000/api/v1/ddns/ip?ipType=IPv6"
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "ip": "123.45.67.89",
    "ipType": "IPv4"
  }
}
```

### 3.2 更新 DDNS 记录

```bash
curl -X POST http://localhost:5000/api/v1/ddns/update \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-token",
    "secret": "",
    "domain": "example.com",
    "subDomain": "home",
    "recordType": "A",
    "ipType": "IPv4",
    "ttl": 300,
    "forceUpdate": false
  }'
```

**请求参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| providerName | string | 是 | 服务商名称 |
| id | string | 是 | API ID/Token |
| secret | string | 否 | API Secret |
| domain | string | 是 | 主域名 |
| subDomain | string | 是 | 子域名 |
| recordType | string | 是 | 记录类型 (A 或 AAAA) |
| ipType | string | 是 | IP 类型 (IPv4 或 IPv6) |
| ttl | int | 否 | TTL 秒数 |
| forceUpdate | bool | 否 | 是否强制更新 |

**响应示例**:
```json
{
  "success": true,
  "data": {
    "updated": true,
    "ip": "123.45.67.89",
    "message": "DNS record updated successfully"
  }
}
```

---

## 4. DDNS 定时任务管理

### 4.1 获取任务列表

```bash
curl -X GET http://localhost:5000/api/v1/ddns/tasks
```

**响应示例**:
```json
{
  "success": true,
  "data": [
    {
      "id": "task-uuid-1",
      "providerName": "cloudflare",
      "domain": "example.com",
      "subDomain": "home",
      "recordType": "A",
      "ipType": "IPv4",
      "intervalMinutes": 5,
      "enabled": true,
      "lastRunTime": "2026-01-14T10:30:00Z",
      "lastIp": "123.45.67.89",
      "lastStatus": "Success"
    }
  ]
}
```

### 4.2 创建定时任务

```bash
curl -X POST http://localhost:5000/api/v1/ddns/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-token",
    "secret": "",
    "domain": "example.com",
    "subDomain": "home",
    "recordType": "A",
    "ipType": "IPv4",
    "ttl": 300,
    "intervalMinutes": 5,
    "enabled": true
  }'
```

### 4.3 更新定时任务

```bash
curl -X PUT http://localhost:5000/api/v1/ddns/tasks/{taskId} \
  -H "Content-Type: application/json" \
  -d '{
    "intervalMinutes": 10,
    "enabled": true
  }'
```

### 4.4 删除定时任务

```bash
curl -X DELETE http://localhost:5000/api/v1/ddns/tasks/{taskId}
```

---

## 5. 错误处理

所有 API 在发生错误时返回统一格式：

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Domain name is required"
  }
}
```

**常见错误码**:
| 错误码 | 说明 |
|--------|------|
| VALIDATION_ERROR | 请求参数验证失败 |
| PROVIDER_NOT_FOUND | 不支持的服务商 |
| AUTH_FAILED | API 认证失败 |
| DNS_QUERY_FAILED | DNS 查询失败 |
| RECORD_NOT_FOUND | DNS 记录不存在 |
| RATE_LIMITED | 请求频率超限 |

---

## 6. 各服务商认证配置

### Cloudflare
- **id**: API Token (推荐) 或 Global API Key
- **secret**: 留空 (使用 Token) 或 Email (使用 Global Key)

### 阿里云 DNS
- **id**: AccessKey ID
- **secret**: AccessKey Secret

### 腾讯云 DNS
- **id**: SecretId
- **secret**: SecretKey

### DNSPod
- **id**: Token ID
- **secret**: Token

### GoDaddy
- **id**: API Key
- **secret**: API Secret

### Namecheap
- **id**: API User
- **secret**: API Key

### 华为云 DNS
- **id**: Access Key
- **secret**: Secret Key

---

## 7. 代码示例

### Python

```python
import requests

BASE_URL = "http://localhost:5000"

# DNS 解析
def resolve_dns(domain, record_type, dns_server):
    response = requests.post(f"{BASE_URL}/api/v1/dns/resolve", json={
        "domain": domain,
        "recordType": record_type,
        "dnsServer": dns_server
    })
    return response.json()

# 批量对比
def compare_dns(domain, record_type, isp_list):
    response = requests.post(f"{BASE_URL}/api/v1/dns/compare", json={
        "domain": domain,
        "recordType": record_type,
        "ispList": isp_list
    })
    return response.json()

# 使用示例
result = resolve_dns("www.baidu.com", "A", "8.8.8.8")
print(result)

comparison = compare_dns("www.baidu.com", "A", ["telecom", "unicom", "google"])
print(comparison)
```

### JavaScript/TypeScript

```typescript
const BASE_URL = "http://localhost:5000";

// DNS 解析
async function resolveDns(domain: string, recordType: string, dnsServer: string) {
  const response = await fetch(`${BASE_URL}/api/v1/dns/resolve`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ domain, recordType, dnsServer }),
  });
  return response.json();
}

// 批量对比
async function compareDns(domain: string, recordType: string, ispList: string[]) {
  const response = await fetch(`${BASE_URL}/api/v1/dns/compare`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ domain, recordType, ispList }),
  });
  return response.json();
}

// 使用示例
const result = await resolveDns("www.baidu.com", "A", "8.8.8.8");
console.log(result);

const comparison = await compareDns("www.baidu.com", "A", ["telecom", "unicom", "google"]);
console.log(comparison);
```

### Go

```go
package main

import (
    "bytes"
    "encoding/json"
    "fmt"
    "net/http"
)

const baseURL = "http://localhost:5000"

type ResolveRequest struct {
    Domain     string `json:"domain"`
    RecordType string `json:"recordType"`
    DnsServer  string `json:"dnsServer"`
}

func resolveDns(domain, recordType, dnsServer string) (map[string]interface{}, error) {
    reqBody, _ := json.Marshal(ResolveRequest{
        Domain:     domain,
        RecordType: recordType,
        DnsServer:  dnsServer,
    })

    resp, err := http.Post(
        baseURL+"/api/v1/dns/resolve",
        "application/json",
        bytes.NewBuffer(reqBody),
    )
    if err != nil {
        return nil, err
    }
    defer resp.Body.Close()

    var result map[string]interface{}
    json.NewDecoder(resp.Body).Decode(&result)
    return result, nil
}

func main() {
    result, _ := resolveDns("www.baidu.com", "A", "8.8.8.8")
    fmt.Printf("%+v\n", result)
}
```
