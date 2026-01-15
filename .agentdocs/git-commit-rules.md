# Git 提交规范

## 自动提交规则

完成某个功能后：
- **自动进行 git commit（不推送）**
- **只提交本次功能相关的文件**
- 对于其他未相关的文件，除非用户明确要求提交所有文件，否则不需要管
- 使用 `git add <具体文件路径>` 来只添加相关文件
- 避免使用 `git add .` 或 `git add -A`，除非用户明确要求

### 示例流程

```bash
# 只添加本次功能相关的文件
git add src/backend/SomeFeature.cs
git add src/frontend/SomeComponent.tsx
git add docs/plans/pXX-feature.md

# 提交
git commit -m "feat: 完成 PXX 某某功能"
```

## 禁止内容

在创建 git commit 时，提交信息中**不能包含**以下内容：

```
🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>
```

## 正确的提交格式

使用简洁的中文提交信息，格式如下：

```
<type>: <简短描述>

<可选的详细说明>
```

### 提交类型 (type)

| 类型 | 说明 |
|------|------|
| `feat` | 新功能 |
| `fix` | 修复 bug |
| `docs` | 文档更新 |
| `chore` | 杂项更新 |
| `refactor` | 代码重构 |
| `test` | 测试相关 |
| `style` | 代码格式调整 |

### 示例

简单提交：
```bash
git commit -m "feat: 完成 DDNS 前端页面"
```

带详细说明：
```bash
git commit -m "feat: 完成 DDNS 前端页面

- 添加 DDNS 类型定义和 API 服务
- 创建 DDNS 管理页面组件
- 更新路由和导航栏
- 添加中英文国际化支持"
```

## 提交检查清单

在提交前确认：
- [ ] 只添加了本次功能相关的文件
- [ ] 提交信息使用正确的类型前缀
- [ ] 提交信息简洁明了
- [ ] 没有包含禁止的自动生成内容
