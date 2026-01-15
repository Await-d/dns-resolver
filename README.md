# DNS Resolver - 多运营商域名解析管理面板

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.x-61DAFB?logo=react)](https://react.dev/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

一个功能强大的多运营商域名解析管理面板，支持查询和对比不同运营商（电信、联通、移动等）的 DNS 解析结果，并集成 23 个主流域名服务商的 DNS 记录管理功能。

## 功能特性

### DNS 解析查询
- **多运营商对比**: 支持同时查询 8 个运营商的 DNS 解析结果（电信、联通、移动、阿里、腾讯、百度、Google、Cloudflare）
- **单次解析**: 指定 DNS 服务器进行单次域名解析
- **批量对比**: 一键对比多个运营商的解析结果，快速发现解析差异
- **多种记录类型**: 支持 A、AAAA、CNAME、MX、TXT、NS、SOA 等记录类型
- **性能监控**: 显示每次查询的响应时间

### DNS 记录管理
- **23 个服务商支持**: 集成阿里云、腾讯云、Cloudflare、GoDaddy、Namecheap 等 23 个主流域名服务商
- **完整 CRUD 操作**: 查询、添加、更新、删除 DNS 记录
- **批量操作**: 支持批量添加和删除 DNS 记录
- **域名列表**: 获取服务商下的所有域名

### DDNS 动态域名解析
- **自动获取公网 IP**: 支持 IPv4 和 IPv6
- **智能更新**: 仅在 IP 变化时更新 DNS 记录
- **强制更新**: 支持手动强制更新
- **多服务商支持**: 可配合任意支持的 DNS 服务商使用

## 技术架构

### 后端技术栈
- **.NET 10.0**: 最新的 .NET 运行时
- **ASP.NET Core**: Web API 框架
- **DDD 架构**: 领域驱动设计，清晰的分层架构
- **DnsClient.NET**: 高性能 DNS 查询库
- **Swagger/OpenAPI**: 完整的 API 文档

### 前端技术栈
- **React 19.x**: 最新的 React 框架
- **TypeScript 5.x**: 类型安全
- **Vite 6.x**: 快速的构建工具
- **TanStack Query**: 强大的数据请求管理
- **Tailwind CSS 4.x**: 现代化的样式框架

## 快速开始

### 环境要求

#### 后端
- .NET SDK 10.0 或更高版本
- 支持的操作系统: Windows、Linux、macOS

#### 前端
- Node.js 22.x LTS 或更高版本
- pnpm 9.x（推荐）或 npm

### 安装步骤

#### 1. 克隆项目
```bash
git clone https://github.com/yourusername/dns-resolver.git
cd dns-resolver
```

#### 2. 启动后端

```bash
# 进入后端目录
cd backend/src/DnsResolver.Api

# 还原依赖
dotnet restore

# 运行项目
dotnet run
```

后端 API 将在 `http://localhost:5000` 启动，Swagger 文档可在 `http://localhost:5000/swagger` 访问。

#### 3. 启动前端

```bash
# 进入前端目录
cd frontend

# 安装依赖
pnpm install

# 启动开发服务器
pnpm dev
```

前端应用将在 `http://localhost:5173` 启动。

### 使用 Docker Compose（推荐）

```bash
# 在项目根目录执行
docker-compose up -d
```

服务将在以下地址启动：
- 前端: `http://localhost:5173`
- 后端 API: `http://localhost:5000`
- Swagger 文档: `http://localhost:5000/swagger`

## 使用指南

### DNS 解析查询

#### 1. 获取支持的运营商列表

访问 Swagger 文档或直接调用 API：

```bash
curl http://localhost:5000/api/v1/dns/isps
```

#### 2. 单次 DNS 解析

```bash
curl -X POST http:i/v1/dns/compare \
  -Happlication/json" \
  -d '{
    "domain": "example.com",
    "recordType": "A",
    "ispList": ["telecom", "unicom", "mobile", "aliyun"]
  }'
```

### DNS 记录管理

#### 1. 获取支持的服务商列表

```bash
curl http://localhost:5000/api/v1/providers
```

#### 2. 获取域名列表

```bash
curl -X POST http://localhost:5000/api/v1/providers/domains \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-key",
    "secret": "your-api-secret"
  }'
```

#### 3. 获取 DNS 记录

```bash
curl -X POST http://localhost:5000/api/v1/providers/records/list \
  -H "Content-Type: application/json" \
  -d '{
    "providerName": "cloudflare",
    "id": "your-api-key",
    "secret": "your-api-secret",
    "domain": "example.com",
    "subDomain": "www",
    "recordType": "A"
  }'
```

#### 4. 添加 DNS 记n  -d '{
    "providerName": "cloudflare",
    "id": "your-api-key",
    "secret": "your-api-secret",
    "domain": "example.com",
   ecret": "your-api-secret",
    "domain": "example.com",
    "recordId": "record-i2.3.4",
    "ttl": 600,
    "forceUpdate": false
  }'
