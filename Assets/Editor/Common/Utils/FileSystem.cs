using System;
using System.IO;

/// <summary>
/// 文件处理工具
/// </summary>
public class FileSystem
{
    /// <summary>
    /// 标准化路径中的路径分隔符（统一使用“/”符号）
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string StandardizeBackslashSeparator(string path)
    {
        path = path.Replace("\\", "/");
        return path;
    }

    /// <summary>
    /// 标准化路径中的路径分隔符（统一使用“\”符号）
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string StandardizeSlashSeparator(string path)
    {
        path = path.Replace("/", "\\");
        return path;
    }

    /// <summary>
    /// 删除目录下使用指定扩展名的文件
    /// </summary>
    /// <param name="dirPath">目录地址</param>
    /// <param name="ext">扩展名 格式可以为[exe]或[.exe]</param>
    public static void DeleteFilesByExt(string dirPath, string ext)
    {
        if (false == ext.StartsWith("."))
        {
            ext = "." + ext;
        }

        string[] dirs = Directory.GetDirectories(dirPath);
        foreach (string dir in dirs)
        {
            DeleteFilesByExt(dir, ext);
        }

        string[] files = Directory.GetFiles(dirPath);
        foreach (string file in files)
        {
            if (File.Exists(file))
            {
                if (Path.GetExtension(file) == ext)
                {
                    File.Delete(file);
                }
            }
        }
    }

    /// <summary>
    /// 将给的路径合并起来
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string CombinePaths(params string[] args)
    {
        if (args.Length == 0)
        {
            return "";
        }

        string path = args[0];
        for (int i = 1; i < args.Length; i++)
        {
            path = Path.Combine(path, args[i]);
        }

        //为了好看
        path = StandardizeBackslashSeparator(path);

        return path;
    }

    /// <summary>
    /// 将给的目录路径合并起来
    /// </summary>
    /// <param name="endWithBackslash">路径最后是否以反斜杠结束</param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string CombineDirs(bool isEndWithBackslash, params string[] args)
    {
        string path = CombinePaths(args);

        if (isEndWithBackslash)
        {
            if (false == path.EndsWith("/"))
            {
                path += "/";
            }
        }
        else
        {
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }
        }

        return path;
    }

    /// <summary>
    /// 如果路径开头有文件分隔符，则移除
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string RemoveStartPathSeparator(string path)
    {
        if (path.StartsWith("/"))
        {
            return path.Substring(1);
        }
        else if (path.StartsWith("\\"))
        {
            return path.Substring(2);
        }

        return path;
    }

    /// <summary>
    /// 如果路径结尾有文件分隔符，则移除
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string RemoveEndPathSeparator(string path)
    {
        if (path.EndsWith("/"))
        {
            return path.Substring(1);
        }
        else if (path.EndsWith("\\"))
        {
            return path.Substring(2);
        }

        return path;
    }

    public static void CopyDirectory(string srcPath, string destPath, Func<string, string> checker = null)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)     //判断是否文件夹
                {
                    if (!Directory.Exists(destPath + "\\" + i.Name))
                    {
                        Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                    }
                    CopyDirectory(i.FullName, destPath + "\\" + i.Name, checker);    //递归调用复制子文件夹
                }
                else
                {
                    string desName = i.Name;
                    if (checker != null) desName = checker(desName);
                    File.Copy(i.FullName, destPath + "\\" + desName, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                }
            }
        }
        catch (System.Exception e)
        {
            throw new System.Exception(e.Message);
        }
    }
}