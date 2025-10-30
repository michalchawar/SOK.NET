using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class SubmitterService : ISubmitterService
    {
        private readonly IUnitOfWorkParish _uow;

        public SubmitterService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<Submitter?> GetSubmitterAsync(int id)
        {
            return await _uow.Submitter.GetAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task<Submitter?> GetSubmitterAsync(string uid)
        {
            return await _uow.Submitter.GetAsync(s => s.UniqueId.ToString() == uid);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Submitter>> GetSubmittersPaginatedAsync(
            Expression<Func<Submitter, bool>>? filter = null,
            int page = 1,
            int pageSize = 1)
        {
            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            return await _uow.Submitter.GetPaginatedAsync(filter, pageSize: pageSize, page: page);
        }

        /// <inheritdoc />
        public async Task CreateSubmitterAsync(Submitter submitter)
        {
            _uow.Submitter.Add(submitter);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSubmitterAsync(int id)
        {
            try
            {
                Submitter? submitter = await _uow.Submitter.GetAsync(s => s.Id == id);
                if (submitter != null)
                {
                    _uow.Submitter.Remove(submitter);
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
        public async Task UpdateSubmitterAsync(Submitter submitter)
        {
            _uow.Submitter.Update(submitter);
            await _uow.SaveAsync();
        }
    }
}