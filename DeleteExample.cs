using System;
using System.IO;
using System.Threading.Tasks;
using SafeDeleteLibrary;

class DeleteExample
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SafeDeleteLibrary 删除示例");
        Console.WriteLine("==========================");

        // 示例1：删除单个文件
        await Example1_DeleteSingleFile();

        // 示例2：删除目录
        await Example2_DeleteDirectory();

        // 示例3：批量删除
        await Example3_BatchDelete();

        // 示例4：交互式删除
        await Example4_InteractiveDelete();

        // 释放资源
        SafeDelete.Dispose();
        
        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 示例1：删除单个文件
    /// </summary>
    static async Task Example1_DeleteSingleFile()
    {
        Console.WriteLine("\n=== 示例1：删除单个文件 ===");

        // 创建测试文件
        string testFile = Path.Combine(Path.GetTempPath(), "test_file.txt");
        File.WriteAllText(testFile, "这是一个测试文件，包含敏感数据需要安全删除。");
        
        Console.WriteLine($"创建测试文件: {testFile}");

        try
        {
            // 调用删除方法
            Console.WriteLine("正在安全删除文件...");
            await SafeDelete.SecureDeleteFileAsync(testFile);
            
            Console.WriteLine("✅ 文件删除成功！");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"❌ 删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 示例2：删除目录
    /// </summary>
    static async Task Example2_DeleteDirectory()
    {
        Console.WriteLine("\n=== 示例2：删除目录 ===");

        // 创建测试目录
        string testDir = Path.Combine(Path.GetTempPath(), "TestDirectory_" + DateTime.Now.Ticks);
        Directory.CreateDirectory(testDir);
        
        // 创建一些文件
        File.WriteAllText(Path.Combine(testDir, "file1.txt"), "文件1内容");
        File.WriteAllText(Path.Combine(testDir, "file2.doc"), "文件2内容");
        
        // 创建子目录
        string subDir = Path.Combine(testDir, "SubDir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "subfile.pdf"), "子文件内容");

        Console.WriteLine($"创建测试目录: {testDir}");
        Console.WriteLine($"包含 {Directory.GetFiles(testDir, "*", SearchOption.AllDirectories).Length} 个文件");

        try
        {
            // 调用删除方法
            Console.WriteLine("正在安全删除目录...");
            await SafeDelete.SecureDeleteDirectoryAsync(testDir);
            
            Console.WriteLine("✅ 目录删除成功！");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"❌ 删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 示例3：批量删除多个文件
    /// </summary>
    static async Task Example3_BatchDelete()
    {
        Console.WriteLine("\n=== 示例3：批量删除 ===");

        // 创建多个测试文件
        string[] testFiles = new string[3];
        for (int i = 0; i < 3; i++)
        {
            testFiles[i] = Path.Combine(Path.GetTempPath(), $"batch_test_{i}.txt");
            File.WriteAllText(testFiles[i], $"批量测试文件 {i} 的内容");
        }

        Console.WriteLine($"创建了 {testFiles.Length} 个测试文件");

        // 批量删除
        int successCount = 0;
        foreach (string file in testFiles)
        {
            try
            {
                Console.WriteLine($"删除: {Path.GetFileName(file)}");
                await SafeDelete.SecureDeleteFileAsync(file);
                successCount++;
                Console.WriteLine("  ✅ 成功");
            }
            catch (SafeDeleteException ex)
            {
                Console.WriteLine($"  ❌ 失败: {ex.Message}");
            }
        }

        Console.WriteLine($"批量删除完成: {successCount}/{testFiles.Length} 成功");
    }

    /// <summary>
    /// 示例4：交互式删除
    /// </summary>
    static async Task Example4_InteractiveDelete()
    {
        Console.WriteLine("\n=== 示例4：交互式删除 ===");
        Console.WriteLine("输入要删除的文件或目录路径（输入 'quit' 退出）:");

        while (true)
        {
            Console.Write("\n请输入路径: ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "quit")
                break;

            await DeletePath(input);
        }
    }

    /// <summary>
    /// 智能删除路径（自动判断文件或目录）
    /// </summary>
    static async Task DeletePath(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"检测到文件: {path}");
                Console.Write("确认删除？(y/N): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    Console.WriteLine("正在安全删除文件...");
                    await SafeDelete.SecureDeleteFileAsync(path);
                    Console.WriteLine("✅ 文件删除成功！");
                }
                else
                {
                    Console.WriteLine("取消删除");
                }
            }
            else if (Directory.Exists(path))
            {
                Console.WriteLine($"检测到目录: {path}");
                int fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                Console.WriteLine($"目录包含 {fileCount} 个文件");
                Console.Write("确认删除整个目录？(y/N): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    Console.WriteLine("正在安全删除目录...");
                    await SafeDelete.SecureDeleteDirectoryAsync(path);
                    Console.WriteLine("✅ 目录删除成功！");
                }
                else
                {
                    Console.WriteLine("取消删除");
                }
            }
            else
            {
                Console.WriteLine("❌ 路径不存在");
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("❌ 权限不足，无法删除");
        }
        catch (SafeDeleteException ex)
        {
            Console.WriteLine($"❌ 安全删除失败: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.FilePath))
                Console.WriteLine($"   问题路径: {ex.FilePath}");
            if (!string.IsNullOrEmpty(ex.Operation))
                Console.WriteLine($"   失败操作: {ex.Operation}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 未知错误: {ex.Message}");
        }
    }
}

/// <summary>
/// 实用工具类
/// </summary>
public static class SafeDeleteUtils
{
    /// <summary>
    /// 简单的文件删除调用
    /// </summary>
    public static void DeleteFile(string filePath)
    {
        try
        {
            SafeDelete.SecureDeleteFile(filePath);
            Console.WriteLine($"✅ 已删除文件: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 删除文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 简单的目录删除调用
    /// </summary>
    public static void DeleteDirectory(string dirPath)
    {
        try
        {
            SafeDelete.SecureDeleteDirectory(dirPath);
            Console.WriteLine($"✅ 已删除目录: {dirPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 删除目录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 异步文件删除调用
    /// </summary>
    public static async Task DeleteFileAsync(string filePath)
    {
        try
        {
            await SafeDelete.SecureDeleteFileAsync(filePath);
            Console.WriteLine($"✅ 已删除文件: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 删除文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 异步目录删除调用
    /// </summary>
    public static async Task DeleteDirectoryAsync(string dirPath)
    {
        try
        {
            await SafeDelete.SecureDeleteDirectoryAsync(dirPath);
            Console.WriteLine($"✅ 已删除目录: {dirPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 删除目录失败: {ex.Message}");
        }
    }
}
