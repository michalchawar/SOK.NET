using Microsoft.AspNetCore.Http;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class SubmissionService : ISubmissionService
    {
        private readonly IUnitOfWorkParish _uow;

        public SubmissionService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<Submission?> GetSubmissionAsync(int id)
        {
            return 
                (await _uow.Submission.GetPaginatedAsync(
                    s => s.Id == id,
                    submitter: true,
                    address: true))
                .FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<Submission?> GetSubmissionAsync(string submissionUid)
        {
            Guid submissionGuid = Guid.Parse(submissionUid);

            Submission? result = 
                (await _uow.Submission.GetPaginatedAsync(
                    s => s.UniqueId == submissionGuid,
                    submitter: true,
                    address: true,
                    visit: true))
                .FirstOrDefault();

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Submission>> GetSubmissionsPaginated(
            Expression<Func<Submission, bool>>? filter = null,
            int page = 1,
            int pageSize = 1)
        {
            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            List<Submission> result = [.. await _uow.Submission.GetPaginatedAsync(
                    filter,
                    pageSize: pageSize,
                    page: page,
                    submitter: true,
                    address: true)];

            return result;
        }

        /// <inheritdoc />
        public async Task CreateSubmissionAsync(SubmissionCreationRequestDto submissionDto)
        {
            // Budynek musi istnieć w momencie tworzenia zgłoszenia
            Building? building = await _uow.Building
                .GetAsync(b => b.Id == submissionDto.Building.Id, includeProperties: "Addresses", tracked: true);

            if (building == null)
                throw new ArgumentException($"Cannot create submission for non-existent building " +
                    $"(of number {submissionDto.Building.Number + submissionDto.Building.Letter})");

            Street? street = await _uow.Street
                .GetAsync(s => s.Id == building.StreetId, includeProperties: "Type,City", tracked: true);

            // Harmonogram również musi istnieć w momencie tworzenia zgłoszenia
            Schedule? schedule = await _uow.Schedule
                .GetAsync(s => s.Id == submissionDto.Schedule.Id, tracked: true);

            if (schedule == null)
                throw new ArgumentException($"Cannot create submission for non-existent schedule " +
                    $"(schedule '{submissionDto.Schedule.Name}')");

            // Sprawdzamy, czy zgłaszający figuruje już w bazie
            Submitter? submitter = await _uow.Submitter
                .GetAsync(Submitter.IsEqualExpression(submissionDto.Submitter), tracked: true);
            if (submitter == null) 
                submitter = submissionDto.Submitter;

            submissionDto.SubmitterNotes = string.IsNullOrWhiteSpace(submissionDto.SubmitterNotes) ? null : submissionDto.SubmitterNotes.Trim([' ', '\n', '\r']);
            submissionDto.AdminNotes = string.IsNullOrWhiteSpace(submissionDto.AdminNotes) ? null : submissionDto.AdminNotes.Trim([' ', '\n', '\r']);
            submissionDto.ApartmentLetter = string.IsNullOrWhiteSpace(submissionDto.ApartmentLetter) ? null : submissionDto.ApartmentLetter.Trim([' ', '\n', '\r']).ToLower();

            // Tworzymy zgłoszenie (jeszcze nie do końca zaludnione)
            Submission submission = new Submission
            {
                Submitter = submitter,
                SubmitterNotes = submissionDto.SubmitterNotes,
                AdminMessage = null,
                AdminNotes = submissionDto.AdminNotes,
                NotesStatus = NotesFulfillmentStatus.NA,
                Address = null,
                FormSubmission = null,
                Visit = null
            };

            if (!string.IsNullOrEmpty(submission.SubmitterNotes))
                submission.NotesStatus = NotesFulfillmentStatus.Pending;

            // Znajdujemy lub tworzymy adres
            Address? address = building.Addresses
                .FirstOrDefault(a => a.ApartmentNumber == submissionDto.ApartmentNumber
                                  && a.ApartmentLetter == submissionDto.ApartmentLetter);
            // Jeśli adres istnieje, to nie może mieć zgłoszenia
            if (address != null)
            {
                address = await _uow.Address.GetAsync(a => a.Id == address.Id, includeProperties: "Submission", tracked: true);
                if (address!.Submission != null)
                    throw new InvalidOperationException($"Cannot create submission for address: '{address.ToString()}'. Address already has a submission.");
                
                address.Submission = submission;
                submission.Address = address;
            }
            // Jeśli adres nie istnieje to utwórz
            else
            {
                address = new Address
                {
                    ApartmentNumber = submissionDto.ApartmentNumber,
                    ApartmentLetter = string.IsNullOrWhiteSpace(submissionDto.ApartmentLetter) ? null : submissionDto.ApartmentLetter.Trim([' ', '\n', '\r']),
                    Building = building,
                    Submission = submission
                };
            }
            submission.Address = address;

            // Tworzymy FormSubmission
            FormSubmission fs = new FormSubmission
            {
                Name = submitter.Name,
                Surname = submitter.Surname,
                Email = submitter.Email,
                Phone = submitter.Phone,
                SubmitterNotes = submission.SubmitterNotes,
                ScheduleName = schedule.Name,
                Apartment = address.ApartmentNumber + address.ApartmentLetter,
                Building = building.Number + building.Letter,
                StreetSpecifier = street!.Type.FullName,
                Street = street.Name,
                City = street.City.Name,
                Method = submissionDto.Method,
                IP = submissionDto.IPAddress ?? "",
                Author = submissionDto.Author,
                Submission = submission
            };
            submission.FormSubmission = fs;

            // Tworzymy wizytę
            Visit visit = new Visit
            {
                OrdinalNumber = null,
                Status = VisitStatus.Unplanned,
                Agenda = null,
                Schedule = schedule,
                Submission = submission
            };
            submission.Visit = visit;

            // Automatycznie dodadzą się powiązane obiekty
            _uow.Submission.Add(submission);
            await _uow.SaveAsync();

            return;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            try
            {
                Submission? submission = await _uow.Submission.GetAsync(s => s.Id == id);
                if (submission != null)
                {
                    _uow.Submission.Remove(submission);
                    await _uow.SaveAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task UpdateSubmissionAsync(Submission submission)
        {
            _uow.Submission.Update(submission);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<Submission?> GetRandomSubmissionAsync()
        {
            return await _uow.Submission.GetRandomAsync();
        }
    }
}
