# SafeDeleteLibrary - 安全文件删除C# DLL库

SafeDeleteLibrary 是一个高性能的安全文件删除C# DLL库，它使用多种数据覆写方法来确保文件数据不可恢复。该库支持单个文件和整个目录的安全删除，并使用并发处理来提高大型目录的处理速度。

## 特性

- **多重覆写方案**：
  - DoD 5220.22-M 标准（美国国防部标准）
  - Gutmann 模式（简化版）
  - 随机数据覆写
- **安全保证**：
  - 每次覆写都超出文件原始大小
  - 使用加密级随机数据
  - 多次不同模式覆写
  - 强制同步到磁盘
  - 文件名和目录名多次随机覆盖
  - 对扩展名进行混淆
- **高性能**：
  - 多线程并行处理
  - 优化的缓冲区大小
  - 智能任务分配
- **易于集成**：
  - 标准.NET DLL库
  - 简单的API接口
  - 详细的异常处理
  - 支持异步操作

## 系统要求

- .NET Framework 4.6.1+ 或 .NET Core 2.0+ 或 .NET 5+
- Windows/Linux/macOS（跨平台支持）

## 安装

### 通过NuGet包管理器
```bash
Install-Package SafeDeleteLibrary
```

### 通过.NET CLI
```bash
dotnet add package SafeDeleteLibrary
```

### 手动编译
```bash
dotnet build SafeDeleteLibrary.csproj
```

## API 参考

### 命名空间
```csharp
using SafeDeleteLibrary;
```

### 主要类：SafeDelete

#### 方法概览

| 方法 | 描述 | 异步版本 |
|------|------|----------|
| `SecureDeleteFile(string)` | 安全删除单个文件 | `SecureDeleteFileAsync(string, CancellationToken)` |
| `SecureDeleteDirectory(string)` | 安全删除目录及其内容 | `SecureDeleteDirectoryAsync(string, CancellationToken)` |
| `Dispose()` | 释放资源 | - |

### 详细API文档

#### SecureDeleteFile

**同步版本**
```csharp
public static void SecureDeleteFile(string filePath)
```

**异步版本**
```csharp
public static async Task SecureDeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
```

**参数**
- `filePath` (string): 要删除的文件的完整路径
- `cancellationToken` (CancellationToken, 可选): 用于取消操作的令牌

**异常**
- `ArgumentException`: 当文件路径为null或空时抛出
- `FileNotFoundException`: 当文件不存在时抛出
- `SafeDeleteException`: 当删除操作失败时抛出

**示例**
```csharp
// 同步删除
try
{
    SafeDelete.SecureDeleteFile(@"C:\sensitive\document.txt");
    Console.WriteLine("文件已安全删除");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"删除失败: {ex.Message}");
    Console.WriteLine($"文件路径: {ex.FilePath}");
    Console.WriteLine($"操作: {ex.Operation}");
}

// 异步删除
try
{
    await SafeDelete.SecureDeleteFileAsync(@"C:\sensitive\document.txt");
    Console.WriteLine("文件已安全删除");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"删除失败: {ex.Message}");
}
```

#### SecureDeleteDirectory

**同步版本**
```csharp
public static void SecureDeleteDirectory(string directoryPath)
```

**异步版本**
```csharp
public static async Task SecureDeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
```

**参数**
- `directoryPath` (string): 要删除的目录的完整路径
- `cancellationToken` (CancellationToken, 可选): 用于取消操作的令牌

**异常**
- `ArgumentException`: 当目录路径为null或空时抛出
- `DirectoryNotFoundException`: 当目录不存在时抛出
- `SafeDeleteException`: 当删除操作失败时抛出

**示例**
```csharp
// 同步删除目录
try
{
    SafeDelete.SecureDeleteDirectory(@"C:\sensitive\folder");
    Console.WriteLine("目录已安全删除");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"删除失败: {ex.Message}");
}

// 异步删除目录
try
{
    await SafeDelete.SecureDeleteDirectoryAsync(@"C:\sensitive\folder");
    Console.WriteLine("目录已安全删除");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"删除失败: {ex.Message}");
}
```

#### Dispose

```csharp
public static void Dispose()
```

释放SafeDelete类使用的资源。建议在应用程序结束时调用此方法。

**示例**
```csharp
// 在应用程序结束时
SafeDelete.Dispose();
```

