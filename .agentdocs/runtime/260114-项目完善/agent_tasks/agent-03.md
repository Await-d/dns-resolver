# Agent-03 任务分配：CI/CD

## 工作流引用
`.agentdocs/workflow/260114-项目完善.md`

## 任务列表
- T-10: 创建 GitHub Actions 构建工作流
- T-11: 添加测试工作流
- T-12: 添加 Docker 镜像发布工作流

## 技术要求
- GitHub Actions
- 自动化测试 + 构建 + 发布
- 支持 .NET 10.0 和 Node.js

## 项目结构
- backend/ - .NET 后端
- frontend/ - React 前端

## 预期输出
- .github/workflows/build.yml - 构建工作流
- .github/workflows/test.yml - 测试工作流
- .github/workflows/docker-publish.yml - Docker 发布工作流

## 完成后
将结果写入 `.agentdocs/runtime/260114-项目完善/results/agent-03-result.md`
