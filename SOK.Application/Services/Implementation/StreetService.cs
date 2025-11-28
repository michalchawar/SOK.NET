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
            bool buildings = false,
            bool type = false)
        {
            string include = "";
            if (buildings) include += "Buildings,";
            if (type) include += "Type";

            return await _uow.Street.GetAllAsync(filter, includeProperties: include, orderBy: s => s.Name);
        }

        /// <inheritdoc />
        public async Task CreateStreetAsync(Street street)
        {
            _uow.Street.Add(street);

            try
            {
                await _uow.SaveAsync();
            } catch (Exception ex)
            {
                _uow.Street.Remove(street);
                if (ex.InnerException?.Message.Contains("Cannot insert duplicate key row in object") ?? false)
                {
                    throw new InvalidOperationException("A street with the same data already exists in the database.");
                }

                throw;
            }
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

            try
            {
                await _uow.SaveAsync();
            } catch (Exception ex)
            {
                _uow.Street.Remove(street);
                if (ex.InnerException?.Message.Contains("Cannot insert duplicate key row in object") ?? false)
                {
                    throw new InvalidOperationException("A street with the same data already exists in the database.");
                }

                throw;
            }
        }

        public Task<IEnumerable<StreetSpecifier>> GetAllStreetSpecifiersAsync(
            Expression<Func<StreetSpecifier, bool>>? filter = null)
        {
            return _uow.StreetSpecifier.GetAllAsync(
                filter,
                orderBy: s => s.FullName);
        }
    }
}