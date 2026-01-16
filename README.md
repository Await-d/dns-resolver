# DNS Resolver

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.x-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**多运营商 DNS 解析管理面板 | Multi-ISP DNS Resolution Management Panel**

[English](./README_EN.md) | 简体中文

</div>

---

## 📖 项目简介

DNS Resolver 是一个功能强大的多运营商域名解析管理面板，提供以下核心功能：

- 🔍 **DNS 解析查询** - 支持同时查询多个运营商的 DNS 解析结果并进行对比
- 🌐 **DNS 记录管理** - 集成 23 个主流域名服务商，支持完整的 DNS 记录 CRUD 操作
- 🔄 **DDNS 动态解析** - 自动检测公网 IP 变化并更新 DNS 记录
- 🎨 **现代化 UI** - 赛博朋克风格界面，支持中英文切换，完美适配移动端

## ✨ 功能特性

### DNS 解析查询
- 支持 8 个运营商同时查询（电信、联通、移动、阿里、腾讯、百度、Google、Cloudflare）
- 一键对比多运营商解析结果，快速发现解析差异
- 支持 A、AAAA、CNAME、MX、TXT、NS、SOA 等记录类型
- 显示每次查询的响应时间

### DNS 记录管理
- 集成 23 个主流域名服务商 API
- 支持查询、添加、更新、删除 DNS 记录
- 支持批量操作
- 用户服务商配置持久化存储

### DDNS 动态域名
- 自动获取公网 IP（支持 IPv4/IPv6）
- 定时任务自动检测 IP 变化
- 仅在 IP 变化时更新，减少 API 调用
- 支持多个 IP 获取源

## 🛠️ 技术栈

### 后端
- **.NET 10.0** - 最新 .NET 运行时
- **ASP.NET Core** - Web API 框架
- **Entity Framework Core** - ORM（SQLite）
- **DDD 架构** - 领域驱动设计
- **JWT 认证** - 安全的用户认证

### 前端
- **React 19** - 最新 React 框架
- **TypeScript 5** - 类型安全
- **Vite 6** - 快速构建工具
- **TanStack Query** - 数据请求管理
- **Tailwind CSS 4** - 原子化 CSS 框架
- **i18next** - 国际化支持

## 🚀 快速开始

### 使用 Docker（推荐）

```bash
# 克隆项目
git clone https://github.com/Await-d/dns-resolver.git
cd dns-resolver

# 启动服务
docker compose up -d
```

服务将在 `http://localhost:7010` 启动。

**默认账号**：admin / admin123

### 手动部署

#### 环境要求
- .NET SDK 10.0+
- Node.js 22.x LTS+
- pnpm 9.x（推荐）

#### 后端

```bash
cd backend/src/DnsResolver.Api
dotnet restore
dotnet run
```

#### 前端

```bash
cd frontend
pnpm install
pnpm dev
```

## 📦 支持的服务商

### DNS 运营商

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

### 域名服务商（23 个）

<details>
<summary>点击展开完整列表</summary>

| 服务商 | ID | 说明 |
|--------|-----|------|
| 阿里云 DNS | alidns | 阿里云域名解析 |
| 阿里云 ESA | aliesa | 阿里云边缘安全加速 |
| 腾讯云 DNS | tencentcloud | 腾讯云域名解析 |
| 腾讯云 EdgeOne | edgeone | 腾讯云边缘安全加速 |
| DNSPod | dnspod | DNSPod 域名解析 |
| Cloudflare | cloudflare | Cloudflare DNS |
| 华为云 DNS | huaweicloud | 华为云域名解析 |
| 百度云 DNS | baiducloud | 百度云域名解析 |
| GoDaddy | godaddy | GoDaddy 域名服务 |
| Namecheap | namecheap | Namecheap 域名服务 |
| Namesilo | namesilo | Namesilo 域名服务 |
| Porkbun | porkbun | Porkbun 域名服务 |
| Vercel | vercel | Vercel DNS |
| Gcore | gcore | Gcore DNS |
| NS1 | nsone | NS1 DNS |
| Dynadot | dynadot | Dynadot 域名服务 |
| DNSLA | dnsla | DNSLA 域名解析 |
| Dynv6 | dynv6 | Dynv6 动态 DNS |
| Spaceship | spaceship | Spaceship 域名服务 |
| TrafficRoute | trafficroute | TrafficRoute DNS |
| Eranet | eranet | 时代互联 |
| NowCN | nowcn | 现在网 |
| Callback | callback | 自定义回调 |

</details>

## 📁 项目结构

```
dns-resolver/
├── backend/                          # 后端项目
│   └── src/
│       ├── DnsResolver.Domain/       # 领域层 - 核心业务逻辑
│       ├── DnsResolver.Application/  # 应用层 - 用例编排
│       ├── DnsResolver.Infrastructure/ # 基础设施层 - 外部依赖
│       └── DnsResolver.Api/          # 表现层 - HTTP API
├── frontend/                         # 前端项目
│   └── src/
│       ├── components/               # 可复用组件
│       ├── pages/                    # 页面组件
│       ├── services/                 # API 调用
│       ├── hooks/                    # 自定义 Hooks
│       ├── contexts/                 # React Context
│       ├── i18n/                     # 国际化配置
│       └── types/                    # TypeScript 类型
├── docker-compose.yml                # Docker Compose 配置
├── Dockerfile                        # Docker 构建文件
└── README.md                         # 项目说明
```

## 🔧 配置说明

### 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `ASPNETCORE_ENVIRONMENT` | 运行环境 | Production |
| `Jwt__SecretKey` | JWT 密钥 | 随机生成 |
| `Cors__Origins` | 允许的跨域来源 | http://localhost:5173 |

### 后端配置

编辑 `backend/src/DnsResolver.Api/appsettings.json`：

```json
{
  "DnsSettings": {
    "QueryTimeoutSeconds": 5,
    "MaxRetries": 2
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "DnsResolver",
    "Audience": "DnsResolver"
  }
}
```

## 📸 界面预览

<details>
<summary>点击查看截图</summary>

- 首页 - DNS 记录管理
- DNS 解析对比
- DDNS 任务管理
- 服务商配置

</details>

## 🤝 贡献指南

欢迎贡献代码、报告问题或提出建议！

1. Fork 本项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 [MIT](LICENSE) 许可证。

## 🙏 致谢

- DNS 服务商集成参考了 [ddns-go](https://github.com/jeessy2/ddns-go) 项目
- 感谢所有贡献者的支持

## 📮 联系方式

- 项目主页: https://github.com/Await-d/dns-resolver
- 问题反馈: https://github.com/Await-d/dns-resolver/issues

---

<div align="center">

**如果这个项目对你有帮助，请给一个 ⭐ Star 支持一下！**

</div>