```

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

## 支持的域名服务商

本项目集成了 23 个主流域名服务商的 API，支持完整的 DNS 记录管理功能：

1. **阿里云 DNS** (alidns)
2. **阿里云 ESA** (aliesa)
3. **腾讯云 DNS** (tencentcloud)
4. **腾讯云 EdgeOne** (edgeone)
5. **DNSPod**"中国电信",
        "PrimaryDns": "202.96.128yDns": "202.96.128.166"
      }
    ]
  }
}
```

### 前端配置

编辑 `frontend/.env`：

```env
VITE_API_BASE_URL=http://localhost:5000
```

## API 文档

完整的 API 文档可通过以下方式访问：

1. **Swagger UI**: 启动后端后访问 `http://localhost:5000/swagger`
2. **OpenAPI JSON**: `http://localhost:5000/swagger/v1/swagger.json`
3. **详细示例**: 查看 [docs/api-examples.md](docs/api-examples.md)

## 项目结构

```
dns-resolver/
├── backend/                    # 后端项目
│   └── src/
│       ├── DnsResolver.Domain/         # 领域层（核心业务逻辑）
│       ├── DnsResolver.Application/    # 应用层（用例编排）
│       ├── DnsResolver.Infrastructure/ # 基础设施层（外部依赖）
│       └── DnsResolver.Api/            # 表现层（HTTP API）
├── frontend/                   # 前端项目
│   ├── src/
│   │   ├── components/        # 可复用组件
│   │   ├── pages/             # 页面组件
│   │   ├── services/          # API 调用
│   │   └── types/             # TypeScript 类型
│   └── package.json
├── docs/                       # 文档目录
│   └── api-examples.md        # API 使用示例
├── docker-compose.yml          # Docker Compose 配置
├── ARCHITECTURE.md             # 技术架构文档
└── README.md                   # 本文件
```

## 开发指南

### 后端开发

```bash
cd backend/src/DnsResolver.Api

# 运行测试
dotnet test

# 构建项目
dotnet build

# 发布项目
dotnet publish -c Release -o ./publish
```

### 前端开发

```bash
cd frontend

# 运行开发服务器
pnpm dev

# 构建生产版本
pnpm build

# 预览生产版本
pnpm preview

# 代码检查
pnpm lint
```

## 常见问题

### 1. DNS 查询超时

如果遇到 DNS 查询超时，可以尝试：
- 增加 `appsettings.json` 中的 `QueryTimeoutSeconds` 值
- 检查网络连接和防火墙设置
- 尝试使用其他 DNS 服务器

### 2. 服务商 API 认证失败

确保：
- API 密钥和密钥正确无误
- API 密钥具有足够的权限
- 检查服务商的 API 文档了解具体要求

### 3. CORS 错误

如果前端访问后端时遇到 CORS 错误：
- 确保 `appsettings.json` 中的 `Cors:Origins` 包含前端地址
- 开发环境默认允许 `http://localhost:5173`

## 贡献指南

欢迎贡献代码、报告问题或提出建议！

1. Fork 本项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 致谢

- DNS 服务商集成参考了 [ddns-go](https://github.com/jeessy2/ddns-go) 项目
- 感谢所有贡献者的支持

## 联系方式

- 项目主页: https://github.com/yourusername/dns-resolver
- 问题反馈: https://github.com/yourusername/dns-resolver/issues

---

**注意**: 本项目仅供学习和研究使用，请遵守相关服务商的使用条款。
