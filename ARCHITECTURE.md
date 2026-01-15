# DNS Resolver 技术架构文档

## 项目概述

DNS Resolver 是一个多运营商域名解析管理面板，支持查询和对比不同运营商（电信、联通、移动等）的 DNS 解析结果。

---

## 技术栈

### 后端
| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 10.0 | 运行时框架 |
| ASP.NET Core | 10.0 | Web API 框架 |
| DnsClient.NET | latest | DNS 查询库 |

### 前端
| 技术 | 版本 | 用途 |
|------|------|------|
| React | 19.x | UI 框架 |
| Vite | 6.x | 构建工具 |
| TypeScript | 5.x | 类型安全 |
| TanStack Query | 5.x | 数据请求管理 |
| Tailwind CSS | 4.x | 样式框架 |

---

## 后端架构 (DDD)

采用领域驱动设计 (Domain-Driven Design) 四层架构：

```
┌─────────────────────────────────────────────────────────────┐
│                      DnsResolver.Api                         │
│                        (表现层)                              │
│  Controllers, Requests, Responses, Middleware                │
└─────────────────────────┬───────────────────────────────────┘
                          │ 依赖
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  DnsResolver.Application                     │
│                        (应用层)                              │
│  Commands, Queries, Handlers, DTOs                           │
└─────────────────────────┬───────────────────────────────────┘
                          │ 依赖
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    DnsResolver.Domain                        │
│                        (领域层)                              │
│  Aggregates, ValueObjects, Services, Exceptions              │
└─────────────────────────────────────────────────────────────┘
                          ▲
                          │ 实现接口
┌─────────────────────────┴───────────────────────────────────┐
│                 DnsResolver.Infrastructure                   │
│                      (基础设施层)                            │
│  DnsClient Adapter, Repositories, Configuration              │
└─────────────────────────────────────────────────────────────┘
```

### 依赖规则

- **Domain 层**: 核心业务逻辑，不依赖任何其他层
- **Application 层**: 编排用例，只依赖 Domain 层
- **Infrastructure 层**: 实现 Domain 层定义的接口
- **Api 层**: HTTP 入口，依赖 Application 层

---

## 项目结构

```
dns-resolver/
├── backend/
│   └── src/
│       ├── DnsResolver.Domain/              # 领域层 (核心)
│       │   ├── Aggregates/
│       │   │   ├── DnsQuery/
│       │   │   │   ├── DnsQuery.cs          # 聚合根
│       │   │   │   └── ResolveResult.cs     # 实体
│       │   │   └── IspProvider/
│       │   │       ├── IspProvider.cs       # 聚合根
│       │   │       └── IIspProviderRepository.cs
│       │   ├── ValueObjects/
│       │   │   ├── DomainName.cs            # 域名 (带验证)
│       │   │   ├── DnsServer.cs             # DNS服务器地址
│       │   │   ├── DnsRecord.cs             # DNS记录
│       │   │   └── RecordType.cs            # 记录类型
│       │   ├── Services/
│       │   │   └── IDnsResolutionService.cs # 领域服务接口
│       │   └── Exceptions/
│       │       ├── DomainException.cs
│       │       ├── InvalidDomainException.cs
│       │       └── DnsResolutionException.cs
│       │
│       ├── DnsResolver.Application/         # 应用层
│       │   ├── Commands/
│       │   │   ├── ResolveDns/
│       │   │   │   ├── ResolveDnsCommand.cs
│       │   │   │   ├── ResolveDnsResult.cs
│       │   │   │   └── ResolveDnsCommandHandler.cs
│       │   │   └── CompareDns/
│       │   │       ├── CompareDnsCommand.cs
│       │   │       ├── CompareDnsResult.cs
│       │   │       └── CompareDnsCommandHandler.cs
│       │   ├── Queries/
│       │   │   └── GetIsps/
│       │   │       ├── GetIspsQuery.cs
│       │   │       └── GetIspsQueryHandler.cs
│       │   ├── DTOs/
│       │   │   ├── DnsRecordDto.cs
│       │   │   └── IspProviderDto.cs
│       │   └── Interfaces/
│       │
│       ├── DnsResolver.Infrastructure/      # 基础设施层
│       │   ├── DnsClient/
│       │   │   └── DnsClientAdapter.cs      # DnsClient.NET 适配器
│       │   ├── Repositories/
│       │   │   └── InMemoryIspProviderRepository.cs
│       │   └── Configuration/
│       │       └── DnsSettings.cs
│       │
│       └── DnsResolver.Api/                 # 表现层
│           ├── Controllers/
│           │   └── DnsController.cs
│           ├── Requests/
│           │   ├── ResolveRequest.cs
│           │   └── CompareRequest.cs
│           ├── Responses/
│           │   └── ApiResponse.cs
│           ├── Middleware/
│           │   └── ExceptionMiddleware.cs
│           ├── Program.cs
│           └── appsettings.json
│
└── frontend/
    ├── src/
    │   ├── components/                # 可复用组件
    │   │   ├── DnsQueryForm.tsx
    │   │   ├── ResultTable.tsx
    │   │   └── IspSelector.tsx
    │   ├── pages/                     # 页面组件
    │   │   └── HomePage.tsx
    │   ├── services/                  # API 调用
    │   │   └── dnsApi.ts
    │   ├── hooks/                     # 自定义 Hooks
    │   │   └── useDnsQuery.ts
    │   ├── types/                     # TypeScript 类型
    │   │   └── dns.ts
    │   ├── App.tsx
    │   └── main.tsx
    ├── index.html
    ├── vite.config.ts
    ├── tsconfig.json
    └── package.json
```

