using Scheduler.Job.Models;
using System;
using System.Collections.Generic;

namespace Scheduler.Job.TaskManager.Quartz
{
    /// <summary>
    /// 任务帮助类
    /// </summary>
    public interface ITaskHelper
    {
        bool AddTask(TaskModel model, string action);

        /// <summary>
        /// 删除指定id任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        void DeleteById(string taskId);

        /// <summary>
        /// 更新任务运行状态
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="status">任务状态</param>
        void UpdateTaskStatus(string taskId, TaskStatus status);

        /// <summary>
        /// 更新任务下次运行时间
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="nextFireTime">下次运行时间</param>
        void UpdateNextFireTime(string taskId, DateTime nextFireTime);

        /// <summary>
        /// 任务完成后，更新上次执行时间
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="recentRunTime">上次执行时间</param>
        void UpdateRecentRunTime(string taskId, DateTime recentRunTime);

        /// <summary>
        /// 获取所有启用的任务
        /// </summary>
        /// <returns>所有启用的任务</returns>
        IList<TaskModel> GetAllTaskList();
    }
}
