using Scheduler.Job.Repository.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Data.Entity;

namespace Scheduler.Job.Repository
{
    class TaskConfigRepository : ITaskConfigRepository
    {
  
        public TaskConfigRepository()
        {

        }


        public async Task<List<JobModel>> GetAllJobList()
        {
            using (var ctx=new TaskConfigContext())
            {
                var list =await ctx.JobModels.ToListAsync();
                return list;
            }
        }

        
    }
}
