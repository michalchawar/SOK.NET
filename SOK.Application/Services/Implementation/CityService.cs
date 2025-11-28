using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class CityService : ICityService
    {
        private readonly IUnitOfWorkParish _uow;

        /// <inheritdoc />
        public CityService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<City?> GetCityAsync(int id)
        {
            return await _uow.City.GetAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<City>> GetAllCitiesAsync(
            Expression<Func<City, bool>>? filter = null,
            bool streets = false)
        {
            string include = "";
            if (streets) include += "Streets,";

            return await _uow.City.GetAllAsync(filter, includeProperties: include, orderBy: s => s.Name);
        }

        /// <inheritdoc />
        public async Task CreateCityAsync(City City)
        {
            _uow.City.Add(City);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteCityAsync(int id)
        {
            try
            {
                City? City = await _uow.City.GetAsync(s => s.Id == id);
                if (City != null)
                {
                    _uow.City.Remove(City);
                    await _uow.SaveAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task UpdateCityAsync(City city)
        {
            _uow.City.Update(city);
            await _uow.SaveAsync();
        }
    }
}