# 学习计划 LearningReminder

一个常驻 Windows 托盘的学习自律小工具：到点用系统通知催你确认学习进度，用日历、统计与成就帮你把计划坚持下去。

官网 <https://liiyunxi.github.io/learning-reminder/> · 下载 [GitHub Releases](https://github.com/liiyunxi/learning-reminder/releases/latest)

## 功能特性

- **两种检查方式**：定时询问（按间隔反复确认，直到完成）/ 里程碑（拆成小目标逐个勾选，可跨天累计）
- **重复规则**：每天 / 每周 / 每月（短月自动顺延）/ 自定义（多选星期，含「工作日」一键勾选）
- **分组与模板**：任务可打标签并按组筛选；内置 4 个常用模板，支持把当前配置另存为模板
- **提醒触达**：Windows 系统通知（带「已完成 / 还没完成 / 稍后再说 / 立即学习」按钮）→ 右下角确认卡片 → 托盘气泡，三级兜底
- **免打扰与节奏**：免打扰时段内不提醒；连续未完成自动放缓询问节奏；通知聚合避免轰炸
- **打卡与时长**：补打卡（对过去日期事后补记）、学习计时（卡片上开始/结束，累计当日时长）
- **统计与激励**：连续完成天数、近 7 天趋势、本月汇总、每日目标、10 项成就
- **托盘与快捷键**：三色状态图标、菜单内今日状态总览、`Ctrl+Alt+L` 打开主界面 / `Ctrl+Alt+K` 立即检查全部
- **数据与隐私**：全部数据仅存本机；每天自动备份（保留 10 份）、导出/导入、可选的 DPAPI 本地加密
- **常驻与自启**：关闭窗口仅隐藏到托盘；支持开机自启；单实例运行

## 下载安装

- 推荐：**自包含单文件版**（约 70 MB，免装运行时）——在[下载页](https://liiyunxi.github.io/learning-reminder/download.html)或 [Releases](https://github.com/liiyunxi/learning-reminder/releases/latest) 获取
- 轻量：**框架依赖版**（约 7.5 MB，需先安装 .NET 8 桌面运行时）
- 首次运行可能出现 SmartScreen 提示（未做代码签名），点「更多信息 → 仍要运行」即可；可在下载页核对 SHA256

## 使用速览

1. 双击运行后常驻托盘，关闭主窗口不会退出（托盘右键 →「退出」才结束进程）
2. 「新建任务」填写名称、学习链接（可选）、分组标签、重复规则与检查方式
3. 到点后直接在系统通知上应答；未处理的通知会以右下角确认卡片兜底
4. 「记录」页查看月历与检查流水、补打卡；「统计」页查看连续天数、趋势与成就
5. 数据目录：`%AppData%\LearningReminder`（数据文件 / 日志 / backups 备份）

完整说明见[使用文档](https://liiyunxi.github.io/learning-reminder/document.html)。

## 技术栈

- .NET 8 · WPF（`net8.0-windows10.0.18362.0`）+ WinForms NotifyIcon
- CommunityToolkit.WinUI.Notifications（Windows 系统通知，支持通知按钮）
- System.Security.Cryptography.ProtectedData（DPAPI 本地加密）
- 像素风 UI：内置 Fusion Pixel 像素字体（SIL OFL）与像素 logo / 托盘图标
- 官网为纯静态 HTML/CSS（`docs/` 目录，GitHub Pages 发布），字体自托管、无 CDN 依赖

## 从源码构建

```powershell
# 调试构建
dotnet build

# 自包含单文件发布（免运行时的分发包）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist\selfcontained

# 框架依赖发布（需 .NET 8 桌面运行时）
dotnet publish -c Release -r win-x64 --self-contained false -o dist\framework
```

## 项目结构

```
Models/          数据模型（任务 / 记录 / 设置 / 模板）
Services/        调度、确认、通知、托盘、统计、备份与加密等服务
ViewModels/      界面视图模型
Views/           主窗口 / 任务编辑 / 确认卡片 / 设置窗口
Resources/       全部文案（AppStrings）与常量（AppConstants）集中管理
Fonts/           内置像素字体（SIL OFL）
Assets/          像素 logo 资源
docs/            官网源码（GitHub Pages 发布目录）
ARCHITECTURE.md  三阶段功能架构图与演进记录
```

## 反馈与支持

- 有问题或功能建议：优先提 [Issues](https://github.com/liiyunxi/learning-reminder/issues)（便于检索与追踪）
- 也可以到 [Support us](https://liiyunxi.github.io/learning-reminder/support.html) 页面扫码加作者微信直接聊

## 许可与字体

- 项目源码公开，供学习与审计
- 内置的 Fusion Pixel 像素字体遵循 [SIL Open Font License 1.1](Fonts/OFL.txt)（作者 TakWolf）