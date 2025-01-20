package main

import (
	"crypto/rand"
	"encoding/hex"
	"flag"
	"fmt"
	"math/big"
	"os"
	"path/filepath"
	"sync"
	"sync/atomic"
	"golang.org/x/sys/windows/registry"
)

const (
	// 每次写入的块大小
	blockSize = 8192
	// 额外覆写的大小（字节）
	extraSize = 4096
	// 最大并发数
	maxWorkers = 5
	// 文件名覆盖次数
	nameOverwriteCount = 3
)

// 常见文件扩展名列表
var commonExtensions = []string{
	".txt", ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".ppt", ".pptx",
	".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".psd",
	".mp3", ".wav", ".wma", ".aac", ".ogg", ".flac", ".m4a", ".mid",
	".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v",
	".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".iso", ".cab",
	".exe", ".dll", ".sys", ".msi", ".bat", ".cmd", ".reg", ".ini",
	".html", ".htm", ".css", ".js", ".xml", ".json", ".yaml", ".sql",
	".php", ".py", ".rb", ".sh", ".c", ".cpp", ".h", ".hpp", ".java",
	".class", ".jar", ".war", ".ear", ".go", ".rs", ".swift", ".kt",
	".apk", ".ipa", ".app", ".deb", ".rpm", ".pkg", ".dmg",

}

// 生成随机扩展名
func generateRandomExtension() (string, error) {
	max := big.NewInt(int64(len(commonExtensions)))
	n, err := rand.Int(rand.Reader, max)
	if err != nil {
		return "", err
	}
	return commonExtensions[n.Int64()], nil
}

// 擦除模式
type ErasePattern struct {
	data     []byte
	repeated bool // 是否重复使用该模式
}

// 进度信息
type Progress struct {
	totalFiles     int32
	completedFiles int32
	currentFile    string
	mu            sync.Mutex
}

// 更新并显示进度
func (p *Progress) update(completed bool, filename string) {
	if completed {
		atomic.AddInt32(&p.completedFiles, 1)
	}
	p.mu.Lock()
	p.currentFile = filename
	percent := float64(atomic.LoadInt32(&p.completedFiles)) / float64(p.totalFiles) * 100
	// 清除当前行
	fmt.Printf("\r\033[K")
	// 显示进度条
	fmt.Printf("\r正在安全删除: %.1f%% (%d/%d) %s", percent, atomic.LoadInt32(&p.completedFiles), p.totalFiles, p.currentFile)
	p.mu.Unlock()
}

// 生成固定模式的数据
func generatePattern(pattern byte, size int) []byte {
	data := make([]byte, size)
	for i := range data {
		data[i] = pattern
	}
	return data
}

// 生成随机数据
func generateRandomData(size int) ([]byte, error) {
	data := make([]byte, size)
	_, err := rand.Read(data)
	return data, err
}

// DoD 5220.22-M 标准的擦除模式序列
var dodPatterns = []ErasePattern{
	{generatePattern(0x00, blockSize), true}, // 全0
	{generatePattern(0xFF, blockSize), true}, // 全1
	{nil, false},                            // 随机数据
}

// Gutmann 模式的简化版本
var gutmannPatterns = []ErasePattern{
	{generatePattern(0x55, blockSize), true}, // 01010101
	{generatePattern(0xAA, blockSize), true}, // 10101010
	{generatePattern(0x92, blockSize), true}, // 10010010
	{generatePattern(0x49, blockSize), true}, // 01001001
	{generatePattern(0x00, blockSize), true}, // 全0
	{generatePattern(0xFF, blockSize), true}, // 全1
	{nil, false},                            // 随机数据
}

// 文件处理任务
type deleteTask struct {
	path string
	err  error
}

// 生成随机文件名
func generateRandomName() (string, error) {
	bytes := make([]byte, 16)
	if _, err := rand.Read(bytes); err != nil {
		return "", err
	}
	return hex.EncodeToString(bytes), nil
}

