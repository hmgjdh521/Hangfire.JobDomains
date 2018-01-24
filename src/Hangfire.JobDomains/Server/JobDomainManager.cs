﻿using Hangfire.JobDomains.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Storage;
using System.Threading.Tasks;
using Common.Logging;

namespace Hangfire.JobDomains.Server
{
    internal class JobDomainManager
    {

        public static string ServerName => Environment.MachineName.ToLower();

        static ILog loger = LogManager.GetLogger<JobDomainManager>();

        public static async Task<BackgroundJobServerOptions> InitServer(string path, int workerCount)
        {
            if (string.IsNullOrEmpty(path)) {
                var server=  StorageService.Provider.GetServer(ServerName);
                if (server != null) path = server.PlugPath;
            }
            else
            {
                await UpdateServer(path);
            }

            var queues = StorageService.Provider.GetQueues(null, ServerName);
            var options = new BackgroundJobServerOptions
            {
                WorkerCount = workerCount,
                Queues = queues.Select(s => s.Name).ToArray()
            };
            return options;
        }

        public static async Task Restart(string path)
        {
            await UpdateServer(path);
            //暂时只支持 web 站重启
            File.SetLastWriteTime($"{AppDomain.CurrentDomain.BaseDirectory}\\Web.config", System.DateTime.Now);
        }

        public static async Task UpdateServer(string path)
        {
            try
            {
                var plugins = await ScanServer(path);
                var server = new ServerDefine(ServerName) { PlugPath = path };
                var success = await StorageService.Provider.AddOrUpdateServerAsync(server, plugins);
                if (success) return;
            }
            catch (Exception ex)
            {
                loger.Error("服务初始化", ex);
            }
            throw (new Exception(" 服务更新失败! "));
        }

        static async Task<List<string>> ScanServer(string basePath)
        {
            if (Directory.Exists(basePath) == false) Directory.CreateDirectory(basePath);
            var plugins = new List<string>();
            var paths = Directory.GetDirectories(basePath);
            foreach (var path in paths)
            {
                var index = path.LastIndexOf("\\");
                var dir = path.Substring(index + 1);
                var define = new PluginDefine(dir);
                LoadDomain(basePath, define);

                var sets = define.GetJobSets();
                if (sets == null || sets.Count == 0) continue;

                await StorageService.Provider.AddPluginAsync(define);
                plugins.Add(define.Title);
            }
            return plugins;
        }

        static void LoadDomain(string basePath, PluginDefine define)
        {
            var files = Directory.GetFiles($"{basePath}\\{define.PathName}", "*.dll");
            var assemblies = new List<AssemblyDefine>();
            foreach (var assemblyFile in files)
            {
                try
                {
                    var assemblyItem = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                    var assemblyDefine = CreateAssemblyDefine(define, assemblyItem);
                    var jobs = ReadPrefabricationAssembly(assemblyDefine, assemblyItem);
                    if (jobs.Count == 0) continue;
                    assemblyDefine.SetJobs(jobs);
                    assemblies.Add(assemblyDefine);
                }
                catch (FileLoadException)
                {
                    //默认公用程序集不需要反射
                }
            }
            if (assemblies.Count == 0) return;
            define.SetJobSets(assemblies);
        }

        static AssemblyDefine CreateAssemblyDefine(PluginDefine domainDefine, Assembly define)
        {
            var file = define.Location;
            var fullName = define.FullName;
            var shortName = define.ManifestModule.Name.Replace(".dll", string.Empty);
            var title = define.ReadReflectionOnlyAssemblyAttribute<AssemblyTitleAttribute>();
            var description = define.ReadReflectionOnlyAssemblyAttribute<AssemblyDescriptionAttribute>();
            return new AssemblyDefine(domainDefine, file, fullName, shortName, title, description);
        }

        static List<JobDefine> ReadPrefabricationAssembly(AssemblyDefine assemblyDefine, Assembly assembly)
        {
            var list = new List<JobDefine>();
            var types = assembly.GetInterfaceTypes<IPrefabrication>();
            foreach (var type in types)
            {
                var (Title, Description) = type.ReadReflectionNameplateAttribute();
                var constructors = GetConstructors(type);
                var define = new JobDefine(assemblyDefine, type.FullName, type.Name, constructors, Title, Description);
                list.Add(define);
            }
            return list;
        }

        static IEnumerable<ConstructorDefine> GetConstructors(Type type)
        {
            var constructors = new List<ConstructorDefine>();
            var ctors = type.GetConstructors();
            foreach (var item in ctors)
            {
                var paramers = item.GetParameters();
                var one = new ConstructorDefine();
                foreach (var par in paramers)
                {
                    one.Paramers.Add((par.Name, par.ParameterType.Name));
                }
                constructors.Add(one);
            }
            return constructors;
        }

    }

     
  

}
