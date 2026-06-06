# 获取 Rick 课件（Get-Rick-Courseware）

[![Windows](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/RS114514/Get-Rick-courseware)
[![language](https://img.shields.io/badge/language-C%23-178600)](https://github.com/RS114514/Get-Rick-courseware)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![license](https://img.shields.io/badge/license-MIT-green)](https://github.com/RS114514/Get-Rick-courseware/blob/main/LICENSE)

一个简洁的 Windows 桌面小工具，插入 U 盘后自动备份内容到指定文件夹。基于 C# WinForms（.NET 10）开发。

## 功能特性

- **U盘检测** — 插入 U 盘后自动识别并开始复制
- **规整归档** — 每次备份生成独立文件夹，命名格式 `日期_时间_盘符_U盘名称`
- **实时日志** — 清楚看到复制了哪些文件
- **托盘运行** — 启动后最小化到系统托盘，可选开机自启（默认关闭）

## 使用须知

- 本工具**仅用于备份你本人拥有的 U 盘**到你本人的电脑
- **请勿**在未经同意的情况下拷贝他人 U 盘里的文件
- 备份文件请妥善保管，避免泄露敏感资料

## 环境要求

- Windows 10 / 11（x64）
- 运行预编译版：需要 [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- 自行编译：需要 .NET 10 SDK + Visual Studio 2022 或 `dotnet` CLI

## 使用方法

### 下载安装

前往 [Releases](https://github.com/RS114514/Get-Rick-courseware/releases/latest) 下载 `获取Rick课件.exe`，或[自行编译](#构建项目)。

### 基本操作

1. 打开程序，点击 **「浏览」** 选择备份保存文件夹
2. 点击 **「启动监控」** 启用监控
3. 插入 U 盘后，程序自动复制并在主界面显示进度
4. 完成后可在保存文件夹中找到对应的备份目录

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| 课件保存文件夹 | 备份文件存放位置 | 空（需手动选择） |
| 开机自启 | 是否随 Windows 启动 | 关闭 |

配置保存在程序目录下的 `RickConfig.ini` 文件中。

## 构建项目

### 方法一：dotnet CLI

```bash
git clone https://github.com/RS114514/Get-Rick-courseware.git
cd Get-Rick-courseware

# 还原依赖
dotnet restore

# 编译
dotnet build --configuration Release

# 发布单文件可执行程序
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

### 方法二：Visual Studio 2022

1. 用 Visual Studio 打开 `RickCourseware.csproj`
2. 顶部菜单选择 **Release** 配置
3. 点击 **生成 > 生成解决方案**（Ctrl+Shift+B）

编译产物位于 `bin/Release/net10.0-windows/` 目录。

## 常见问题

**Q: 缺少 .NET 10 SDK 怎么办？**
前往 [Microsoft .NET 下载页面](https://dotnet.microsoft.com/download/dotnet/10.0) 安装。

**Q: 提示 WinForms 依赖缺失？**
确保安装 Visual Studio 时勾选了「.NET 桌面开发」工作负载。

**Q: 插入 U 盘没反应？**
确认已点击「启动监控」，且 U 盘为可移动存储设备。

**Q: 某些文件没复制？**
被其他程序占用的文件会被跳过并在日志中提示，关闭占用程序后重试即可。

## 开源协议

基于 [MIT 协议](LICENSE) 开源。
