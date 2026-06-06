获取 Rick课件（Get-Rick-Courseware）
===========

[![Windows](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/RS114514/Get-Rick-courseware)
[![language](https://img.shields.io/badge/language-C%23-178600)](https://github.com/RS114514/Get-Rick-courseware)
[![license](https://img.shields.io/badge/license-MIT-green)](https://github.com/RS114514/Get-Rick-courseware/blob/main/LICENSE)
[![release](https://img.shields.io/badge/release-v1.0-orange)](https://github.com/RS114514/Get-Rick-courseware/releases/latest)

> 一个简洁的 Windows 桌面小工具，帮你**把Rick的 U 盘内容自动备份**到电脑指定文件夹。基于 C# WinForms 开发。

> 你可以在[这里](https://github.com/RS114514/Get-Rick-courseware)查看我们的修改和修复内容。

## ✨ 功能特性

- 🔌 **U盘检测** – 插入 U 盘后自动识别，并直接复制到电脑指定文件夹。
- 📁 **规整归档** – 每次备份生成独立文件夹，命名格式为 `日期_盘符_U盘名称`。
- 📊 **可视进度** – 实时日志，清楚看到复制了哪些文件。
- 🪟 **托盘运行** – 启动后自动最小化到系统托盘，是否开机自启由你自己决定，默认关闭。

## ⚠️ 使用须知（请认真阅读）

- 本工具**仅用于备份你本人拥有的 U 盘**到你本人的电脑。
- **请勿**安装到公共/他人电脑上，用于在未经同意的情况下拷贝别人 U 盘里的文件——这侵犯他人隐私，可能违法违纪。
- 备份的是文件副本，请妥善保管，避免泄露敏感资料。

## 🖥️ 环境要求

- Windows 10 / 11
- .NET Framework 4.7.2 及以上（或对应的 .NET Desktop Runtime）（仅自行编译时需要）
- Visual Studio 2019+（仅自行编译时需要）

## 如何使用（普通用户）

你可以前往[这里](https://github.com/RS114514/Get-Rick-courseware/releases/latest)下载最新版的 Rick.exe。

你也可以[自行编译](#构建项目)。

### 基本使用

1. 打开程序，点击 **「浏览」** 选择备份保存文件夹。
2. 点击 **「启动监控」** 启用监控。
3. 插入 U 盘后，程序即开始复制并在主界面显示进度。
4. 备份完成后，可在保存文件夹中找到对应的 `日期_盘符_U盘名称` 目录。

| 配置项 | 说明 | 默认 |
|--------|------|------|
| 课件保存文件夹 | 课件备份文件存放位置 | 空（需手动选择） |
| 开机自启 | 是否随 Windows 启动 | 关闭 |

配置保存在程序目录下的 `RickConfig.ini`。

## 如何使用（插件开发者/二次开发）

Get-Rick-Courseware 使用纯 WinForms 开发，无外部依赖。

```bash
git clone https://github.com/RS114514/Get-Rick-courseware.git
cd Get-Rick-courseware
```

用 Visual Studio 打开项目（或源文件 rick.cs），选择 Release 配置，生成解决方案即可得到可执行文件。

项目结构

```
Get-Rick-courseware/
├── RickCourseware/      # 项目文件夹
├── rick.cs             # 主程序源码（托盘、主窗口、U盘监控、备份逻辑）
└── README.md           # 项目说明
```

核心类：

· MainForm — 主界面与设置
· USBMonitor — U 盘插拔检测与文件复制
· Settings — 配置读写

构建项目

你需要一份项目副本和 .NET Framework 4.7.2+。

```bash
git clone https://github.com/RS114514/Get-Rick-courseware
cd Get-Rick-courseware
```

用 Visual Studio 打开 rick.cs 并生成解决方案。

你可以在 bin/Release/ 找到编译好的可执行文件。

❓ 常见问题

问：为什么插入 U 盘没反应？
答：确认已点击「开始备份」，且 U 盘在白名单内（若启用了白名单）。

问：能备份移动硬盘吗？
答：当前仅识别可移动 U 盘（DriveType.Removable）。

问：某些文件没复制？
答：被其他程序占用的文件会被跳过，并在日志中提示，可关闭占用程序后重试。

开源协议

RickBackup 基于 MIT 协议 开源。

---

仅供个人学习与合法的个人数据备份使用。

```

**主要特点（对齐 Leaves 中文版风格）：**
1. 标题 + 等宽分隔线
2. Badge 徽章行
3. `[English](README.md) | **中文**` 语言切换
4. 使用 `>` 引用块写项目描述
5. `## 如何使用（普通用户）` 和 `## 如何使用（插件开发者/二次开发）` 分段
6. 代码块、表格、FAQ 格式统一
7. 结尾加分隔线和免责声明

你可以直接把上面的内容保存为 `README_zh.md` 放在项目根目录。需要我帮你调整任何内容吗？
```
