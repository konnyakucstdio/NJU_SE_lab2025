# 南京大学2025软件工程课程实验

- 一个基于 C# WinForms 开发的个人记账本应用，提供收入支出记录、数据统计、预算管理等功能。

## 🚀 功能特性

- 📊 **收支记录** - 手动记录收入和支出 （扫描和关联待完成）
- 📈 **数据统计** - 收支分类统计和图表展示
- 💰 **预算管理** - 设置和管理月度预算
- 🏦 **多账户管理** - 支持支付宝、微信、银行卡等账户 （待完成）
- 💾 **数据持久化** - 使用 JSON 文件保存数据 （安全加密工程进行中）
- 🎯 **简洁界面** - 模仿现代移动应用的 UI 设计

## 📦 系统要求

### 开发环境
- **Visual Studio**: 2019 或更高版本
- **.NET Framework**: 4.7.2 或更高版本
- **操作系统**: Windows 10/11

### 运行时环境
- .NET Framework 4.7.2
- Windows 7 SP1 或更高版本

## 🔧 编译说明

### 必需的工具和库
1. **Visual Studio 2019/2022** - 社区版即可
2. **.NET Framework 4.7.2 Developer Pack**
3. **NuGet 包**:
   - System.Text.Json (自动包含在 .NET Framework 4.7.2+ 中)

### 编译步骤
1. 克隆仓库到本地
2. 使用 Visual Studio 打开 `MoneyManager.sln`
3. 等待 NuGet 包恢复完成
4. 选择 `Release` 配置
5. 点击 `生成` → `生成解决方案` 或按 `Ctrl+Shift+B`

## 📁 项目结构
MoneyManager/
├── Forms/ # 窗体文件
│ ├── MainForm.cs # 主界面
│ ├── AddTransactionForm.cs
│ ├── StatisticsForm.cs
│ └── AllTransactionsForm.cs
├── Models/ # 数据模型
│ └── Transaction.cs
├── Services/ # 业务逻辑
│ └── TransactionService.cs
├── Program.cs # 程序入口
└── MoneyManager.csproj # 项目文件

text

## 📥 下载和安装

### 方法一：使用预编译版本（推荐）
1. 克隆本仓库到本地
2. 进入 `bin/Release/` 目录
3. 运行 `MoneyManager.exe`

如果 `bin/Release/` 目录不存在，请使用方法二从源码编译。

### 方法二：从源码编译
```bash
# 克隆仓库
git clone https://github.com/yourusername/money-manager.git

# 使用 Visual Studio 打开并编译
# 或使用 MSBuild 命令行编译
msbuild MoneyManager.sln /p:Configuration=Release