// 安全覆盖文件名
func secureOverwriteName(path string) (string, error) {
	dir := filepath.Dir(path)
	currentPath := path

	// 多次重命名文件
	var err error
	for i := 0; i < nameOverwriteCount; i++ {
		// 生成随机文件名
		randomName, err := generateRandomName()
		if err != nil {
			return currentPath, fmt.Errorf("生成随机文件名失败: %v", err)
		}

		// 生成随机扩展名
		randomExt, err := generateRandomExtension()
		if err != nil {
			return currentPath, fmt.Errorf("生成随机扩展名失败: %v", err)
		}

		// 组合新路径
		newPath := filepath.Join(dir, randomName+randomExt)
		if err := os.Rename(currentPath, newPath); err != nil {
			return currentPath, fmt.Errorf("重命名失败: %v", err)
		}
		currentPath = newPath
	}
	return currentPath, err
}

// 安全擦除单个文件
func secureDeleteFile(path string, progress *Progress) error {
	if progress != nil {
		progress.update(false, filepath.Base(path))
	}

	// 获取文件信息
	fileInfo, err := os.Stat(path)
	if err != nil {
		return fmt.Errorf("获取文件信息失败: %v", err)
	}

	// 先覆盖文件名
	currentPath, err := secureOverwriteName(path)
	if err != nil {
		return err
	}

	// 打开文件
	file, err := os.OpenFile(currentPath, os.O_RDWR, 0666)
	if err != nil {
		return fmt.Errorf("打开文件失败: %v", err)
	}
	defer file.Close()

	fileSize := fileInfo.Size()
	writeSize := fileSize + extraSize

	// 执行所有擦除操作
	if err := applyPatterns(file, writeSize, dodPatterns); err != nil {
		return err
	}
	if err := applyPatterns(file, writeSize, gutmannPatterns); err != nil {
		return err
	}
	if err := randomOverwrite(file, writeSize); err != nil {
		return err
	}

	// 截断文件到原始大小
	if err := file.Truncate(fileSize); err != nil {
		return fmt.Errorf("截断文件失败: %v", err)
	}

	// 关闭文件
	file.Close()

	// 删除文件
	if err := os.Remove(currentPath); err != nil {
		return fmt.Errorf("删除文件失败: %v", err)
	}

	if progress != nil {
		progress.update(true, filepath.Base(currentPath))
	}

	return nil
}

// 应用擦除模式序列
func applyPatterns(file *os.File, writeSize int64, patterns []ErasePattern) error {
	for _, pattern := range patterns {
		if _, err := file.Seek(0, 0); err != nil {
			return fmt.Errorf("文件定位失败: %v", err)
		}

		remaining := writeSize
		for remaining > 0 {
			size := blockSize
			if remaining < int64(size) {
				size = int(remaining)
			}

			var data []byte
			var err error

			if pattern.repeated {
				// 使用固定模式
				if size == blockSize {
					data = pattern.data
				} else {
					data = pattern.data[:size]
				}
			} else {
				// 生成随机数据
				data, err = generateRandomData(size)
				if err != nil {
					return fmt.Errorf("生成随机数据失败: %v", err)
				}
			}

			if _, err := file.Write(data); err != nil {
				return fmt.Errorf("写入文件失败: %v", err)
			}

			remaining -= int64(size)
		}

		// 强制写入磁盘
		if err := file.Sync(); err != nil {
			return fmt.Errorf("同步到磁盘失败: %v", err)
		}
	}
	return nil
}

// 随机覆写
func randomOverwrite(file *os.File, writeSize int64) error {
	if _, err := file.Seek(0, 0); err != nil {
		return fmt.Errorf("文件定位失败: %v", err)
	}

	remaining := writeSize
	for remaining > 0 {
		size := blockSize
		if remaining < int64(size) {
			size = int(remaining)
		}

		data, err := generateRandomData(size)
		if err != nil {
			return fmt.Errorf("生成随机数据失败: %v", err)
		}

		if _, err := file.Write(data); err != nil {
			return fmt.Errorf("写入文件失败: %v", err)
		}

		remaining -= int64(size)
	}

	return file.Sync()
}

