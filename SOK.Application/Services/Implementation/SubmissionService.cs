using Microsoft.AspNetCore.Http;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Helpers.EmailTypes;
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
        private readonly IEmailService _emailService;
        private readonly IParishInfoService _parishInfoService;

        public SubmissionService(
            IUnitOfWorkParish uow, 
            IEmailService emailService,
            IParishInfoService parishInfoService)
        {
            _uow = uow;
            _emailService = emailService;
            _parishInfoService = parishInfoService;

            _emailService.SetSMTPTimeout(3000);
        }

        /// <inheritdoc />
        public async Task<Submission?> GetSubmissionAsync(int id)
        {
            return 
                (await _uow.Submission.GetPaginatedAsync(
                    s => s.Id == id,
                    submitter: true,
                    address: true,
                    visit: true,
                    formSubmission: true,
                    plan: true,
                    tracked: true))
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
                    addressFull: true,
                    visit: true,
                    formSubmission: true,
                    plan: true,
                    tracked: true))
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
        public async Task<(IEnumerable<Submission> submissions, int totalCount)> GetSubmissionsPaginatedWithSorting(
            Expression<Func<Submission, bool>>? filter = null,
            string sortBy = "time",
            string order = "desc",
            int page = 1,
            int pageSize = 1)
        {
            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            var result = await _uow.Submission.GetPaginatedWithSortingAsync(
                filter,
                sortBy: sortBy,
                order: order,
                pageSize: pageSize,
                page: page,
                submitter: true,
                address: true,
                visit: true);

            return result;
        }

        /// <inheritdoc />
        public async Task<int?> CreateSubmissionAsync(SubmissionCreationRequestDto submissionDto)
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
                .GetAsync(s => s.Id == submissionDto.Schedule.Id, includeProperties: "Plan", tracked: true);

            if (schedule == null)
                throw new ArgumentException($"Cannot create submission for non-existent schedule " +
                    $"(schedule '{submissionDto.Schedule.Name}')");

            // Sprawdzamy, czy zgłaszający figuruje już w bazie
            Submitter? submitter = await _uow.Submitter
                .GetAsync(Submitter.IsEqualExpression(submissionDto.Submitter), tracked: true);
            if (submitter == null) 
                submitter = submissionDto.Submitter;

            submissionDto.Submitter.Name = submissionDto.Submitter.Name.Trim([' ', '\n', '\r']).FirstCharToUpper();
            submissionDto.Submitter.Surname = submissionDto.Submitter.Surname.Trim([' ', '\n', '\r']).FirstCharToUpper();
            submissionDto.Submitter.Email = string.IsNullOrWhiteSpace(submissionDto.Submitter.Email) ? null : submissionDto.Submitter.Email.Trim([' ', '\n', '\r']).ToLower();
            submissionDto.Submitter.Phone = string.IsNullOrWhiteSpace(submissionDto.Submitter.Phone) ? null : submissionDto.Submitter.Phone.Trim([' ', '\n', '\r']);

            submissionDto.SubmitterNotes = string.IsNullOrWhiteSpace(submissionDto.SubmitterNotes) ? null : submissionDto.SubmitterNotes.Trim([' ', '\n', '\r']);
            submissionDto.AdminNotes = string.IsNullOrWhiteSpace(submissionDto.AdminNotes) ? null : submissionDto.AdminNotes.Trim([' ', '\n', '\r']);
            submissionDto.ApartmentLetter = string.IsNullOrWhiteSpace(submissionDto.ApartmentLetter) ? null : submissionDto.ApartmentLetter.Trim([' ', '\n', '\r']).ToLower();

            // Tworzymy zgłoszenie (jeszcze nie do końca zaludnione)
            Submission submission = new Submission(schedule.Plan)
            {
                Submitter = submitter,
                SubmitterNotes = submissionDto.SubmitterNotes,
                AdminMessage = null,
                AdminNotes = submissionDto.AdminNotes,
                NotesStatus = NotesFulfillmentStatus.NA,
                Address = null!,
                FormSubmission = null!,
                Visit = null!
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

            submission = (await _uow.Submission.GetPaginatedAsync(
                s => s.Id == submission.Id,
                submitter: true,
                addressFull: true,
                visit: true,
                formSubmission: true
            )).FirstOrDefault()!;

            // Wysyłanie emaila potwierdzającego (jeśli włączone)
            if (submissionDto.SendConfirmationEmail && !string.IsNullOrWhiteSpace(submitter.Email))
            {
                try
                {
                    // Sprawdź czy wysyłanie emaili jest włączone globalnie
                    var emailEnabled = await _parishInfoService.GetValueAsync(InfoKeys.Email.EnableEmailSending);
                    if (emailEnabled == "true" || emailEnabled == "True")
                    {
                        var controlLinkBase = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);

                        // Utwórz typ emaila confirmation
                        var confirmationEmail = new ConfirmationEmail(
                            submission: submission,
                            controlLinkBase: controlLinkBase ?? string.Empty
                        );

                        // Kolejkuj email - dane zostaną automatycznie wypełnione z submission
                        await _emailService.QueueEmailAsync(
                            emailType: confirmationEmail,
                            submission: submission,
                            forceSend: true
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Logowanie błędu, ale nie przerywamy procesu tworzenia zgłoszenia
                    Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
                }
            }

            return submission.Id;
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
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task UpdateSubmissionAsync(Submission submission)
        {
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<Submission?> GetRandomSubmissionAsync()
        {
            return await _uow.Submission.GetRandomAsync();
        }

        /// <inheritdoc />
        public async Task<bool> SendConfirmationEmailAsync(int submissionId)
        {
            Submission? submission = (await _uow.Submission.GetPaginatedAsync(
                s => s.Id == submissionId,
                submitter: true,
                addressFull: true,
                visit: true,
                formSubmission: true
            )).FirstOrDefault();
            
            if (submission is null)
            {
                throw new ArgumentException("Nie znaleziono powiązanego zgłoszenia.");
            }
            if (string.IsNullOrWhiteSpace(submission.Submitter.Email))
            {
                throw new ArgumentException("Zgłaszający nie posiada adresu email.");
            }

            var controlLinkBase = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);
            if (string.IsNullOrWhiteSpace(controlLinkBase))
            {
                throw new InvalidOperationException("Nie skonfigurowano bazowego URL aplikacji.");
            }

            try
            {
                // Utwórz typ emaila confirmation
                var confirmationEmail = new ConfirmationEmail(
                    submission: submission,
                    controlLinkBase: controlLinkBase
                );

                int? emailLogId = await _emailService.QueueEmailAsync(
                    emailType: confirmationEmail,
                    submission: submission,
                    forceSend: true
                );

                if (emailLogId.HasValue)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<string> PreviewConfirmationEmailAsync(int submissionId)
        {
            Submission? submission = (await _uow.Submission.GetPaginatedAsync(
                s => s.Id == submissionId,
                submitter: true,
                addressFull: true,
                visit: true,
                formSubmission: true
            )).FirstOrDefault();
            
            if (submission is null)
            {
                throw new ArgumentException("Nie znaleziono powiązanego zgłoszenia.");
            }
            if (string.IsNullOrWhiteSpace(submission.Submitter.Email))
            {
                throw new ArgumentException("Zgłaszający nie posiada adresu email.");
            }

            var controlLinkBase = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);
            if (string.IsNullOrWhiteSpace(controlLinkBase))
            {
                throw new InvalidOperationException("Nie skonfigurowano bazowego URL aplikacji.");
            }

            try
            {
                // Utwórz typ emaila confirmation
                var confirmationEmail = new ConfirmationEmail(
                    submission: submission,
                    controlLinkBase: controlLinkBase
                );

                string emailPreview = await _emailService.PreviewEmailAsync(
                    emailType: confirmationEmail
                );

                return emailPreview;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<bool> SendInvalidEmailAsync(int submissionId, string to = "")
        {
            
            Submission? submission = (await _uow.Submission.GetPaginatedAsync(
                s => s.Id == submissionId,
                submitter: true,
                addressFull: true,
                visit: true,
                formSubmission: true
            )).FirstOrDefault();
            
            if (submission is null)
            {
                throw new ArgumentException("Nie znaleziono powiązanego zgłoszenia.");
            }
            if (string.IsNullOrWhiteSpace(submission.Submitter.Email))
            {
                throw new ArgumentException("Zgłaszający nie posiada adresu email.");
            }
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Nie podano nowego adresu email.");
            }

            var controlLinkBase = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);
            if (string.IsNullOrWhiteSpace(controlLinkBase))
            {
                throw new InvalidOperationException("Nie skonfigurowano bazowego URL aplikacji.");
            }

            try
            {
                // Utwórz typ emaila invalid
                var invalidEmail = new InvalidEmailNotification(
                    submission: submission,
                    newEmail: to
                );

                int? emailLogId = await _emailService.QueueEmailAsync(
                    emailType: invalidEmail,
                    submission: submission,
                    forceSend: true
                );

                if (emailLogId.HasValue)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public async Task<string> PreviewInvalidEmailAsync(int submissionId, string to = "")
        {
            Submission? submission = (await _uow.Submission.GetPaginatedAsync(
                s => s.Id == submissionId,
                submitter: true,
                addressFull: true,
                visit: true,
                formSubmission: true
            )).FirstOrDefault();
            
            if (submission is null)
            {
                throw new ArgumentException("Nie znaleziono powiązanego zgłoszenia.");
            }
            if (string.IsNullOrWhiteSpace(submission.Submitter.Email))
            {
                throw new ArgumentException("Zgłaszający nie posiada adresu email.");
            }
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Nie podano nowego adresu email.");
            }

            var controlLinkBase = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);
            if (string.IsNullOrWhiteSpace(controlLinkBase))
            {
                throw new InvalidOperationException("Nie skonfigurowano bazowego URL aplikacji.");
            }

            try
            {
                // Utwórz typ emaila confirmation
                var confirmationEmail = new InvalidEmailNotification(
                    submission: submission,
                    newEmail: to
                );

                string emailPreview = await _emailService.PreviewEmailAsync(
                    emailType: confirmationEmail
                );

                return emailPreview;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