### 异常处理

#### SafeDeleteException

自定义异常类，提供详细的错误信息。

**属性**
- `FilePath` (string): 导致异常的文件或目录路径
- `Operation` (string): 发生异常时正在执行的操作
- `Message` (string): 错误消息
- `InnerException` (Exception): 内部异常

**示例**
```csharp
try
{
    SafeDelete.SecureDeleteFile("nonexistent.txt");
}
catch (SafeDeleteException ex)
{
    Console.WriteLine($"错误: {ex.Message}");
    Console.WriteLine($"文件: {ex.FilePath}");
    Console.WriteLine($"操作: {ex.Operation}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"内部异常: {ex.InnerException.Message}");
    }
}
```

## 工作原理

### 安全删除过程

1. **文件名安全覆盖**
   - 在删除前对文件名进行3次随机覆盖
   - 使用随机扩展名进行混淆
   - 使用加密级随机数据生成新文件名

2. **DoD 5220.22-M 标准覆写**
   - 第一遍：写入全0 (0x00)
   - 第二遍：写入全1 (0xFF)
   - 第三遍：写入随机数据

3. **Gutmann 模式覆写（简化版）**
   - 写入 01010101 模式 (0x55)
   - 写入 10101010 模式 (0xAA)
   - 写入 10010010 模式 (0x92)
   - 写入 01001001 模式 (0x49)
   - 写入全0 (0x00)
   - 写入全1 (0xFF)
   - 写入随机数据

4. **最终随机覆写**
   - 使用加密级随机数据进行最后一次覆写

5. **目录处理**
   - 递归处理所有子文件和子目录
   - 自底向上删除目录结构
   - 每个目录都进行名称覆盖

### 性能优化

- **并发处理**: 使用信号量控制最大并发数（默认5个线程）
- **缓冲区优化**: 8KB的写入缓冲区，4KB的额外覆写空间
- **智能任务分配**: 异步任务处理，提高大目录处理效率

## 使用示例

### 基本使用

```csharp
using System;
using System.Threading.Tasks;
using SafeDeleteLibrary;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // 删除单个文件
            SafeDelete.SecureDeleteFile(@"C:\temp\secret.txt");
            
            // 异步删除目录
            await SafeDelete.SecureDeleteDirectoryAsync(@"C:\temp\sensitive_data");
            
            Console.WriteLine("所有文件已安全删除");
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
    }
}
```

### 批量删除文件

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SafeDeleteLibrary;

class BatchDelete
{
    static async Task DeleteMultipleFiles(string[] filePaths)
    {
        var tasks = new List<Task>();

        foreach (var filePath in filePaths)
        {
            if (File.Exists(filePath))
            {
                tasks.Add(SafeDelete.SecureDeleteFileAsync(filePath));
            }
        }

        try
        {
            await Task.WhenAll(tasks);
            Console.WriteLine($"成功删除 {tasks.Count} 个文件");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"批量删除过程中出现错误: {ex.Message}");
        }
    }
}
```

### 带取消功能的删除

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using SafeDeleteLibrary;

class CancellableDelete
{
    static async Task DeleteWithCancellation()
    {
        using var cts = new CancellationTokenSource();

        // 5秒后取消操作
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await SafeDelete.SecureDeleteDirectoryAsync(@"C:\large_directory", cts.Token);
            Console.WriteLine("目录删除完成");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("删除操作已取消");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"删除失败: {ex.Message}");
        }
    }
}
```

## 注意事项

1. **不可恢复性**: 使用此库删除的文件和目录是不可恢复的，请谨慎使用
2. **权限要求**: 需要对目标文件/目录有读写和重命名权限
3. **处理时间**: 大文件和复杂目录结构的处理可能需要较长时间
4. **资源管理**: 建议在应用程序结束时调用 `SafeDelete.Dispose()` 释放资源
5. **异常处理**: 始终使用try-catch块处理可能的异常
6. **并发限制**: 库内部限制最大并发数为5，避免系统资源过度使用

## 安全建议

1. 确保对要删除的文件有完整备份（如果需要的话）
2. 不要在系统关键目录中使用
3. 删除前仔细确认文件路径
4. 确保文件没有其他程序正在使用
5. 在生产环境中使用前进行充分测试

## 许可证

MIT License

## 技术支持

如有问题或建议，请联系开发团队或提交Issue。
