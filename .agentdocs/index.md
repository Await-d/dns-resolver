# DNS Resolver 项目知识库

## 项目概述

DNS Resolver 是一个多运营商域名解析管理面板，支持查询和对比不同运营商（电信、联通、移动等）的 DNS 解析结果。

## 技术栈

- **后端**: .NET 10.0 + ASP.NET Core + DnsClient.NET (DDD 架构)
- **前端**: React 19 + Vite 6 + TypeScript 5 + TanStack Query 5 + Tailwind CSS 4

## 项目结构

```
dns-resolver/
├── backend/           # .NET 后端服务 (DDD 架构)
│   └── src/
│       ├── DnsResolver.Domain/         # 领域层
│       ├── DnsResolver.Application/    # 应用层
│       ├── DnsResolver.Infrastructure/ # 基础设施层
│       └── DnsResolver.Api/            # 表现层
└── frontend/          # React 前端应用
    └── src/
        ├── components/
        ├── pages/
        ├── services/
        ├── hooks/
        └── types/
```

## 相关文档

- `ARCHITECTURE.md` - 完整技术架构文档
- `git-commit-rules.md` - Git 提交规范

---

## 当前任务文档

- `workflow/260115-数据持久化.md` - 将内存存储改为 SQLite 持久化

---

## 已完成任务

- `workflow/done/260115-ddns前端页面.md` - DDNS 动态域名管理前端页面 ✅
- `workflow/done/260114-项目完善.md` - 测试、部署、文档、功能增强 ✅
- `workflow/done/260114-项目初始化开发.md` - 根据架构文档实现完整功能 ✅
- `workflow/done/260114-后端DDD架构重构.md` - 后端 DDD 架构重构 ✅
- `workflow/done/260114-集成ddns-go域名服务商.md` - 集成 ddns-go 23 个域名服务商 ✅

---

## 知识备忘

### 运营商 DNS 服务器

| 运营商 | 主 DNS | 备用 DNS |
|--------|--------|----------|
| 中国电信 | 202.96.128.86 | 202.96.128.166 |
| 中国联通 | 221.5.88.88 | 221.6.4.66 |
| 中国移动 | 211.136.192.6 | 211.136.112.50 |
| 阿里 DNS | 223.5.5.5 | 223.6.6.6 |
| 腾讯 DNS | 119.29.29.29 | 119.28.28.28 |

### 支持的记录类型

A, AAAA, CNAME, MX, TXT, NS, SOA
