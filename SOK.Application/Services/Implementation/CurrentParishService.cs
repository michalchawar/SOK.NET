using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
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
        private readonly ICryptoService _cryptoService;

        /// <inheritdoc/>
        public string? ParishUid { get; private set; }
        
        /// <inheritdoc/>
        public string? ConnectionString { get; private set; }

        public CurrentParishService(IUnitOfWorkCentral central, ICryptoService cryptoService)
        {
            _uow = central;
            _cryptoService = cryptoService;
        }

        /// <inheritdoc/>
        public async Task<bool> SetParishAsync(string parishUid)
        {
            var parish = await _uow.Parishes.GetAsync(p => p.UniqueId.ToString() == parishUid);

            if (parish != null)
            {
                ParishUid = parishUid;
                ConnectionString = _cryptoService.Decrypt(parish.EncryptedConnectionString);

                return true;
            }
            else
            {
                throw new InvalidOperationException($"Parish with UID {parishUid} not found.");
            }
        }

        /// <inheritdoc />
        public Task<ParishEntry?> GetCurrentParishAsync()
        {
            return _uow.Parishes.GetAsync(p => p.UniqueId.ToString() == ParishUid);
        }
    }
}