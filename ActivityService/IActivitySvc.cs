using ModelService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActivityService
{
    public interface IActivitySvc
    {
        Task AddUserActivity(ActivityModel model);
        Task<List<ActivityModel>> GetUserActivities(string userId);
        Task<List<ActivityModel>> GetActivities();
    }
}
