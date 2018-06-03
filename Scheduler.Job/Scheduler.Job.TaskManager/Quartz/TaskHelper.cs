using System;
using System.Collections.Generic;
using AutoMapper;
using Scheduler.Job.Common.Config;
using Scheduler.Job.Common.Utils;
using Scheduler.Job.Models;
using Scheduler.Job.Repository;

namespace Scheduler.Job.TaskManager.Quartz
{
    /// <summary>
    /// 任务帮助类
    /// </summary>
    public class TaskHelper : ITaskHelper
    {
        private readonly ITaskConfigRepository _taskConfigRepository;

        public TaskHelper(ITaskConfigRepository taskConfigRepository)
        {
            _taskConfigRepository = taskConfigRepository;
        }


        /// <summary>
        /// 配置文件地址
        /// </summary>
        private readonly string taskPath = FileHelper.GetAbsolutePath("Config/TaskConfig.xml");

        public bool AddTask(TaskModel model, string action)
        {
            return false;
        }

        /// <summary>
        /// 删除指定id任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        public void DeleteById(string taskId)
        {
            return;
        }

        /// <summary>
        /// 更新任务运行状态
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="status">任务状态</param>
        public void UpdateTaskStatus(string taskId, TaskStatus status)
        {
            return;
        }

        /// <summary>
        /// 更新任务下次运行时间
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="nextFireTime">下次运行时间</param>
        public void UpdateNextFireTime(string taskId, DateTime nextFireTime)
        {
            return;
        }

        /// <summary>
        /// 任务完成后，更新上次执行时间
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="recentRunTime">上次执行时间</param>
        public void UpdateRecentRunTime(string taskId, DateTime recentRunTime)
        {
            return;
        }

        /// <summary>
        /// 从数据库中读取全部任务列表
        /// </summary>
        /// <returns></returns>
        private IList<TaskModel> TaskInDb()
        {
            List<JobModel> list = _taskConfigRepository.GetAllJobList().Result;
            if (list == null)
            {
                return null;
            }
            List<TaskModel> taskModels = Mapper.Map<List<TaskModel>>(list);
            return taskModels;

        }

        /// <summary>
        /// 从配置文件中读取任务列表
        /// </summary>
        /// <returns></returns>
        private IList<TaskModel> ReadTaskConfig()
        {
            return XmlHelper.XmlToList<TaskModel>(taskPath, "Task");
        }

        /// <summary>
        /// 获取所有启用的任务
        /// </summary>
        /// <returns>所有启用的任务</returns>
        public IList<TaskModel> GetAllTaskList()
        {
            if (int.Parse(System.Configuration.ConfigurationManager.AppSettings["StorageMode"]) == 1)
            {
                return TaskInDb();
            }
            else
            {
                return ReadTaskConfig();
            }
        }


    }
}