// 计算目录中的文件总数
func countFiles(path string) (int, error) {
	count := 0
	err := filepath.Walk(path, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if !info.IsDir() {
			count++
		}
		return nil
	})
	return count, err
}

// 递归删除目录
func secureDeleteDir(path string) error {
	// 计算总文件数
	var totalFiles int32
	err := filepath.Walk(path, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if !info.IsDir() {
			atomic.AddInt32(&totalFiles, 1)
		}
		return nil
	})
	if err != nil {
		return fmt.Errorf("计算文件数失败: %v", err)
	}

	if totalFiles == 0 {
		// 如果是空目录，直接覆盖目录名后删除
		newPath, err := secureOverwriteName(path)
		if err != nil {
			return err
		}
		return os.RemoveAll(newPath)
	}

	progress := &Progress{
		totalFiles: totalFiles,
	}

	// 创建任务通道
	tasks := make(chan deleteTask, totalFiles)
	var wg sync.WaitGroup

	// 启动工作协程
	for i := 0; i < maxWorkers; i++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			for task := range tasks {
				if err := secureDeleteFile(task.path, progress); err != nil {
					task.err = err
				}
			}
		}()
	}

	// 收集所有文件路径
	var firstErr error
	err = filepath.Walk(path, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if !info.IsDir() {
			task := deleteTask{path: path}
			tasks <- task
			if task.err != nil && firstErr == nil {
				firstErr = task.err
			}
		}
		return nil
	})

	// 关闭任务通道
	close(tasks)

	// 等待所有工作协程完成
	wg.Wait()

	if err != nil {
		return fmt.Errorf("遍历目录失败: %v", err)
	}

	if firstErr != nil {
		return firstErr
	}

	// 自底向上覆盖目录名并删除空目录
	var dirs []string
	err = filepath.Walk(path, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if info.IsDir() {
			dirs = append([]string{path}, dirs...) // 将目录添加到切片开头
		}
		return nil
	})

	if err != nil {
		return fmt.Errorf("收集目录失败: %v", err)
	}

	// 处理每个目录
	for _, dir := range dirs {
		newPath, err := secureOverwriteName(dir)
		if err != nil {
			// 如果重命名失败，继续尝试删除
			fmt.Printf("警告: 目录名覆盖失败: %v\n", err)
			newPath = dir
		}
		if err := os.Remove(newPath); err != nil {
			return fmt.Errorf("删除目录失败: %v", err)
		}
	}

	// 清除进度显示并换行
	fmt.Println()
	return nil
}

