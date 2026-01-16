# 重新设计首页 - 展示已配置服务商的域名和记录

## 任务概述
将首页从"DNS 查询对比"功能改为展示用户已配置的 DNS 服务商、域名列表和 DNS 记录信息。

## 当前分析
- 当前首页使用 `useDnsCompare` 进行 DNS 查询对比（使用公共 DNS 服务器）
- 用户希望首页展示已配置的服务商（如 DNSPod）及其域名和记录
- 后端已有 `UserProviderConfigController` 支持通过配置 ID 获取域名和记录

## 解决方案设计
1. 首页左侧/顶部显示已配置的服务商列表
2. 选择服务商后，显示该服务商下的域名列表
3. 选择域名后，显示该域名的 DNS 记录表格

## 实现计划

### Phase 1: 后端接口（已完成）
- [x] T-01: 添加 `GET /api/v1/user/providers/{id}/domains` 接口
- [x] T-02: 添加 `GET /api/v1/user/providers/{id}/domains/{domain}/records` 接口

### Phase 2: 前端 API 和 Hooks（已完成）
- [x] T-03: 添加 `fetchDomainsByConfig` 和 `fetchRecordsByConfig` API 函数
- [x] T-04: 添加 `useDomainsByConfig` 和 `useRecordsByConfig` hooks

### Phase 3: 首页重构
- [ ] T-05: 重新设计首页布局和状态管理
- [ ] T-06: 实现服务商选择器组件
- [ ] T-07: 实现域名列表组件
- [ ] T-08: 实现 DNS 记录表格组件
- [ ] T-09: 添加移动端适配

## 备注
- 保持 cyber/terminal 风格设计
- 需要处理加载状态和空状态
