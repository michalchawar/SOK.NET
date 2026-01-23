using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class SuperAdminService : ISuperAdminService
    {
        private readonly IUnitOfWorkCentral _uowCentral;

        public SuperAdminService(IUnitOfWorkCentral uowCentral)
        {
            _uowCentral = uowCentral;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ParishEntry>> GetAllParishesAsync()
        {
            return await _uowCentral.Parishes.GetAllAsync();
        }

        /// <inheritdoc />
        public async Task<ParishEntry?> GetParishByIdAsync(int id)
        {
            return await _uowCentral.Parishes.GetAsync(p => p.Id == id);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateParishNameAsync(int id, string newName)
        {
            try
            {
                var parish = await _uowCentral.Parishes.GetAsync(p => p.Id == id, tracked: true);
                
                if (parish == null)
                {
                    return false;
                }

                parish.ParishName = newName;
                await _uowCentral.SaveAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
