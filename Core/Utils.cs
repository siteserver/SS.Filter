using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace SS.Filter.Core
{
    public static class Utils
    {
        public const string PluginId = "SS.Filter";

        private const char UrlSeparatorChar = '/';
        private const char PathSeparatorChar = '\\';

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        private static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
            {
                return new Dictionary<string, T>();
            }

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
                AddPropertyToDictionary(property, source, dictionary);
            return dictionary;
        }

        private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
        {
            var value = property.GetValue(source);
            if (IsOfType<T>(value))
                dictionary.Add(property.Name, (T)value);
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        public static string PathCombine(params string[] paths)
        {
            var retVal = string.Empty;
            if (paths != null && paths.Length > 0)
            {
                retVal = paths[0]?.Replace(UrlSeparatorChar, PathSeparatorChar).TrimEnd(PathSeparatorChar) ?? string.Empty;
                for (var i = 1; i < paths.Length; i++)
                {
                    var path = paths[i] != null ? paths[i].Replace(UrlSeparatorChar, PathSeparatorChar).Trim(PathSeparatorChar) : string.Empty;
                    retVal = Path.Combine(retVal, path);
                }
            }
            return retVal;
        }

        public static IEnumerable<string> GetDirectoryNames(string directoryPath)
        {
            var directories = Directory.GetDirectories(directoryPath);
            var retVal = new string[directories.Length];
            var i = 0;
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                retVal[i++] = directoryInfo.Name;
            }
            return retVal;
        }

        public static void DeleteDirectoryIfExists(string directoryPath)
        {
            try
            {
                if (IsDirectoryExists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static bool IsFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static string ReadText(string filePath)
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public static void WriteText(string filePath, string content)
        {
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        public static void CopyDirectory(string sourcePath, string targetPath, bool isOverride)
        {
            if (!Directory.Exists(sourcePath)) return;

            CreateDirectoryIfNotExists(targetPath);
            var directoryInfo = new DirectoryInfo(sourcePath);
            foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                var destPath = Path.Combine(targetPath, fileSystemInfo.Name);
                if (fileSystemInfo is FileInfo)
                {
                    CopyFile(fileSystemInfo.FullName, destPath, isOverride);
                }
                else if (fileSystemInfo is DirectoryInfo)
                {
                    CopyDirectory(fileSystemInfo.FullName, destPath, isOverride);
                }
            }
        }

        private static void CopyFile(string sourceFilePath, string destFilePath, bool isOverride)
        {
            try
            {
                CreateDirectoryIfNotExists(destFilePath);

                File.Copy(sourceFilePath, destFilePath, isOverride);
            }
            catch
            {
                // ignored
            }
        }

        public static void CreateDirectoryIfNotExists(string path)
        {
            var directoryPath = GetDirectoryPath(path);

            if (IsDirectoryExists(directoryPath)) return;

            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch
            {
                //Scripting.FileSystemObject fso = new Scripting.FileSystemObjectClass();
                //string[] directoryNames = directoryPath.Split('\\');
                //string thePath = directoryNames[0];
                //for (int i = 1; i < directoryNames.Length; i++)
                //{
                //    thePath = thePath + "\\" + directoryNames[i];
                //    if (StringUtils.Contains(thePath.ToLower(), ConfigUtils.Instance.PhysicalApplicationPath.ToLower()) && !IsDirectoryExists(thePath))
                //    {
                //        fso.CreateFolder(thePath);
                //    }
                //}                    
            }
        }

        private static bool IsDirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        private static string GetDirectoryPath(string path)
        {
            var ext = Path.GetExtension(path);
            var directoryPath = !string.IsNullOrEmpty(ext) ? Path.GetDirectoryName(path) : path;
            return directoryPath;
        }

        public static bool EqualsIgnoreCase(string a, string b)
        {
            if (a == b) return true;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            return string.Equals(a.Trim().ToLower(), b.Trim().ToLower());
        }

        public static DateTime ToDateTime(string dateTimeStr)
        {
            return ToDateTime(dateTimeStr, DateTime.Now);
        }

        private static DateTime ToDateTime(string dateTimeStr, DateTime defaultValue)
        {
            var datetime = defaultValue;
            if (!string.IsNullOrEmpty(dateTimeStr))
            {
                if (!DateTime.TryParse(dateTimeStr.Trim(), out datetime))
                {
                    datetime = defaultValue;
                }
                return datetime;
            }
            if (datetime <= DateTime.MinValue)
            {
                datetime = DateTime.Now;
            }
            return datetime;
        }

        public static bool ToBool(string boolStr)
        {
            bool boolean;
            if (!bool.TryParse(boolStr?.Trim(), out boolean))
            {
                boolean = false;
            }
            return boolean;
        }

        public static int ToInt(string intStr)
        {
            int i;
            if (!int.TryParse(intStr?.Trim(), out i))
            {
                i = 0;
            }
            return i;
        }

        public static string GetMessageHtml(string message, bool isSuccess)
        {
            return isSuccess
                ? $@"<div class=""alert alert-success"" role=""alert"">{message}</div>"
                : $@"<div class=""alert alert-danger"" role=""alert"">{message}</div>";
        }

        public static string ReplaceNewline(string inputString, string replacement)
        {
            if (string.IsNullOrEmpty(inputString)) return string.Empty;
            var retVal = new StringBuilder();
            inputString = inputString.Trim();
            foreach (var t in inputString)
            {
                switch (t)
                {
                    case '\n':
                        retVal.Append(replacement);
                        break;
                    case '\r':
                        break;
                    default:
                        retVal.Append(t);
                        break;
                }
            }
            return retVal.ToString();
        }

        public static List<int> StringCollectionToIntList(string collection)
        {
            var list = new List<int>();
            if (!string.IsNullOrEmpty(collection))
            {
                var array = collection.Split(',');
                foreach (var s in array)
                {
                    int i;
                    int.TryParse(s.Trim(), out i);
                    list.Add(i);
                }
            }
            return list;
        }

        public static string GetShortGuid(bool isUppercase)
        {
            var i = Guid.NewGuid().ToByteArray().Aggregate<byte, long>(1, (current, b) => current * (b + 1));
            var retVal = $"{i - DateTime.Now.Ticks:x}";
            return isUppercase ? retVal.ToUpper() : retVal.ToLower();
        }
    }
}
