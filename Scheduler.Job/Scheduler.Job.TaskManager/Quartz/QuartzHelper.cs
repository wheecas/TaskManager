using Infrustructure.Log;
using Infrustructure.Utilities;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using Scheduler.Job.Common.Config;
using Scheduler.Job.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;


namespace Scheduler.Job.TaskManager.Quartz
{
    /// <summary>
    /// 任务处理帮助类
    /// </summary>
    public class QuartzHelper
    {
        private static readonly object Obj = new object();

        private static readonly string Scheme = "tcp";

        private static readonly string Server = ConfigurationManager.AppSettings["QuartzServer"];

        private static readonly string Port = ConfigurationManager.AppSettings["QuartzPort"];

        private static readonly string ConnectionString= ConfigurationManager.ConnectionStrings["SqlConnect"].ConnectionString.ToString();
        /// <summary>
        /// 缓存任务所在程序集信息
        /// </summary>
        private static readonly Dictionary<string, Assembly> AssemblyDict = new Dictionary<string, Assembly>();

        private static IScheduler _scheduler = null;

        private static IList<TaskModel> _currentTaskList = null;



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IList<TaskModel> CurrentTaskList()
        {
            if (_currentTaskList == null)
            {
                var taskHelper = CommonContainer.Resolve<ITaskHelper>();
                _currentTaskList = taskHelper.GetAllTaskList();
            }

            return _currentTaskList;
        }

