using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(ConfigFile ="log4net.config",Watch = true)]
namespace Infrustructure.Log
{
    /// <summary>
    /// 使用LOG4NET记录日志的功能，在WEB.CONFIG里要配置相应的节点
    /// </summary>
    public class LogHelper
    {
        //log4net日志专用
        private static readonly ILog Loginfo = LogManager.GetLogger("loginfo");
        private static readonly ILog Logerror = LogManager.GetLogger("logerror");

        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }
        /// <summary>
        /// 普通的文件记录日志
        /// </summary>
        /// <param name="info"></param>
        public static void WriteLog(string info)
        {
            if (Loginfo.IsInfoEnabled)
            {
                Loginfo.Info(info);
            }
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void WriteLog(string info, Exception se)
        {
            if (Logerror.IsErrorEnabled)
            {
                Logerror.Error(info, se);
            }
        }

        /// <summary>
        /// 职责名称
        /// </summary>
        private string _repositoryName;

        /// <summary>
        /// 日志级别
        /// </summary>
        private string _level;

        /// <summary>
        /// 初始化 
        /// </summary>
        /// <param name="repositoryName">职责名称</param>
        /// <param name="level">职责名称</param>
        /// <remarks>解决任务和日志对应问题</remarks>
        public LogHelper(string repositoryName, string level)
        {
            this._repositoryName = repositoryName;
            this._level = level;
        }
    }
}
