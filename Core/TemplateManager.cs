using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using SiteServer.Plugin;

namespace SS.Filter.Core
{
    public static class TemplateManager
    {
        private static string CacheGetFileContent(string filePath)
        {
            ObjectCache cache = MemoryCache.Default;

            if (cache[filePath] is string fileContents) return fileContents;

            var policy = new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(0, 1, 0, 0)
            };
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string> { filePath }));

            fileContents = Utils.ReadText(filePath);

            cache.Set(filePath, fileContents, policy);

            return fileContents;
        }

        public static string GetTemplatesDirectoryPath()
        {
            return Context.PluginApi.GetPluginPath(Utils.PluginId, "templates");
        }

        public static List<TemplateInfo> GetTemplateInfoList()
        {
            var templateInfoList = new List<TemplateInfo>();

            var directoryPath = GetTemplatesDirectoryPath();
            var directoryNames = Utils.GetDirectoryNames(directoryPath);
            foreach (var directoryName in directoryNames)
            {
                var templateInfo = GetTemplateInfo(directoryPath, directoryName);
                if (templateInfo != null)
                {
                    templateInfoList.Add(templateInfo);
                }
            }

            return templateInfoList;
        }

        public static TemplateInfo GetTemplateInfo(string name)
        {
            var directoryPath = GetTemplatesDirectoryPath();
            return GetTemplateInfo(directoryPath, name);
        }

        private static TemplateInfo GetTemplateInfo(string templatesDirectoryPath, string name)
        {
            TemplateInfo templateInfo = null;

            var configPath = Utils.PathCombine(templatesDirectoryPath, name, "config.json");
            if (Utils.IsFileExists(configPath))
            {
                templateInfo = Context.UtilsApi.JsonDeserialize<TemplateInfo>(Utils.ReadText(configPath));
                templateInfo.Name = name;
            }

            return templateInfo;
        }

        public static void Clone(string nameToClone, TemplateInfo templateInfo, string templateHtml = null)
        {
            var directoryPath = Context.PluginApi.GetPluginPath(Utils.PluginId, "templates");

            Utils.CopyDirectory(Utils.PathCombine(directoryPath, nameToClone), Utils.PathCombine(directoryPath, templateInfo.Name), true);

            var configJson = Context.UtilsApi.JsonSerialize(templateInfo);
            var configPath = Utils.PathCombine(directoryPath, templateInfo.Name, "config.json");
            Utils.WriteText(configPath, configJson);

            if (templateHtml != null)
            {
                SetTemplateHtml(templateInfo, templateHtml);
            }
        }

        public static void Edit(TemplateInfo templateInfo)
        {
            var directoryPath = Context.PluginApi.GetPluginPath(Utils.PluginId, "templates");

            var configJson = Context.UtilsApi.JsonSerialize(templateInfo);
            var configPath = Utils.PathCombine(directoryPath, templateInfo.Name, "config.json");
            Utils.WriteText(configPath, configJson);
        }

        public static string GetTemplateHtml(TemplateInfo templateInfo)
        {
            var directoryPath = GetTemplatesDirectoryPath();
            var htmlPath = Utils.PathCombine(directoryPath, templateInfo.Name, templateInfo.Main);
            return CacheGetFileContent(htmlPath);
        }

        public static void SetTemplateHtml(TemplateInfo templateInfo, string html)
        {
            var directoryPath = GetTemplatesDirectoryPath();
            var htmlPath = Utils.PathCombine(directoryPath, templateInfo.Name, templateInfo.Main);

            Utils.WriteText(htmlPath, html);
        }

        public static void DeleteTemplate(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            var directoryPath = GetTemplatesDirectoryPath();
            var templatePath = Utils.PathCombine(directoryPath, name);
            Utils.DeleteDirectoryIfExists(templatePath);
        }
    }
}
