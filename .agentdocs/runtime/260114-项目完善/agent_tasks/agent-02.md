# Agent-02 任务分配：Docker 部署

## 工作流引用
`.agentdocs/workflow/260114-项目完善.md`

## 任务列表
- T-06: 创建后端 Dockerfile
- T-07: 创建前端 Dockerfile
- T-08: 创建 docker-compose.yml
- T-09: 添加 nginx 配置

## 技术要求
- 多阶段构建优化镜像大小
- 前后端分离部署
- 后端使用 .NET 10.0 SDK/Runtime
- 前端使用 Node.js 构建 + nginx 运行

## 项目结构
- backend/ - .NET 后端
- frontend/ - React 前端

## 预期输出
- backend/Dockerfile
- frontend/Dockerfile
- docker-compose.yml (根目录)
- nginx/nginx.conf

## 完成后
将结果写入 `.agentdocs/runtime/260114-项目完善/results/agent-02-result.md`
