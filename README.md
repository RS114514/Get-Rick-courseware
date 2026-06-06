获取 Rick课件（Get-Rick-Courseware）
  ===========

  [![Windows](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/RS114514/Get-Rick-courseware)
  [![language](https://img.shields.io/badge/language-C%23-178600)](https://github.com/RS114514/Get-Rick-courseware)
  [![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
  [![license](https://img.shields.io/badge/license-MIT-green)](https://github.com/RS114514/Get-Rick-courseware/blob/main
  /LICENSE)
  [![release](https://img.shields.io/badge/release-v1.0-orange)](https://github.com/RS114514/Get-Rick-courseware/release
  s/latest)

  > 一个简洁的 Windows 桌面小工具，帮你**把 Rick 的 U 盘内容自动备份**到电脑指定文件夹。基于 C# WinForms（.NET
  10）开发。

  > 你可以在[这里](https://github.com/RS114514/Get-Rick-courseware)查看我们的修改和修复内容。

  ## ✨ 功能特性

  - 🔌 **U盘检测** – 插入 U 盘后自动识别，并直接复制到电脑指定文件夹。
  - 📁 **规整归档** – 每次备份生成独立文件夹，命名格式为 `日期_盘符_U盘名称`。
  - 📊 **可视进度** – 实时日志，清楚看到复制了哪些文件。
  - 🪟 **托盘运行** – 启动后自动最小化到系统托盘，是否开机自启由你自己决定，默认关闭。

  ## ⚠️  使用须知（请认真阅读）

  - 本工具**仅用于备份你本人拥有的 U 盘**到你本人的电脑。
  - **请勿**安装到公共/他人电脑上，用于在未经同意的情况下拷贝别人 U 盘里的文件——这侵犯他人隐私，可能违法违纪。
  - 备份的是文件副本，请妥善保管，避免泄露敏感资料。

  ## 🖥️  环境要求

  - Windows 10 / 11（x64）
  - [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)（运行预编译版本时需要）
  - .NET 10 SDK + Visual Studio 2022 / `dotnet` CLI（仅自行编译时需要）

  > **注意**：本项目基于 .NET 10 开发，需要安装对应的 SDK 和运行时。.NET 10 是跨平台框架，但本项目使用
  WinForms，因此仅支持 Windows 系统。

  ## 如何使用（普通用户）

  你可以前往 [Releases](https://github.com/RS114514/Get-Rick-courseware/releases/latest) 下载最新版的
  `获取Rick课件.exe`。

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

  配置保存在程序目录下的 `RickConfi。

  ## 如何使用（插件开发者/二次开发）

  Get-Rick-Courseware 使用纯 WinForms 开发，无外部依赖。

  ```bash
  git clone https://github.com/RS114514/Get-Rick-courseware.git
  cd Get-Rick-courseware

  用 Visual Studio 打开项目（或源文件 rick.cs），选择 Release 配置，生成解决方案即可得到可执行文件。

  构建项目

  你需要安装 .NET 10 SDK (https://dotnet.microsoft.com/download/dotnet/10.0) 和 Visual Studio 2022（或更高版本）。

  方法一：Visual Studio 2022

  1. 用 Visual Studio 打开 RickCourseware.csproj 文件
  2. 在顶部菜单栏选择 Release 配置
  3. 点击 生成 > 生成解决方案（或按 Ctrl+Shift+B）
  4. 编译完成后，在 bin/Release/net10.0-windows/ 目录下找到 获取Rick课件.exe

  方法二：dotnet CLI 命令行

  # 还原 NuGet 包
  dotnet restore

  # 编译项目
  dotnet build --configuration Release

  # 发布单文件可执行程序
  dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

  编译产物位于 bin/Release/net10.0-windows/ 目录：
  - 获取Rick课件.exe — 主程序
  - 获取Rick课件.dll — 程序集（如需调试）
  
  常见问题

  Q: 缺少 .NET 10 SDK 怎么办？
  A: 前往 Microsoft .NET 下载页面 (https://dotnet.microsoft.com/download/dotnet/10.0) 安装最新的 .NET 10 SDK。

  Q: 提示 WinForms 依赖缺失？
  A: 确保安装 Visual Studio 时勾选了 ".NET 桌面开发" 工作负载。

  Q: 为什么插入 U 盘没反应？
  A: 确认已点击「启动监控」，且 U 盘为可移动存储设备（DriveType.Removable）。

  Q: 某些文件没复制？
  A: 被其他程序占用的文件会被跳过，并在日志中提示，可关闭占用程序后重试。

  配置说明

  程序使用混合配置机制：
  - Properties.Settings — 存储用户设置（保存路径、开机自启等）
  - INI 文件 — 部分配置可能存储在 RickConfig.ini（兼容旧版本）
  
  开源协议

  Get-Rick-Courseware 基于 MIT 协议 开源。