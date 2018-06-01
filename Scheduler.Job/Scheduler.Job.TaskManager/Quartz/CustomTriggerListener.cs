using Infrustructure.Utilities;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler.Job.TaskManager.Quartz
{
    /// <summary>
    /// 自定义触发器监听
    /// </summary>
    public class CustomTriggerListener : ITriggerListener
    {
        public string Name => "All_TriggerListener";

        /// <summary>
        /// Job执行时调用
        /// </summary>
        /// <param name="trigger">触发器</param>
        /// <param name="context">上下文</param>
        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {

        }

        /// <summary>
        ///  //Trigger触发后，job执行时调用本方法。true即否决，job后面不执行。
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            var taskHelper = CommonContainer.Resolve<ITaskHelper>();
            taskHelper.UpdateRecentRunTime(trigger.JobKey.Name, TimeZoneInfo.ConvertTimeFromUtc(context.FireTimeUtc.Value.DateTime, TimeZoneInfo.Local));
            return false;
        }

        /// <summary>
        /// Job完成时调用
        /// </summary>
        /// <param name="trigger">触发器</param>
        /// <param name="context">上下文</param>
        /// <param name="triggerInstructionCode"></param>
        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            var taskHelper = CommonContainer.Resolve<ITaskHelper>();
            taskHelper.UpdateNextFireTime(trigger.JobKey.Name, TimeZoneInfo.ConvertTimeFromUtc(context.NextFireTimeUtc.Value.DateTime, TimeZoneInfo.Local));
        }

        /// <summary>
        /// 错过触发时调用
        /// </summary>
        /// <param name="trigger">触发器</param>
        public void TriggerMisfired(ITrigger trigger)
        {

        }
    }
}
