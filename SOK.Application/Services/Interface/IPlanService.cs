using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    public interface IPlanService
    {
        Task<Plan?> GetPlanAsync(int id);

        Task<List<Plan>> GetPlansPaginatedAsync(
            Expression<Func<Plan, bool>>? filter = null,
            int page = 1,
            int pageSize = 1);

        Task CreatePlanAsync(Plan plan);

        Task<bool> DeletePlanAsync(int id);

        Task UpdatePlanAsync(Plan plan);

        Task SetActivePlanAsync(Plan plan);

        Task<Plan?> GetActivePlanAsync();
    }
}