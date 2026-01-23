using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;

namespace SOK.Application.Services.Implementation
{
    /// <summary>
    /// Usługa do ładowania i przechowywania aktualnie wybranej parafii.
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

        private bool _isParishSet = false;

        public CurrentParishService(IUnitOfWorkCentral central, ICryptoService cryptoService)
        {
            _uow = central;
            _cryptoService = cryptoService;
        }

        /// <inheritdoc/>
        public async Task<bool> SetParishAsync(string parishUid)
        {
            var parish = await _uow.Parishes.GetAsync(p => p.UniqueId.ToString() == parishUid);

            if (parish is null)
            {
                Console.WriteLine($"Parish with UID {parishUid} not found.");
                return false;
            }
            ParishUid = parishUid;
            ConnectionString = _cryptoService.Decrypt(parish.EncryptedConnectionString);
            _isParishSet = true;

            return true;
        }

        /// <inheritdoc />
        public Task<ParishEntry?> GetCurrentParishAsync()
        {
            return _uow.Parishes.GetAsync(p => p.UniqueId.ToString() == ParishUid);
        }
        
        /// <inheritdoc />
        public bool IsParishSet()
        {
            return _isParishSet;
        }
    }
}