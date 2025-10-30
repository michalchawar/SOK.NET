using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class BuildingService : IBuildingService
    {
        private readonly IUnitOfWorkParish _uow;

        public BuildingService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<Building?> GetBuildingAsync(int id)
        {
            return await _uow.Building.GetAsync(b => b.Id == id);
        }

        /// <inheritdoc />
        //public async Task<IEnumerable<Building>> GetBuildingsPaginatedAsync(
        //    Expression<Func<Building, bool>>? filter = null,
        //    int page = 1,
        //    int pageSize = 1)
        //{
        //    if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
        //    if (page < 1) throw new ArgumentException("Page must be positive.");

        //    return await _uow.Building.GetPaginatedAsync(filter, pageSize: pageSize, page: page);
        //}

        /// <inheritdoc />
        public async Task CreateBuildingAsync(Building building)
        {
            _uow.Building.Add(building);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteBuildingAsync(int id)
        {
            try
            {
                Building? building = await _uow.Building.GetAsync(b => b.Id == id);
                if (building != null)
                {
                    _uow.Building.Remove(building);
                    await _uow.SaveAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task UpdateBuildingAsync(Building building)
        {
            _uow.Building.Update(building);
            await _uow.SaveAsync();
        }
    }
}