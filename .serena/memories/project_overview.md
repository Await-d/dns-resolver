# DNS Resolver 项目概述

## 项目目的
DNS Resolver 是一个多运营商域名解析管理面板，支持查询和对比不同运营商（电信、联通、移动等）的 DNS 解析结果。

## 技术栈

### 后端
- .NET 10.0 + ASP.NET Core
- DDD 四层架构 (Domain, Application, Infrastructure, Api)
- SQLite 数据持久化
- JWT 认证

### 前端
- React 19 + Vite 6 + TypeScript 5
- TanStack Query 5
- Tailwind CSS 4
- i18next 国际化 (中文/英文)

## 项目结构
```
dns-resolver/
├── backend/src/
│   ├── DnsResolver.Domain/         # 领域层
│   ├── DnsResolver.Application/    # 应用层
│   ├── DnsResolver.Infrastructure/ # 基础设施层
│   └── DnsResolver.Api/            # 表现层
└── frontend/src/
    ├── components/    # 组件
    ├── pages/         # 页面
    ├── services/      # API 服务
    ├── contexts/      # React Context
    ├── hooks/         # 自定义 Hooks
    ├── types/         # TypeScript 类型
    └── i18n/          # 国际化
```

## 主要功能
1. DNS 查询 - 查询域名解析结果
2. DNS 管理 - 管理 DNS 服务商配置
3. DDNS 动态域名 - 动态 DNS 更新任务
4. 用户认证 - 登录、修改密码

## 开发命令

### 后端
```bash
cd backend/src/DnsResolver.Api
dotnet restore
dotnet run
```

### 前端
```bash
cd frontend
pnpm install
pnpm dev
```

## 代码风格
- 后端: C# 命名规范，DDD 架构模式
- 前端: TypeScript，函数式组件，Tailwind CSS
