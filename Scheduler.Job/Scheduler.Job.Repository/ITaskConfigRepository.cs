using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Job.Repository
{
    public interface ITaskConfigRepository
    {
        /// <summary>
        /// 查询库中所有的任务
        /// </summary>
        /// <returns></returns>
        Task<List<JobModel>> GetAllJobList();
    }
}
