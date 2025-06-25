using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SafeDeleteLibrary;

/// <summary>
/// SafeDeleteLibrary 简单使用示例
/// 这个示例展示了如何在您的C#应用程序中使用SafeDeleteLibrary
/// </summary>
class UsageExample
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SafeDeleteLibrary 使用示例");
        Console.WriteLine("==========================");

        try
        {
            // 示例1: 删除单个文件
            await DeleteSingleFileExample();

            // 示例2: 删除目录
            await DeleteDirectoryExample();

            // 示例3: 错误处理
            ErrorHandlingExample();

            Console.WriteLine("\n所有示例完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"示例执行过程中发生错误: {ex.Message}");
        }
        finally
        {
            // 释放资源
            SafeDelete.Dispose();
            Console.WriteLine("资源已释放");
        }

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 示例1: 安全删除单个文件
    /// </summary>
    static async Task DeleteSingleFileExample()
    {
        Console.WriteLine("\n1. 单文件删除示例");
        Console.WriteLine("------------------");

        // 创建一个临时测试文件
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "这是一个包含敏感信息的测试文件。\n删除后应该无法恢复。");
        
        Console.WriteLine($"创建测试文件: {tempFile}");
        Console.WriteLine($"文件大小: {new FileInfo(tempFile).Length} 字节");

        try
        {
            // 同步删除
            SafeDelete.SecureDeleteFile(tempFile);
            Console.WriteLine("✓ 文件已安全删除（同步方式）");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"✗ 删除失败: {ex.Message}");
        }

        // 创建另一个测试文件用于异步删除
        var tempFile2 = Path.GetTempFileName();
        File.WriteAllText(tempFile2, "另一个测试文件，用于演示异步删除。");
        Console.WriteLine($"创建第二个测试文件: {tempFile2}");

        try
        {
            // 异步删除
            await SafeDelete.SecureDeleteFileAsync(tempFile2);
            Console.WriteLine("✓ 文件已安全删除（异步方式）");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"✗ 异步删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 示例2: 安全删除目录
    /// </summary>
    static async Task DeleteDirectoryExample()
    {
        Console.WriteLine("\n2. 目录删除示例");
        Console.WriteLine("----------------");

        // 创建测试目录结构
        var tempDir = Path.Combine(Path.GetTempPath(), "SafeDeleteTest_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(tempDir);

        // 创建一些测试文件
        File.WriteAllText(Path.Combine(tempDir, "document1.txt"), "文档1的内容");
        File.WriteAllText(Path.Combine(tempDir, "document2.pdf"), "文档2的内容");
        
        // 创建子目录
        var subDir = Path.Combine(tempDir, "SubFolder");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "subdoc.docx"), "子文档的内容");

        Console.WriteLine($"创建测试目录: {tempDir}");
        Console.WriteLine($"目录包含文件数: {Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories).Length}");

        try
        {
            // 异步删除整个目录
            await SafeDelete.SecureDeleteDirectoryAsync(tempDir);
            Console.WriteLine("✓ 目录已安全删除");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"✗ 目录删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 示例3: 错误处理
    /// </summary>
    static void ErrorHandlingExample()
    {
        Console.WriteLine("\n3. 错误处理示例");
        Console.WriteLine("----------------");

        // 测试删除不存在的文件
        try
        {
            SafeDelete.SecureDeleteFile("不存在的文件.txt");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"✓ 正确捕获文件不存在异常: {ex.GetType().Name}");
        }

        // 测试删除不存在的目录
        try
        {
            SafeDelete.SecureDeleteDirectory("不存在的目录");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"✓ 正确捕获目录不存在异常: {ex.GetType().Name}");
        }

        // 测试空路径
        try
        {
            SafeDelete.SecureDeleteFile("");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✓ 正确捕获参数异常: {ex.GetType().Name}");
        }

        Console.WriteLine("错误处理测试完成");
    }
}

/// <summary>
/// 高级使用示例类
/// </summary>
public static class AdvancedUsageExamples
{
    /// <summary>
    /// 批量删除文件
    /// </summary>
    public static async Task BatchDeleteFiles(string[] filePaths)
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
            Console.WriteLine($"成功批量删除 {tasks.Count} 个文件");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"批量删除过程中出现错误: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.FilePath))
            {
                Console.WriteLine($"问题文件: {ex.FilePath}");
            }
        }
    }

    /// <summary>
    /// 智能删除（自动判断文件或目录）
    /// </summary>
    public static async Task SmartDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                await SafeDelete.SecureDeleteFileAsync(path);
                Console.WriteLine($"文件已删除: {path}");
            }
            else if (Directory.Exists(path))
            {
                await SafeDelete.SecureDeleteDirectoryAsync(path);
                Console.WriteLine($"目录已删除: {path}");
            }
            else
            {
                Console.WriteLine($"路径不存在: {path}");
            }
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"删除失败: {ex.Message}");
            Console.WriteLine($"路径: {ex.FilePath}");
            Console.WriteLine($"操作: {ex.Operation}");
        }
    }

    /// <summary>
    /// 带确认的安全删除
    /// </summary>
    public static async Task DeleteWithConfirmation(string path)
    {
        Console.WriteLine($"确定要安全删除 '{path}' 吗？");
        Console.WriteLine("警告: 此操作不可恢复！");
        Console.Write("输入 'YES' 确认删除: ");
        
        var confirmation = Console.ReadLine();
        if (confirmation?.ToUpper() == "YES")
        {
            try
            {
                await SmartDelete(path);
                Console.WriteLine("删除完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除失败: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("删除操作已取消");
        }
    }
}
