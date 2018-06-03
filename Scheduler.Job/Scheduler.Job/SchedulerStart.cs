using Scheduler.Job.Common.Config;
using Scheduler.Job.TaskManager.Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Job
{
    public class SchedulerStart
    {

        public void Start()
        {
            ////配置信息读取
            //ConfigInit.InitConfig();
            //初始化任务调度对象
            QuartzHelper.InitScheduler();
            // 启用任务调度
            // 启动调度时会把任务表中状态为“执行中”的任务加入到任务调度队列中
            QuartzHelper.StartScheduler();
        }

        public void Stop()
        {
            //停止任务调度
            QuartzHelper.StopSchedule();
            //系统终止
            System.Environment.Exit(0);
        }
    }
}
