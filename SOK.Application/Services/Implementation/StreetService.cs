using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class StreetService : IStreetService
    {
        private readonly IUnitOfWorkParish _uow;

        /// <inheritdoc />
        public StreetService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<Street?> GetStreetAsync(int id)
        {
            return await _uow.Street.GetAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Street>> GetAllStreetsAsync(
            Expression<Func<Street, bool>>? filter = null,
            bool buildings = false)
        {
            return await _uow.Street.GetAllAsync(filter, includeProperties: buildings ? "Buildings" : null, orderBy: s => s.Name);
        }

        /// <inheritdoc />
        public async Task CreateStreetAsync(Street street)
        {
            _uow.Street.Add(street);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteStreetAsync(int id)
        {
            try
            {
                Street? street = await _uow.Street.GetAsync(s => s.Id == id);
                if (street != null)
                {
                    _uow.Street.Remove(street);
                    await _uow.SaveAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task UpdateStreetAsync(Street street)
        {
            _uow.Street.Update(street);
            await _uow.SaveAsync();
        }
    }
}