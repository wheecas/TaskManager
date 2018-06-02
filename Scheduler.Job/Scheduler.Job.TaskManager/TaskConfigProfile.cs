using AutoMapper;
using Scheduler.Job.Models;
using Scheduler.Job.Repository;

namespace Scheduler.Job.TaskManager
{
    public class TaskConfigProfile : Profile
    {
        public TaskConfigProfile()
        {
            CreateMap<JobModel, TaskModel>()
                .ForMember(dto => dto.ModifyTime, opt => opt.MapFrom(o => o.UpdateTime));
        }
    }
}
