using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Job.Repository.DBContext
{
  public  class TaskConfigContext : DbContext
    {
        public TaskConfigContext(): base("name=SqlConnect")
        {

        }

        public DbSet<JobModel> JobModels { get; set; }

    }
}
