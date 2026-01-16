# 开发常用命令

## 后端

### 构建
```bash
cd backend/src/DnsResolver.Api
dotnet build
```

### 运行
```bash
cd backend/src/DnsResolver.Api
dotnet run
```

### 测试
```bash
cd backend/src/DnsResolver.Tests
dotnet test
```

## 前端

### 安装依赖
```bash
cd frontend
pnpm install
```

### 开发模式
```bash
cd frontend
pnpm dev
```

### 构建
```bash
cd frontend
pnpm build
```

### 类型检查
```bash
cd frontend
pnpm run build  # tsc -b && vite build
```

## 系统工具
- `git` - 版本控制
- `ls`, `cd`, `grep`, `find` - 文件操作
- `cat`, `head`, `tail` - 文件查看