---

## 领域模型

### 值对象 (Value Objects)

```csharp
// DomainName.cs - 域名值对象，包含格式验证
public sealed class DomainName : IEquatable<DomainName>
{
    public string Value { get; }
    public static DomainName Create(string value);  // 验证域名格式
}

// RecordType.cs - DNS记录类型
public sealed class RecordType : IEquatable<RecordType>
{
    public static readonly RecordType A, AAAA, CNAME, MX, TXT, NS, SOA;
    public string Value { get; }
    public static RecordType Create(string value);  // 验证记录类型
}

// DnsServer.cs - DNS服务器地址
public sealed class DnsServer : IEquatable<DnsServer>
{
    public string Address { get; }
    public int Port { get; }
    public static DnsServer Create(string address, int port = 53);
}

// DnsRecord.cs - DNS记录
public sealed record DnsRecord(string Value, int Ttl, RecordType Type);
```

### 聚合根 (Aggregates)

```csharp
// DnsQuery.cs - DNS查询聚合根
public class DnsQuery
{
    public Guid Id { get; }
    public DomainName Domain { get; }
    public RecordType RecordType { get; }
    public DnsServer DnsServer { get; }
    public string? IspName { get; }
    public ResolveResult? Result { get; }

    public static DnsQuery Create(DomainName domain, RecordType recordType, DnsServer dnsServer, string? ispName);
    public void SetResult(IReadOnlyList<DnsRecord> records, int queryTimeMs);
    public void SetError(string errorMessage);
}

// IspProvider.cs - ISP运营商聚合根
public class IspProvider
{
    public string Id { get; }
    public string Name { get; }
    public DnsServer PrimaryDns { get; }
    public DnsServer? SecondaryDns { get; }
}
```

### 领域服务接口

```csharp
// IDnsResolutionService.cs
public interface IDnsResolutionService
{
    Task<DnsQuery> ResolveAsync(
        DomainName domain, RecordType recordType, DnsServer dnsServer,
        string? ispName = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DnsQuery>> BatchResolveAsync(
        DomainName domain, RecordType recordType, IEnumerable<IspProvider> providers,
        CancellationToken cancellationToken = default);
}

// IIspProviderRepository.cs
public interface IIspProviderRepository
{
    Task<IReadOnlyList<IspProvider>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IspProvider?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IspProvider?> FindByDnsServerAsync(string dnsServer, CancellationToken cancellationToken = default);
}
```

---

## 应用层 (CQRS 模式)

### Commands (写操作)

```csharp
// ResolveDnsCommand.cs
public record ResolveDnsCommand(string Domain, string RecordType, string DnsServer);
public record ResolveDnsResult(string Domain, string RecordType, string DnsServer,
    string IspName, IReadOnlyList<DnsRecordDto> Records, int QueryTimeMs, bool Success, string? ErrorMessage);

// CompareDnsCommand.cs
public record CompareDnsCommand(string Domain, string RecordType, List<string> IspIds);
public record CompareDnsResult(string Domain, string RecordType, IReadOnlyList<ResolveDnsResult> Results);
```