func registerContextMenu() error {
	// 获取当前可执行文件的完整路径
	exePath, err := os.Executable()
	if err != nil {
		return fmt.Errorf("获取程序路径失败: %v", err)
	}
	exePath = `"` + exePath + `" "%1"`

	// 创建 *\shell\SafaDel 键
	k, _, err := registry.CreateKey(registry.CLASSES_ROOT, `*\shell\SafaDel`, registry.ALL_ACCESS)
	if err != nil {
		return fmt.Errorf("创建菜单键失败: %v", err)
	}
	defer k.Close()

	// 设置显示名称
	if err := k.SetStringValue("", "安全擦除"); err != nil {
		return fmt.Errorf("设置菜单名称失败: %v", err)
	}

	// 设置图标（使用程序自身作为图标）
	if err := k.SetStringValue("Icon", filepath.Clean(exePath)); err != nil {
		return fmt.Errorf("设置图标失败: %v", err)
	}

	// 创建命令键
	cmd, _, err := registry.CreateKey(registry.CLASSES_ROOT, `*\shell\SafaDel\command`, registry.ALL_ACCESS)
	if err != nil {
		return fmt.Errorf("创建命令键失败: %v", err)
	}
	defer cmd.Close()

	// 设置命令
	if err := cmd.SetStringValue("", exePath); err != nil {
		return fmt.Errorf("设置命令失败: %v", err)
	}

	// 为目录也添加右键菜单
	dirKey, _, err := registry.CreateKey(registry.CLASSES_ROOT, `Directory\shell\SafaDel`, registry.ALL_ACCESS)
	if err != nil {
		return fmt.Errorf("创建目录菜单键失败: %v", err)
	}
	defer dirKey.Close()

	if err := dirKey.SetStringValue("", "安全擦除"); err != nil {
		return fmt.Errorf("设置目录菜单名称失败: %v", err)
	}

	if err := dirKey.SetStringValue("Icon", filepath.Clean(exePath)); err != nil {
		return fmt.Errorf("设置目录图标失败: %v", err)
	}

	dirCmd, _, err := registry.CreateKey(registry.CLASSES_ROOT, `Directory\shell\SafaDel\command`, registry.ALL_ACCESS)
	if err != nil {
		return fmt.Errorf("创建目录命令键失败: %v", err)
	}
	defer dirCmd.Close()

	if err := dirCmd.SetStringValue("", exePath); err != nil {
		return fmt.Errorf("设置目录命令失败: %v", err)
	}

	return nil
}

func unregisterContextMenu() error {
	// 删除文件的右键菜单
	err := registry.DeleteKey(registry.CLASSES_ROOT, `*\shell\SafaDel\command`)
	if err != nil && err != registry.ErrNotExist {
		return fmt.Errorf("删除文件命令键失败: %v", err)
	}
	err = registry.DeleteKey(registry.CLASSES_ROOT, `*\shell\SafaDel`)
	if err != nil && err != registry.ErrNotExist {
		return fmt.Errorf("删除文件菜单键失败: %v", err)
	}

	// 删除目录的右键菜单
	err = registry.DeleteKey(registry.CLASSES_ROOT, `Directory\shell\SafaDel\command`)
	if err != nil && err != registry.ErrNotExist {
		return fmt.Errorf("删除目录命令键失败: %v", err)
	}
	err = registry.DeleteKey(registry.CLASSES_ROOT, `Directory\shell\SafaDel`)
	if err != nil && err != registry.ErrNotExist {
		return fmt.Errorf("删除目录菜单键失败: %v", err)
	}

	return nil
}

func main() {
	// 检查是否为注册命令
	if len(os.Args) > 1 {
		switch os.Args[1] {
		case "register":
			if err := registerContextMenu(); err != nil {
				fmt.Printf("注册右键菜单失败: %v\n", err)
				os.Exit(1)
			}
			fmt.Println("成功注册到右键菜单！")
			os.Exit(0)
		case "unregister":
			if err := unregisterContextMenu(); err != nil {
				fmt.Printf("取消注册右键菜单失败: %v\n", err)
				os.Exit(1)
			}
			fmt.Println("成功取消注册右键菜单！")
			os.Exit(0)
		}
	}

	flag.Parse()
	args := flag.Args()

	if len(args) < 1 {
		fmt.Println("使用方法: safadel <文件或目录路径>")
		os.Exit(1)
	}

	path := args[0]

	// 获取文件/目录信息
	fileInfo, err := os.Stat(path)
	if err != nil {
		fmt.Printf("错误: %v\n", err)
		os.Exit(1)
	}

	var deleteErr error
	if fileInfo.IsDir() {
		deleteErr = secureDeleteDir(path)
	} else {
		progress := &Progress{
			totalFiles: 1,
		}
		deleteErr = secureDeleteFile(path, progress)
		fmt.Println() // 换行
	}

	if deleteErr != nil {
		fmt.Printf("删除失败: %v\n", deleteErr)
		os.Exit(1)
	}

	fmt.Println("删除完成")
} 