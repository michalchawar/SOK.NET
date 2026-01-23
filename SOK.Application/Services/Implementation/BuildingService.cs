using SOK.Application.Common.DTO.Address;
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
            return 
                (await _uow.Building.GetPaginatedAsync(
                    b => b.Id == id,
                    street: true,
                    tracked: true))
                .FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Building>> GetBuildingsPaginatedAsync(
           Expression<Func<Building, bool>>? filter = null,
           int page = 1,
           int pageSize = 1)
        {
           if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
           if (page < 1) throw new ArgumentException("Page must be positive.");

           return await _uow.Building.GetPaginatedAsync(filter, pageSize: pageSize, page: page);
        }

        /// <inheritdoc />
        public async Task CreateBuildingAsync(Building building)
        {
            if (building.Letter != null)
                building.Letter = building.Letter.ToUpper();

            _uow.Building.Add(building);

            try
            {
                await _uow.SaveAsync();
            } catch (Exception ex)
            {
                _uow.Building.Remove(building);
                if (ex.InnerException?.Message.Contains("Cannot insert duplicate key row in object") ?? false)
                {
                    throw new InvalidOperationException("A building with the same data already exists on the given street.");
                }

                throw;
            }
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
            if (building.Letter != null)
                building.Letter = building.Letter.ToUpper();
                
            _uow.Building.Update(building);

            try
            {
                await _uow.SaveAsync();
            } catch (Exception ex)
            {
                _uow.Building.Remove(building);
                if (ex.InnerException?.Message.Contains("Cannot insert duplicate key row in object") ?? false)
                {
                    throw new InvalidOperationException("A building with the same data already exists on the given street.");
                }

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<BuildingSimpleDto>> GetAllBuildingsAsync()
        {
            var buildings = await _uow.Building.GetAllAsync(
                includeProperties: "Street.Type,Street.City"
            );

            return buildings.Select(b => new BuildingSimpleDto
            {
                Id = b.Id,
                Number = b.Number,
                Letter = b.Letter,
                StreetId = b.StreetId,
                StreetName = b.Street.Name,
                StreetType = b.Street.Type.Abbreviation ?? "",
                CityName = b.Street.City.Name
            }).ToList();
        }
    }
}