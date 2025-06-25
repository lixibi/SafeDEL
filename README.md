# SafeDeleteLibrary 🔒

[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](https://github.com/dotnet/core/blob/main/os-lifecycle-policy.md)

[English](#english) | [中文](#中文)

---

## 中文

### 📖 项目简介

SafeDeleteLibrary 是一个高性能的C#安全文件删除库，使用多种数据覆写方法确保文件数据不可恢复。该库从Go语言项目成功转换而来，保持了原有的核心安全删除功能，并提供了标准的.NET API接口。

### ✨ 核心功能

- 🛡️ **多重安全覆写**：DoD 5220.22-M标准 + Gutmann模式 + 随机数据覆写
- 🔄 **文件名混淆**：3次随机重命名，使用随机扩展名
- ⚡ **高性能并发**：最大5个工作线程并行处理
- 🔀 **异步支持**：完整的async/await支持，避免UI阻塞
- 🌐 **跨平台兼容**：基于.NET Standard 2.0，支持Windows/Linux/macOS
- 🎯 **精确异常处理**：详细的错误信息和上下文
- 📦 **易于集成**：标准DLL库，支持NuGet包管理

### 📁 项目结构

```
SafeDeleteLibrary/
├── 🔧 核心库文件
│   ├── SafeDeleteLibrary.cs      # 主要API类
│   ├── SafeDeleteLibrary.csproj  # 项目文件
│   ├── ErasePattern.cs           # 擦除模式结构
│   ├── SafeDeleteException.cs    # 自定义异常类
│   └── Constants.cs              # 常量定义
│
├── 📚 文档和示例
│   ├── API_Documentation.md      # 完整API文档
│   ├── PROJECT_SUMMARY.md        # 项目总结
│   ├── UsageExample.cs           # 使用示例
│   ├── DeleteExample.cs          # 删除示例
│   └── TestApp.cs               # 测试应用
│
├── 📦 编译输出
│   └── bin/Debug/netstandard2.0/
│       ├── SafeDeleteLibrary.dll          # 编译后的DLL
│       └── SafeDeleteLibrary.1.0.0.nupkg  # NuGet包
│
└── 📄 配置文件
    ├── README.md                 # 本文件
    ├── .gitignore               # Git忽略文件
    └── SafeDeleteLibrary.sln    # 解决方案文件
```

### 💻 系统要求

- **.NET Framework**: 4.6.1 或更高版本
- **.NET Core**: 2.0 或更高版本
- **.NET**: 5.0 或更高版本
- **操作系统**: Windows, Linux, macOS
- **权限**: 对目标文件/目录的读写和重命名权限

### 🚀 快速开始

#### 1. 安装

**方法一：直接引用DLL**
```xml
<ItemGroup>
  <Reference Include="SafeDeleteLibrary">
    <HintPath>libs\SafeDeleteLibrary.dll</HintPath>
  </Reference>
</ItemGroup>
```

**方法二：通过NuGet包**
```bash
# 安装本地包
dotnet add package SafeDeleteLibrary --source ./
```

**方法三：项目引用**
```xml
<ItemGroup>
  <ProjectReference Include="path\to\SafeDeleteLibrary.csproj" />
</ItemGroup>
```

#### 2. 基本使用

```csharp
using SafeDeleteLibrary;

try
{
    // 删除单个文件
    SafeDelete.SecureDeleteFile(@"C:\sensitive\document.txt");
    
    // 异步删除目录
    await SafeDelete.SecureDeleteDirectoryAsync(@"C:\sensitive\folder");
    
    Console.WriteLine("删除完成！");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"删除失败: {ex.Message}");
}
finally
{
    // 释放资源
    SafeDelete.Dispose();
}
```

### 📋 API概览

| 方法 | 描述 | 返回类型 | 异步版本 |
|------|------|----------|----------|
| `SecureDeleteFile(string)` | 同步删除单个文件 | `void` | ✅ |
| `SecureDeleteFileAsync(string, CancellationToken)` | 异步删除单个文件 | `Task` | - |
| `SecureDeleteDirectory(string)` | 同步删除目录及内容 | `void` | ✅ |
| `SecureDeleteDirectoryAsync(string, CancellationToken)` | 异步删除目录及内容 | `Task` | - |
| `Dispose()` | 释放系统资源 | `void` | ❌ |

### 💡 使用示例

#### 基本文件删除
```csharp
using SafeDeleteLibrary;

// 同步删除
try
{
    SafeDelete.SecureDeleteFile(@"C:\temp\secret.txt");
    Console.WriteLine("✅ 文件已安全删除");
}
catch (FileNotFoundException)
{
    Console.WriteLine("❌ 文件不存在");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"❌ 删除失败: {ex.Message}");
}
```

#### 异步目录删除
```csharp
using System.Threading;

// 异步删除，支持取消
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5)); // 5分钟超时

try
{
    await SafeDelete.SecureDeleteDirectoryAsync(@"C:\temp\sensitive", cts.Token);
    Console.WriteLine("✅ 目录已安全删除");
}
catch (OperationCanceledException)
{
    Console.WriteLine("⏰ 删除操作已取消");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"❌ 删除失败: {ex.Message}");
    Console.WriteLine($"📁 问题路径: {ex.FilePath}");
    Console.WriteLine($"🔧 失败操作: {ex.Operation}");
}
```

#### 批量删除
```csharp
string[] sensitiveFiles = {
    @"C:\temp\document1.pdf",
    @"C:\temp\document2.docx",
    @"C:\temp\spreadsheet.xlsx"
};

foreach (string file in sensitiveFiles)
{
    try
    {
        if (File.Exists(file))
        {
            await SafeDelete.SecureDeleteFileAsync(file);
            Console.WriteLine($"✅ 已删除: {Path.GetFileName(file)}");
        }
    }
    catch (SafeDeleteException ex)
    {
        Console.WriteLine($"❌ 删除失败 {Path.GetFileName(file)}: {ex.Message}");
    }
}
```

#### Windows Forms集成
```csharp
private async void btnSecureDelete_Click(object sender, EventArgs e)
{
    string filePath = txtFilePath.Text;
    
    if (string.IsNullOrEmpty(filePath))
    {
        MessageBox.Show("请选择要删除的文件", "提示");
        return;
    }
    
    // 确认对话框
    var result = MessageBox.Show(
        $"确定要安全删除文件吗？\n\n{filePath}\n\n⚠️ 此操作不可恢复！", 
        "安全删除确认", 
        MessageBoxButtons.YesNo, 
        MessageBoxIcon.Warning);
    
    if (result != DialogResult.Yes) return;
    
    try
    {
        // 更新UI状态
        btnSecureDelete.Enabled = false;
        lblStatus.Text = "正在安全删除...";
        progressBar.Style = ProgressBarStyle.Marquee;
        
        // 执行删除
        await SafeDelete.SecureDeleteFileAsync(filePath);
        
        // 删除成功
        MessageBox.Show("文件已安全删除！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        txtFilePath.Clear();
    }
    catch (SafeDeleteException ex)
    {
        MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        // 恢复UI状态
        btnSecureDelete.Enabled = true;
        lblStatus.Text = "就绪";
        progressBar.Style = ProgressBarStyle.Blocks;
    }
}
```

### ⚠️ 安全警告

> **🚨 重要警告**: 使用SafeDeleteLibrary删除的文件和目录是**完全不可恢复**的！

- ❗ **删除前备份**: 确保重要文件已有备份
- ❗ **谨慎操作**: 仔细确认文件路径，避免误删
- ❗ **权限检查**: 确保有足够的文件访问权限
- ❗ **避免系统文件**: 不要在系统关键目录中使用
- ❗ **测试环境**: 在生产环境使用前充分测试

### 🔧 编译构建

```bash
# 克隆项目
git clone <repository-url>
cd SafeDeleteLibrary

# 恢复依赖
dotnet restore

# 编译库
dotnet build SafeDeleteLibrary.csproj

# 运行测试
dotnet run --project TestApp.csproj

# 创建Release版本
dotnet build -c Release

# 打包NuGet
dotnet pack SafeDeleteLibrary.csproj
```

### ⚡ 性能特点

- **处理速度**: 安全删除比普通删除慢，这是确保数据不可恢复的必要代价
- **内存占用**: 8KB缓冲区，内存使用效率高
- **并发控制**: 自动限制最大5个并发操作，避免系统过载
- **异步优化**: 大文件和目录建议使用异步方法
- **磁盘I/O**: 多次覆写会产生大量磁盘写入操作

### 🔬 技术细节

#### 安全删除流程
1. **📝 文件名覆盖**: 进行3次随机重命名，使用随机扩展名
2. **🛡️ DoD 5220.22-M覆写**: 0x00 → 0xFF → 随机数据
3. **🔄 Gutmann模式覆写**: 多种固定模式 + 随机数据
4. **🎲 最终随机覆写**: 使用加密级随机数据
5. **💾 强制同步**: 每次覆写后强制写入磁盘
6. **🗑️ 物理删除**: 最终删除文件系统条目

#### 覆写模式详情
- **DoD标准**: 全0 → 全1 → 随机数据
- **Gutmann模式**: 0x55, 0xAA, 0x92, 0x49, 0x00, 0xFF + 随机数据
- **额外覆写**: 超出原文件大小4KB的额外覆写空间

### 📚 相关文档

- 📖 [完整API文档](API_Documentation.md) - 详细的方法说明和参数
- 📋 [项目总结](PROJECT_SUMMARY.md) - 项目转换过程和技术细节
- 💻 [使用示例](UsageExample.cs) - 更多实用代码示例
- 🧪 [测试代码](DeleteExample.cs) - 功能测试和验证代码

### 📄 许可证

本项目采用 [MIT License](LICENSE) 开源许可证。

### 🤝 贡献

欢迎提交Issue和Pull Request来改进这个项目。

### 📞 支持

如有问题或建议，请：
- 提交 [GitHub Issue](../../issues)
- 联系开发团队
- 查阅项目文档

### 🔗 原始项目

本C# DLL库基于以下Go语言项目重写：
- **原始项目**: [https://github.com/u-wlkjyy/safedel](https://github.com/u-wlkjyy/safedel)
- **项目类型**: Go语言安全文件删除工具
- **转换说明**: 保持了原有的核心安全删除功能，转换为标准的.NET DLL库

---

## English

### 📖 Project Overview

SafeDeleteLibrary is a high-performance C# secure file deletion library that uses multiple data overwrite methods to ensure file data cannot be recovered. This library was successfully converted from a Go language project, maintaining the original core secure deletion functionality while providing standard .NET API interfaces.

### ✨ Core Features

- 🛡️ **Multiple Secure Overwrites**: DoD 5220.22-M standard + Gutmann patterns + random data overwrite
- 🔄 **Filename Obfuscation**: 3 rounds of random renaming with random extensions
- ⚡ **High-Performance Concurrency**: Up to 5 worker threads for parallel processing
- 🔀 **Async Support**: Full async/await support to prevent UI blocking
- 🌐 **Cross-Platform**: Based on .NET Standard 2.0, supports Windows/Linux/macOS
- 🎯 **Precise Exception Handling**: Detailed error information and context
- 📦 **Easy Integration**: Standard DLL library with NuGet package support

### 📁 Project Structure

```
SafeDeleteLibrary/
├── 🔧 Core Library Files
│   ├── SafeDeleteLibrary.cs      # Main API class
│   ├── SafeDeleteLibrary.csproj  # Project file
│   ├── ErasePattern.cs           # Erase pattern structure
│   ├── SafeDeleteException.cs    # Custom exception class
│   └── Constants.cs              # Constants definition
│
├── 📚 Documentation & Examples
│   ├── API_Documentation.md      # Complete API documentation
│   ├── PROJECT_SUMMARY.md        # Project summary
│   ├── UsageExample.cs           # Usage examples
│   ├── DeleteExample.cs          # Deletion examples
│   └── TestApp.cs               # Test application
│
├── 📦 Build Output
│   └── bin/Debug/netstandard2.0/
│       ├── SafeDeleteLibrary.dll          # Compiled DLL
│       └── SafeDeleteLibrary.1.0.0.nupkg  # NuGet package
│
└── 📄 Configuration Files
    ├── README.md                 # This file
    ├── .gitignore               # Git ignore file
    └── SafeDeleteLibrary.sln    # Solution file
```

### 💻 System Requirements

- **.NET Framework**: 4.6.1 or higher
- **.NET Core**: 2.0 or higher
- **.NET**: 5.0 or higher
- **Operating System**: Windows, Linux, macOS
- **Permissions**: Read/write and rename permissions for target files/directories

### 🚀 Quick Start

#### 1. Installation

**Method 1: Direct DLL Reference**
```xml
<ItemGroup>
  <Reference Include="SafeDeleteLibrary">
    <HintPath>libs\SafeDeleteLibrary.dll</HintPath>
  </Reference>
</ItemGroup>
```

**Method 2: NuGet Package**
```bash
# Install local package
dotnet add package SafeDeleteLibrary --source ./
```

**Method 3: Project Reference**
```xml
<ItemGroup>
  <ProjectReference Include="path\to\SafeDeleteLibrary.csproj" />
</ItemGroup>
```

#### 2. Basic Usage

```csharp
using SafeDeleteLibrary;

try
{
    // Delete a single file
    SafeDelete.SecureDeleteFile(@"C:\sensitive\document.txt");

    // Asynchronously delete directory
    await SafeDelete.SecureDeleteDirectoryAsync(@"C:\sensitive\folder");

    Console.WriteLine("Deletion completed!");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"Deletion failed: {ex.Message}");
}
finally
{
    // Release resources
    SafeDelete.Dispose();
}
```

### 📋 API Overview

| Method | Description | Return Type | Async Version |
|--------|-------------|-------------|---------------|
| `SecureDeleteFile(string)` | Synchronously delete a single file | `void` | ✅ |
| `SecureDeleteFileAsync(string, CancellationToken)` | Asynchronously delete a single file | `Task` | - |
| `SecureDeleteDirectory(string)` | Synchronously delete directory and contents | `void` | ✅ |
| `SecureDeleteDirectoryAsync(string, CancellationToken)` | Asynchronously delete directory and contents | `Task` | - |
| `Dispose()` | Release system resources | `void` | ❌ |

### 💡 Usage Examples

#### Basic File Deletion
```csharp
using SafeDeleteLibrary;

// Synchronous deletion
try
{
    SafeDelete.SecureDeleteFile(@"C:\temp\secret.txt");
    Console.WriteLine("✅ File securely deleted");
}
catch (FileNotFoundException)
{
    Console.WriteLine("❌ File not found");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"❌ Deletion failed: {ex.Message}");
}
```

#### Async Directory Deletion
```csharp
using System.Threading;

// Async deletion with cancellation support
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5)); // 5-minute timeout

try
{
    await SafeDelete.SecureDeleteDirectoryAsync(@"C:\temp\sensitive", cts.Token);
    Console.WriteLine("✅ Directory securely deleted");
}
catch (OperationCanceledException)
{
    Console.WriteLine("⏰ Deletion operation cancelled");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"❌ Deletion failed: {ex.Message}");
    Console.WriteLine($"📁 Problem path: {ex.FilePath}");
    Console.WriteLine($"🔧 Failed operation: {ex.Operation}");
}
```

#### Batch Deletion
```csharp
string[] sensitiveFiles = {
    @"C:\temp\document1.pdf",
    @"C:\temp\document2.docx",
    @"C:\temp\spreadsheet.xlsx"
};

foreach (string file in sensitiveFiles)
{
    try
    {
        if (File.Exists(file))
        {
            await SafeDelete.SecureDeleteFileAsync(file);
            Console.WriteLine($"✅ Deleted: {Path.GetFileName(file)}");
        }
    }
    catch (SafeDeleteException ex)
    {
        Console.WriteLine($"❌ Failed to delete {Path.GetFileName(file)}: {ex.Message}");
    }
}
```

#### Windows Forms Integration
```csharp
private async void btnSecureDelete_Click(object sender, EventArgs e)
{
    string filePath = txtFilePath.Text;

    if (string.IsNullOrEmpty(filePath))
    {
        MessageBox.Show("Please select a file to delete", "Notice");
        return;
    }

    // Confirmation dialog
    var result = MessageBox.Show(
        $"Are you sure you want to securely delete this file?\n\n{filePath}\n\n⚠️ This operation cannot be undone!",
        "Secure Delete Confirmation",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning);

    if (result != DialogResult.Yes) return;

    try
    {
        // Update UI state
        btnSecureDelete.Enabled = false;
        lblStatus.Text = "Securely deleting...";
        progressBar.Style = ProgressBarStyle.Marquee;

        // Perform deletion
        await SafeDelete.SecureDeleteFileAsync(filePath);

        // Success
        MessageBox.Show("File securely deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        txtFilePath.Clear();
    }
    catch (SafeDeleteException ex)
    {
        MessageBox.Show($"Deletion failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        // Restore UI state
        btnSecureDelete.Enabled = true;
        lblStatus.Text = "Ready";
        progressBar.Style = ProgressBarStyle.Blocks;
    }
}
```

### ⚠️ Security Warnings

> **🚨 CRITICAL WARNING**: Files and directories deleted with SafeDeleteLibrary are **completely unrecoverable**!

- ❗ **Backup First**: Ensure important files are backed up
- ❗ **Careful Operation**: Double-check file paths to avoid accidental deletion
- ❗ **Permission Check**: Ensure sufficient file access permissions
- ❗ **Avoid System Files**: Do not use in critical system directories
- ❗ **Test Environment**: Thoroughly test before production use

### 🔧 Build & Compilation

```bash
# Clone the project
git clone <repository-url>
cd SafeDeleteLibrary

# Restore dependencies
dotnet restore

# Build the library
dotnet build SafeDeleteLibrary.csproj

# Run tests
dotnet run --project TestApp.csproj

# Create Release build
dotnet build -c Release

# Package NuGet
dotnet pack SafeDeleteLibrary.csproj
```

### ⚡ Performance Characteristics

- **Processing Speed**: Secure deletion is slower than normal deletion - this is necessary to ensure data unrecoverability
- **Memory Usage**: 8KB buffer, efficient memory utilization
- **Concurrency Control**: Automatically limits to maximum 5 concurrent operations to prevent system overload
- **Async Optimization**: Recommended to use async methods for large files and directories
- **Disk I/O**: Multiple overwrites generate significant disk write operations

### 🔬 Technical Details

#### Secure Deletion Process
1. **📝 Filename Overwrite**: 3 rounds of random renaming with random extensions
2. **🛡️ DoD 5220.22-M Overwrite**: 0x00 → 0xFF → random data
3. **🔄 Gutmann Pattern Overwrite**: Multiple fixed patterns + random data
4. **🎲 Final Random Overwrite**: Using cryptographic-grade random data
5. **💾 Force Sync**: Force write to disk after each overwrite
6. **🗑️ Physical Deletion**: Final deletion of filesystem entry

#### Overwrite Pattern Details
- **DoD Standard**: All zeros → All ones → Random data
- **Gutmann Patterns**: 0x55, 0xAA, 0x92, 0x49, 0x00, 0xFF + random data
- **Extra Overwrite**: Additional 4KB overwrite space beyond original file size

### 📚 Related Documentation

- 📖 [Complete API Documentation](API_Documentation.md) - Detailed method descriptions and parameters
- 📋 [Project Summary](PROJECT_SUMMARY.md) - Project conversion process and technical details
- 💻 [Usage Examples](UsageExample.cs) - More practical code examples
- 🧪 [Test Code](DeleteExample.cs) - Functional testing and validation code

### 📄 License

This project is licensed under the [MIT License](LICENSE).

### 🤝 Contributing

Issues and Pull Requests are welcome to improve this project.

### 📞 Support

For questions or suggestions, please:
- Submit a [GitHub Issue](../../issues)
- Contact the development team
- Refer to project documentation

### 🔗 Original Project

This C# DLL library is rewritten based on the following Go language project:
- **Original Project**: [https://github.com/u-wlkjyy/safedel](https://github.com/u-wlkjyy/safedel)
- **Project Type**: Go language secure file deletion tool
- **Conversion Notes**: Maintains the original core secure deletion functionality, converted to a standard .NET DLL library

---

**Note**: This project has been successfully converted from Go language to a C# DLL library. The original Go code has been cleaned up.

---

*SafeDeleteLibrary - Secure file deletion made simple and reliable* 🔒✨