        /// <summary>
        /// 初始化任务调度对象
        /// </summary>
        public static void InitScheduler()
        {
            try
            {
                lock (Obj)
                {
                    if (_scheduler == null)
                    {
                        #region quartz 实例配置，参考资料 http://www.cnblogs.com/mushroom/p/4231642.html
                        NameValueCollection properties = new NameValueCollection();
                        //存储类型
                        properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
                        //表明前缀
                        properties["quartz.jobStore.tablePrefix"] = "QRTZ_";
                        // Quartz Scheduler唯一实例ID，auto：自动生成
                        properties["quartz.scheduler.instanceId"] = "AUTO";
                        // 集群
                        properties["quartz.jobStore.clustered"] = "true";
                        //驱动类型
                        properties["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz";
                        //数据源名称
                        properties["quartz.jobStore.dataSource"] = "myDS";
                        //连接字符串
                        properties["quartz.dataSource.myDS.connectionString"] = ConnectionString;

                        //sqlserver版本
                        properties["quartz.dataSource.myDS.provider"] = "SqlServer-20";
                        //远程配置
                        properties["quartz.scheduler.exporter.type"] = "Quartz.Simpl.RemotingSchedulerExporter, Quartz";
                        properties["quartz.scheduler.exporter.port"] = Port;
                        properties["quartz.scheduler.exporter.bindName"] = "QuartzScheduler";
                        properties["quartz.scheduler.exporter.channelType"] = Scheme;

                        #endregion
                        // 配置文件的方式，配置quartz实例
                        ISchedulerFactory schedulerFactory = new StdSchedulerFactory(properties);
                        _scheduler = schedulerFactory.GetScheduler();

                        LogHelper.WriteLog("任务调度初始化成功！");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("任务调度初始化失败！", ex);
            }
        }

        /// <summary>
        /// 启用任务调度
        /// 启动调度时会把任务表中状态为“执行中”的任务加入到任务调度队列中
        /// </summary>
        public static void StartScheduler()
        {
            try
            {
                if (!_scheduler.IsStarted)
                {
                    //添加全局监听
                    _scheduler.ListenerManager.AddTriggerListener(new CustomTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());
                    _scheduler.Start();
                    var taskHelper = CommonContainer.Resolve<ITaskHelper>();
                    //获取所有执行中的任务
                    List<TaskModel> listTask = taskHelper.GetAllTaskList()?.ToList<TaskModel>();
                    _currentTaskList = listTask;
                    if (listTask != null && listTask.Count > 0)
                    {
                        foreach (TaskModel taskUtil in listTask)
                        {
                            try
                            {
                                ScheduleJob(taskUtil,true);//防止修改cron表达式
                            }
                            catch (Exception e)
                            {
                                LogHelper.WriteLog($"任务“{taskUtil.TaskName}”启动失败！", e);
                            }
                        }
                    }
                    LogHelper.WriteLog("任务调度启动成功！");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("任务调度启动失败！", ex);
            }
        }

        /// <summary>
        /// 初始化 远程Quartz服务器中的，各个Scheduler实例。
        /// 提供给远程管理端的后台，用户获取Scheduler实例的信息。
        /// </summary>
        public static void InitRemoteScheduler()
        {
            try
            {
                NameValueCollection properties = new NameValueCollection
                {
                    ["quartz.scheduler.instanceName"] = "ExampleQuartzScheduler",
                    ["quartz.scheduler.proxy"] = "true",
                    ["quartz.scheduler.proxy.address"] = $"{Scheme}://{Server}:{Port}/QuartzScheduler"
                };

                ISchedulerFactory sf = new StdSchedulerFactory(properties);

                _scheduler = sf.GetScheduler();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("初始化远程任务管理器失败" + ex.StackTrace);
            }
        }


        /// <summary>
        /// 删除现有任务
        /// </summary>
        /// <param name="jobKey"></param>
        public static void DeleteJob(string jobKey)
        {
            try
            {
                JobKey jk = new JobKey(jobKey);
                if (_scheduler.CheckExists(jk))
                {
                    //任务已经存在则删除
                    _scheduler.DeleteJob(jk);
                    LogHelper.WriteLog($"任务“{jobKey}”已经删除");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 启用任务
        /// <param name="task">任务信息</param>
        /// <param name="isDeleteOldTask">是否删除原有任务</param>
        /// <returns>返回任务trigger</returns>
        /// </summary>
        public static void ScheduleJob(TaskModel task, bool isDeleteOldTask = false)
        {
            if (isDeleteOldTask)
            {
                //先删除现有已存在任务
                DeleteJob(task.Id.ToString());
            }
            //验证是否正确的Cron表达式
            if (ValidExpression(task.CronExpressionString))
            {
                IJobDetail job = new JobDetailImpl(task.Id, GetClassInfo(task.AssemblyName, task.ClassName));
                //添加任务执行参数
                job.JobDataMap.Add("TaskParam", task.TaskParam);

                CronTriggerImpl trigger = new CronTriggerImpl();
                trigger.CronExpressionString = task.CronExpressionString;
                trigger.Name = task.Id.ToString();
                trigger.Description = task.TaskName;
                JobKey jk  = new JobKey(task.Id.ToString());
                if (!_scheduler.CheckExists(jk))
                {
                    _scheduler.ScheduleJob(job, trigger);
                }
                if (task.Status == TaskStatus.STOP)
                {
                    _scheduler.PauseJob(jk);
                }
                else
                {
                    LogHelper.WriteLog($"任务“{task.TaskName}”启动成功,未来5次运行时间如下:");
                    List<DateTime> list = GetNextFireTime(task.CronExpressionString, 5);
                    foreach (var time in list)
                    {
                        LogHelper.WriteLog(time.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
            }
            else
            {
                throw new Exception(task.CronExpressionString + "不是正确的Cron表达式,无法启动该任务!");
            }
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="jobKey"></param>
        public static void PauseJob(string jobKey)
        {
            try
            {
                JobKey jk = new JobKey(jobKey);
                if (_scheduler.CheckExists(jk))
                {
                    //任务已经存在则暂停任务
                    _scheduler.PauseJob(jk);
                    LogHelper.WriteLog($"任务“{jobKey}”已经暂停");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary>
        /// <param name="jobKey">任务key</param>
        public static void ResumeJob(string jobKey)
        {
            try
            {
                JobKey jk = new JobKey(jobKey);
                if (_scheduler.CheckExists(jk))
                {
                    //任务已经存在则暂停任务
                    _scheduler.ResumeJob(jk);
                    LogHelper.WriteLog($"任务“{jobKey}”恢复运行");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("恢复任务失败！", ex);
            }
        }

        /// <summary>
        /// 获取类的属性、方法  
        /// <param name="assemblyName">程序集</param>  
        /// <param name="className">类名</param>  
        /// </summary>  
        private static Type GetClassInfo(string assemblyName, string className)
        {
            try
            {
                assemblyName = FileHelper.GetAbsolutePath(assemblyName + ".dll");
                Assembly assembly = null;
                if (!AssemblyDict.TryGetValue(assemblyName, out assembly))
                {
                    assembly = Assembly.LoadFrom(assemblyName);
                    AssemblyDict[assemblyName] = assembly;
                }
                Type type = assembly.GetType(className, true, true);
                return type;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        public static void StopSchedule()
        {
            try
            {
                //判断调度是否已经关闭
                if (!_scheduler.IsShutdown)
                {
                    //等待任务运行完成
                    _scheduler.Shutdown(true);
                    LogHelper.WriteLog("任务调度停止！");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("任务调度停止失败！", ex);
            }
        }

        /// <summary>
        /// 校验字符串是否为正确的Cron表达式
        /// </summary>
        /// <param name="cronExpression">带校验表达式</param>
        /// <returns></returns>
        public static bool ValidExpression(string cronExpression)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }

        /// <summary>
        /// 获取任务在未来周期内哪些时间会运行
        /// </summary>
        /// <param name="cronExpressionString">Cron表达式</param>
        /// <param name="numTimes">运行次数</param>
        /// <returns>运行时间段</returns>
        public static List<DateTime> GetNextFireTime(string cronExpressionString, int numTimes)
        {
            if (numTimes < 0)
            {
                throw new Exception("参数numTimes值大于等于0");
            }
            //时间表达式
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(cronExpressionString).Build();
            IList<DateTimeOffset> dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, numTimes);
            List<DateTime> list = new List<DateTime>();
            foreach (DateTimeOffset dtf in dates)
            {
                list.Add(TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local));
            }
            return list;
        }

        /// <summary>
        /// 获取当前执行的Task 对象
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TaskModel GetTaskDetail(IJobExecutionContext context)
        {
            TaskModel task = new TaskModel();

            if (context != null)
            {
                task.Id = context.Trigger.Key.Name;
                task.TaskName = context.Trigger.Description;
                task.RecentRunTime = DateTime.Now;
                task.TaskParam = context.JobDetail.JobDataMap.Get("TaskParam") != null ? context.JobDetail.JobDataMap.Get("TaskParam").ToString() : "";
            }
            return task;
        }
    }
}
