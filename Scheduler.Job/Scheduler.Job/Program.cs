using Autofac;
using AutoMapper;
using Infrustructure.Utilities;
using Scheduler.Job.Repository.DBContext;
using Scheduler.Job.TaskManager;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Scheduler.Job
{
    class Program
    {
        static void Main(string[] args)
        {
            AutoFac();
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<TaskConfigProfile>();//添加一个配置文件
            });
            Database.SetInitializer<TaskConfigContext>(null);//关闭修改数据库表结构
            HostFactory.Run(x =>
            {
                x.Service<SchedulerStart>(s =>
                {
                    s.ConstructUsing(name => new SchedulerStart());//配置一个完全定制的服务,对Topshelf没有依赖关系。常用的方式。
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                // 服务使用NETWORK_SERVICE内置帐户运行。身份标识，有好几种方式，如：x.RunAs("username", "password");  x.RunAsPrompt(); x.RunAsNetworkService(); 等
                x.RunAsLocalSystem();

                x.SetDescription("TaskManagerService Host");//安装服务后，服务的描述
                x.SetDisplayName("TaskManagerService");//显示名称
                x.SetServiceName("TaskManagerService");//服务名称
            });

        }

        public static void AutoFac()
        {
            var builder = new ContainerBuilder();

            #region 程序集注入

            var assembly = new Assembly[]
            {
                Assembly.Load("Scheduler.Job.Repository"),
                Assembly.Load("Scheduler.Job.Service"),
                Assembly.Load("Scheduler.Job.TaskManager"),
                Assembly.Load("Infrustructure")
            };

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            #endregion

            var container = builder.Build();
            CommonContainer.RegisterContainer(container);
        }
    }
}