### Queries (读操作)

```csharp
// GetIspsQuery.cs
public record GetIspsQuery;
// Handler 返回 IReadOnlyList<IspProviderDto>
```

---

## 支持的运营商 DNS

| 运营商 | ID | 主 DNS | 备用 DNS |
|--------|-----|--------|----------|
| 中国电信 | telecom | 202.96.128.86 | 202.96.128.166 |
| 中国联通 | unicom | 221.5.88.88 | 221.6.4.66 |
| 中国移动 | mobile | 211.136.192.6 | 211.136.112.50 |
| 阿里 DNS | aliyun | 223.5.5.5 | 223.6.6.6 |
| 腾讯 DNS | tencent | 119.29.29.29 | 119.28.28.28 |
| 百度 DNS | baidu | 180.76.76.76 | - |
| Google DNS | google | 8.8.8.8 | 8.8.4.4 |
| Cloudflare | cloudflare | 1.1.1.1 | 1.0.0.1 |

### 支持的记录类型

- A (IPv4 地址)
- AAAA (IPv6 地址)
- CNAME (别名记录)
- MX (邮件交换)
- TXT (文本记录)
- NS (域名服务器)
- SOA (授权起始)

---

## API 设计

### 基础路径
```
/api/v1/dns
```

### 接口列表

#### 1. 获取运营商列表
```http
GET /api/v1/dns/isps
```

**响应:**
```json
{
  "success": true,
  "data": [
    { "id": "telecom", "name": "中国电信", "primaryDns": "202.96.128.86", "secondaryDns": "202.96.128.166" },
    { "id": "aliyun", "name": "阿里DNS", "primaryDns": "223.5.5.5", "secondaryDns": "223.6.6.6" }
  ]
}
```

#### 2. 单次解析
```http
POST /api/v1/dns/resolve
Content-Type: application/json

{
  "domain": "example.com",
  "recordType": "A",
  "dnsServer": "223.5.5.5"
}
```

**响应:**
```json
{
  "success": true,
  "data": {
    "domain": "example.com",
    "recordType": "A",
    "dnsServer": "223.5.5.5",
    "ispName": "阿里DNS",
    "records": [
      { "value": "93.184.216.34", "ttl": 3600, "recordType": "A" }
    ],
    "queryTimeMs": 23,
    "success": true,
    "errorMessage": null
  }
}
```

#### 3. 批量对比解析
```http
POST /api/v1/dns/compare
Content-Type: application/json

{
  "domain": "example.com",
  "recordType": "A",
  "ispList": ["telecom", "unicom", "mobile", "aliyun"]
}
```

---

## 配置管理

ISP 配置已从代码移至 `appsettings.json`:

```json
{
  "DnsSettings": {
    "QueryTimeoutSeconds": 5,
    "Retries": 2,
    "Isps": [
      { "Id": "telecom", "Name": "中国电信", "PrimaryDns": "202.96.128.86", "SecondaryDns": "202.96.128.166" },
      { "Id": "aliyun", "Name": "阿里DNS", "PrimaryDns": "223.5.5.5", "SecondaryDns": "223.6.6.6" }
    ]
  }
}
```

---

## 前端类型 (TypeScript)

```typescript
// types/dns.ts
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

export type RecordType = 'A' | 'AAAA' | 'CNAME' | 'MX' | 'TXT' | 'NS' | 'SOA';
```

---

## 快速开始

### 后端启动
```bash
cd backend/src/DnsResolver.Api
dotnet restore
dotnet run
```

### 前端启动
```bash
cd frontend
pnpm install
pnpm dev
```

---

## 开发环境要求

### 后端
- .NET SDK 10.0+
- Visual Studio 2024 / VS Code / Rider

### 前端
- Node.js 22.x LTS
- pnpm 9.x (推荐) 或 npm

---

## 后续扩展计划

1. **历史记录** - 保存查询历史，支持导出
2. **定时监控** - 定期检测域名解析变化
3. **告警通知** - 解析异常时发送通知
4. **批量导入** - 支持批量域名查询
5. **API 密钥** - 支持第三方调用
6. **MediatR** - 引入中间件管道
7. **领域事件** - 发布/订阅模式

---

## 许可证

MIT License
