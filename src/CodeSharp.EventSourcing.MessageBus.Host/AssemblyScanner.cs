using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing.MessageBus.Host
{
    /// <summary>
    /// 辅助类，用于扫描当前宿主目录下的所有程序集
    /// </summary>
    public class AssemblyScanner
    {
        /// <summary>
        /// 扫描当前宿主目录下的所有程序集
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static IEnumerable<Assembly> GetScannableAssemblies(string baseDirectory)
        {
            var root = new DirectoryInfo(baseDirectory);
            var assemblyFiles = root.GetFiles("*.dll", SearchOption.AllDirectories).Union(root.GetFiles("*.exe", SearchOption.AllDirectories));
            var results = new List<Assembly>();

            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    assembly.GetTypes(); //这里确保当前程序集的所有类型都能正常访问
                    results.Add(assembly);
                }
                catch (BadImageFormatException badImageException)
                {
                    string errorMessage = string.Format("无法加载程序集: {0}. 错误信息: {1}.", assemblyFile.FullName, badImageException);
                    throw new Exception(errorMessage, badImageException);
                }
                catch (ReflectionTypeLoadException reflectionException)
                {
                    string errorMessage = string.Format("无法加载程序集: {0}. 错误信息: {1}.", assemblyFile.FullName, reflectionException);
                    throw new Exception(errorMessage, reflectionException);
                }
            }
            return results;
        }
    }
}
