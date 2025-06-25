using System;
using System.IO;
using System.Threading.Tasks;
using SafeDeleteLibrary;  // 引用我们的DLL

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("SafeDeleteLibrary 测试程序");
            Console.WriteLine("========================");

            try
            {
                // 示例1：创建并删除测试文件
                await TestFileDelete();

                // 示例2：创建并删除测试目录
                await TestDirectoryDelete();

                Console.WriteLine("\n✅ 所有测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 错误: {ex.Message}");
            }
            finally
            {
                // 释放资源
                SafeDelete.Dispose();
            }

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 测试文件删除功能
        /// </summary>
        static async Task TestFileDelete()
        {
            Console.WriteLine("\n📁 测试文件删除");
            Console.WriteLine("---------------");

            // 创建一个测试文件
            string testFile = Path.Combine(Path.GetTempPath(), "test_sensitive_file.txt");
            File.WriteAllText(testFile, "这是一个包含敏感信息的测试文件，需要安全删除。");
            
            Console.WriteLine($"创建测试文件: {testFile}");
            Console.WriteLine($"文件大小: {new FileInfo(testFile).Length} 字节");

            try
            {
                // 使用SafeDeleteLibrary安全删除文件
                Console.WriteLine("正在安全删除文件...");
                await SafeDelete.SecureDeleteFileAsync(testFile);
                
                Console.WriteLine("✅ 文件已安全删除！");
                
                // 验证文件是否真的被删除了
                if (!File.Exists(testFile))
                {
                    Console.WriteLine("✅ 验证：文件确实已被删除");
                }
            }
            catch (SafeDeleteException ex)
            {
                Console.WriteLine($"❌ 删除失败: {ex.Message}");
                Console.WriteLine($"文件路径: {ex.FilePath}");
                Console.WriteLine($"操作: {ex.Operation}");
            }
        }

        /// <summary>
        /// 测试目录删除功能
        /// </summary>
        static async Task TestDirectoryDelete()
        {
            Console.WriteLine("\n📂 测试目录删除");
            Console.WriteLine("---------------");

            // 创建测试目录结构
            string testDir = Path.Combine(Path.GetTempPath(), "SafeDeleteTest_" + DateTime.Now.Ticks);
            Directory.CreateDirectory(testDir);

            // 创建一些测试文件
            File.WriteAllText(Path.Combine(testDir, "document1.txt"), "敏感文档1的内容");
            File.WriteAllText(Path.Combine(testDir, "document2.pdf"), "敏感文档2的内容");
            
            // 创建子目录
            string subDir = Path.Combine(testDir, "SubFolder");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "subdoc.docx"), "子目录中的敏感文档");

            Console.WriteLine($"创建测试目录: {testDir}");
            Console.WriteLine($"包含文件数: {Directory.GetFiles(testDir, "*", SearchOption.AllDirectories).Length}");

            try
            {
                // 使用SafeDeleteLibrary安全删除整个目录
                Console.WriteLine("正在安全删除目录...");
                await SafeDelete.SecureDeleteDirectoryAsync(testDir);
                
                Console.WriteLine("✅ 目录已安全删除！");
                
                // 验证目录是否真的被删除了
                if (!Directory.Exists(testDir))
                {
                    Console.WriteLine("✅ 验证：目录确实已被删除");
                }
            }
            catch (SafeDeleteException ex)
            {
                Console.WriteLine($"❌ 删除失败: {ex.Message}");
                Console.WriteLine($"目录路径: {ex.FilePath}");
                Console.WriteLine($"操作: {ex.Operation}");
            }
        }
    }

    /// <summary>
    /// 实用工具类 - 展示更多使用方式
    /// </summary>
    public static class SafeDeleteHelper
    {
        /// <summary>
        /// 智能删除 - 自动判断是文件还是目录
        /// </summary>
        public static async Task SmartDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"检测到文件，正在安全删除: {path}");
                    await SafeDelete.SecureDeleteFileAsync(path);
                    Console.WriteLine("✅ 文件删除完成");
                }
                else if (Directory.Exists(path))
                {
                    Console.WriteLine($"检测到目录，正在安全删除: {path}");
                    await SafeDelete.SecureDeleteDirectoryAsync(path);
                    Console.WriteLine("✅ 目录删除完成");
                }
                else
                {
                    Console.WriteLine($"❌ 路径不存在: {path}");
                }
            }
            catch (SafeDeleteException ex)
            {
                Console.WriteLine($"❌ 安全删除失败: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.FilePath))
                    Console.WriteLine($"问题路径: {ex.FilePath}");
                if (!string.IsNullOrEmpty(ex.Operation))
                    Console.WriteLine($"失败操作: {ex.Operation}");
            }
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        public static async Task BatchDeleteFiles(params string[] filePaths)
        {
            Console.WriteLine($"开始批量删除 {filePaths.Length} 个文件...");
            
            int successCount = 0;
            int failCount = 0;

            foreach (string filePath in filePaths)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        await SafeDelete.SecureDeleteFileAsync(filePath);
                        Console.WriteLine($"✅ 已删除: {Path.GetFileName(filePath)}");
                        successCount++;
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ 文件不存在: {filePath}");
                    }
                }
                catch (SafeDeleteException ex)
                {
                    Console.WriteLine($"❌ 删除失败: {Path.GetFileName(filePath)} - {ex.Message}");
                    failCount++;
                }
            }

            Console.WriteLine($"\n批量删除完成: 成功 {successCount} 个，失败 {failCount} 个");
        }

        /// <summary>
        /// 带确认的删除
        /// </summary>
        public static async Task DeleteWithConfirmation(string path)
        {
            Console.WriteLine($"\n⚠️ 警告：即将安全删除 '{path}'");
            Console.WriteLine("此操作不可恢复！");
            Console.Write("确认删除请输入 'YES': ");
            
            string input = Console.ReadLine();
            if (input?.ToUpper() == "YES")
            {
                await SmartDelete(path);
            }
            else
            {
                Console.WriteLine("❌ 删除操作已取消");
            }
        }
    }
}
