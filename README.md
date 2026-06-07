# 获取 Rick 课件（Get-Rick-Courseware）

[![Windows](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/RS114514/Get-Rick-courseware)
[![macOS](https://img.shields.io/badge/platform-macOS-lightgrey)](https://github.com/RS114514/Get-Rick-courseware)
[![language](https://img.shields.io/badge/language-C%23-178600)](https://github.com/RS114514/Get-Rick-courseware)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![license](https://img.shields.io/badge/license-MIT-green)](https://github.com/RS114514/Get-Rick-courseware/blob/main/LICENSE)

一个简洁的跨平台 U 盘课件自动备份小工具。插入 U 盘后自动备份内容到指定文件夹。
- **Windows 版本**：基于 C# WinForms 开发。
- **macOS 版本**：基于 C# Avalonia UI 框架开发，具有一致的视觉界面。

---

## 功能特性

- **跨平台支持** — 支持 Windows 与 macOS 双系统，界面与核心监控逻辑完全一致。
- **U盘检测与过滤** — 自动识别 U 盘插入，在 macOS 下支持高精度过滤（排除外部移动硬盘和 DMG 镜像）。
- **监控防抖/零卡顿** — macOS 版使用内存级磁盘元数据缓存并结合异步后台轮询，CPU 占用接近 0%。
- **U盘屏蔽管理** — 支持一键屏蔽指定 U 盘，支持同名 U 盘通过序列号/UUID 精准唯一屏蔽。
- **规整归档** — 每次备份生成独立文件夹，命名格式 `日期_时间_盘符/卷标_U盘名称`。
- **Mac 专属清理** — 自动跳过 `.DS_Store`、`.Trashes` 等 macOS 系统冗余文件。
- **运行日志持久化** — 运行日志自动保存至 `logs/` 文件夹下，按天分文件。
- **托盘运行与开机自启** — 支持最小化到系统托盘，且支持双平台原生开机自启动。

---

## 使用须知

- 本工具**仅用于备份你本人拥有的 U 盘**到你本人的电脑。
- **请勿**在未经同意的情况下拷贝他人 U 盘里的文件。
- 备份文件请妥善保管，避免泄露敏感资料。

---

## 环境要求

### Windows 平台
- Windows 10 / 11（x64）
- 运行预编译免安装版（自包含）：无需任何依赖，双击直接启动。
- 自行编译：需要安装 .NET 10 SDK。

### macOS 平台
- macOS 10.15 及以上（Intel / Apple Silicon 芯片）
- 运行预编译免安装版（自包含）：解压后直接双击 `.app` 运行。
- 自行编译：需要安装 .NET 10 SDK。

---

## 使用方法

### 下载安装
前往 [Releases](https://github.com/RS114514/Get-Rick-courseware/releases) 页面：
* **Windows 用户**：下载 `Rick.exe`，自包含运行时，双击即可运行。
* **macOS 用户**：下载 `Rick-macOS.zip`，解压得到 `获取Rick课件.app`，即可双击运行。

### 基本操作
1. 打开程序，点击 **「浏览」** 选择课件备份保存的文件夹。
2. 点击 **「启动监控」** 按钮启用监控，状态显示为绿色 **「监控中 ✓」**。
3. 插入 U 盘后，程序自动静默复制并在主界面底部的进度条显示状态。
4. 复制完成后可在您指定的保存文件夹中找到归档目录。
5. 在下拉菜单中选择插入的 U 盘并点击 **「屏蔽此U盘」**，可使其不再被自动复制。

---

## 一键构建项目

我们在项目中提供了非常方便的一键构建脚本，供您本地直接编译双端程序。

### 💻 构建 Windows 版本
在 Windows 系统下，双击根目录下的 **`build-win.bat`** 即可自动开始编译。
- 编译生成的文件将输出在 `./publish-win/Rick.exe`，该文件已整合 .NET 运行时，可发给其他电脑免安装直接启动。

### 🍏 构建 macOS 版本
在 macOS 系统终端下，进入 `mac/` 文件夹并执行 **`build-app.sh`**：
```bash
cd mac
chmod +x build-app.sh
./build-app.sh
```
- 编译生成的成品将输出在 `./mac/获取Rick课件.app`，已经自带图标和 macOS 架构配置，可直接拖入 `/Applications` 中使用。

---

## 常见问题

**Q: 为什么编译报错缺少 MSBuild 或 WindowsTargeting？**
在 macOS/Linux 下，您可以在编译 Windows 项目时加上 `<EnableWindowsTargeting>true</EnableWindowsTargeting>`（已在 `.csproj` 中默认配置），或者在 Windows 下直接使用 Visual Studio 2022 开发。

**Q: macOS 下插入移动硬盘时会被复制吗？**
不会。macOS 版本的磁盘识别逻辑会读取 `diskutil info`，只有可移动属性（Removable Media）为 `Removable` 且非磁盘镜像的 USB 闪存盘才会被识别为 U 盘，普通移动硬盘不受影响。

**Q: 提示文件正在被占用？**
如果 U 盘中某些文件被其他进程以独占方式打开，本软件在备份时会自动跳过它并在运行日志中输出提示，以避免因锁定导致整个复制队列崩溃。

---

## 开源协议

基于 [MIT 协议](LICENSE) 开源。
