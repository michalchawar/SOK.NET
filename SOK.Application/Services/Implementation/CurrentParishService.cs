using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Implementation
{
    /// <summary>
    /// Us³uga do ³adowania i przechowywania aktualnie wybranej parafii.
    /// Parafia jest pobierana z centralnej bazy danych na podstawie jej publicznego 
    /// unikalnego identyfikatora (<c>UID</c>).
    /// </summary>
    public class CurrentParishService : ICurrentParishService
    {
        private readonly IUnitOfWorkCentral _uow;

        /// <inheritdoc/>
        public string? ParishUid { get; private set; }
        
        /// <inheritdoc/>
        public string? ConnectionString { get; private set; }

        public CurrentParishService(IUnitOfWorkCentral central)
        {
            _uow = central;
        }

        /// <inheritdoc/>
        public async Task<bool> SetParishAsync(string parishUid)
        {
            var parish = await _uow.Parishes.GetAsync(p => p.UniqueId.ToString() == parishUid);

            if (parish != null)
            {
                ParishUid = parishUid;
                ConnectionString = parish.EncryptedConnectionString;

                return true;
            }
            else
            {
                throw new InvalidOperationException($"Parish with UID {parishUid} not found.");
            }
        }
    }
}