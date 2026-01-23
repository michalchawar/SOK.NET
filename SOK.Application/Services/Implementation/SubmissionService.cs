using Microsoft.Extensions.Logging;
using SOK.Application.Common.DTO.Submission;
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
        private readonly IVisitService _visitService;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(
            IUnitOfWorkParish uow, 
            IEmailService emailService,
            IParishInfoService parishInfoService,
            IVisitService visitService,
            ILogger<SubmissionService> logger
            )
        {
            _uow = uow;
            _emailService = emailService;
            _parishInfoService = parishInfoService;
            _visitService = visitService;
            _logger = logger;

            _emailService.SetSMTPTimeout(3000);
        }

        /// <inheritdoc />
        public async Task<Submission?> GetSubmissionAsync(int id)
        {
            return 
                (await _uow.Submission.GetPaginatedAsync(
                    s => s.Id == id,
                    submitter: true,
                    addressFull: true,
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
                    address: true,
                    visit: true,
                    formSubmission: true)];

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
                plan: true,
                visit: true);

            return result;
        }

        /// <inheritdoc />
        public async Task<int?> CreateSubmissionAsync(SubmissionCreationRequestDto submissionDto)
        {
            // Budynek musi istnie� w momencie tworzenia zg�oszenia
            Building? building = await _uow.Building
                .GetAsync(b => b.Id == submissionDto.Building.Id, includeProperties: "Addresses", tracked: true);

            if (building == null)
                throw new ArgumentException($"Cannot create submission for non-existent building " +
                    $"(of number {submissionDto.Building.Number + submissionDto.Building.Letter})");

            Street? street = await _uow.Street
                .GetAsync(s => s.Id == building.StreetId, includeProperties: "Type,City", tracked: true);

            // Harmonogram r�wnie� musi istnie� w momencie tworzenia zg�oszenia
            Schedule? schedule = await _uow.Schedule
                .GetAsync(s => s.Id == submissionDto.Schedule.Id, includeProperties: "Plan", tracked: true);

            if (schedule == null)
                throw new ArgumentException($"Cannot create submission for non-existent schedule " +
                    $"(schedule '{submissionDto.Schedule.Name}')");

            // Sprawdzamy, czy zg�aszaj�cy figuruje ju� w bazie
            Submitter? submitter = await _uow.Submitter
                .GetAsync(Submitter.IsEqualExpression(submissionDto.Submitter), tracked: true);
            if (submitter == null) 
                submitter = submissionDto.Submitter;

            submissionDto.Submitter.Name = submissionDto.Submitter.Name.Trim().FirstCharToUpper();
            submissionDto.Submitter.Surname = submissionDto.Submitter.Surname.Trim().FirstCharToUpper();
            submissionDto.Submitter.Email = submissionDto.Submitter.Email.NullOrTrimmed()?.ToLower();
            submissionDto.Submitter.Phone = submissionDto.Submitter.Phone.NullOrTrimmed();

            submissionDto.SubmitterNotes = submissionDto.SubmitterNotes.NullOrTrimmed();
            submissionDto.AdminNotes = submissionDto.AdminNotes.NullOrTrimmed();
            submissionDto.ApartmentLetter = submissionDto.ApartmentLetter.NullOrTrimmed()?.ToLower();

            // Tworzymy zg�oszenie (jeszcze nie do ko�ca zaludnione)
            Submission submission = new Submission(schedule.Plan, submissionDto.Created)
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
            
            // Je�li adres istnieje, to nie mo�e mie� zg�oszenia
            if (address != null)
            {
                address = await _uow.Address.GetAsync(a => a.Id == address.Id, includeProperties: "Submissions", tracked: true);
                if (address!.Submissions.Any(s => s.PlanId == schedule.Plan.Id))
                    throw new InvalidOperationException($"Cannot create submission for address: '{address.ToString()}'. Address already has a submission.");
                
                address.Submissions.Add(submission);
                submission.Address = address;
            }
            // Je�li adres nie istnieje to utw�rz
            else
            {
                address = new Address
                {
                    ApartmentNumber = submissionDto.ApartmentNumber,
                    ApartmentLetter = string.IsNullOrWhiteSpace(submissionDto.ApartmentLetter) ? null : submissionDto.ApartmentLetter.Trim(),
                    Building = building,
                    Submissions = [submission]
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
                AuthorId = submissionDto.Author?.Id ?? null,
                Submission = submission
            };
            submission.FormSubmission = fs;

            // Tworzymy wizyt�
            Visit visit = new Visit
            {
                OrdinalNumber = null,
                Status = VisitStatus.Unplanned,
                Agenda = null,
                Schedule = schedule,
                Submission = submission
            };
            submission.Visit = visit;

            // Automatycznie dodadz� si� powi�zane obiekty
            _uow.Submission.Add(submission);
            await _uow.SaveAsync();

            submission = (await _uow.Submission.GetPaginatedAsync(
                s => s.Id == submission.Id,
                submitter: true,
                addressFull: true,
                visit: true,
                formSubmission: true
            )).FirstOrDefault()!;

            // Sprawd� czy istnieje BuildingAssignment z w��czonym auto-assign
            var buildingAssignment = await _uow.BuildingAssignment.GetAsync(
                filter: ba => 
                    ba.BuildingId == building.Id &&
                    ba.ScheduleId == schedule.Id &&
                    ba.EnableAutoAssign,
                tracked: false);

            if (buildingAssignment != null && !submissionDto.DisableAutoAssignment)
            {
                try
                {
                    // Automatyczne przypisanie wizyty do odpowiedniej agendy w danym dniu
                    await _visitService.AssignVisitToDay(
                        visitId: submission.Visit.Id, 
                        dayId: buildingAssignment.DayId, 
                        sendEmail: false);
                }
                catch (Exception ex)
                {
                    // Logowanie błędu - ale nie przerywamy procesu tworzenia zgłoszenia
                    _logger.LogError(ex, "Failed to auto-assign visit {VisitId} to day {DayId}: {ErrorMessage}", 
                        submission.Visit.Id, buildingAssignment.DayId, ex.Message);
                }
            }

            // Wysy�anie emaila potwierdzaj�cego (je�li w��czone)
            if (submissionDto.SendConfirmationEmail && !string.IsNullOrWhiteSpace(submitter.Email))
            {
                try
                {
                    // Sprawd� czy wysy�anie emaili jest w��czone globalnie
                    var emailEnabled = await _parishInfoService.GetValueAsync(InfoKeys.Email.EnableEmailSending);
                    if (emailEnabled == "true" || emailEnabled == "True")
                    {
                        var controlLinkBase = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);

                        // Utw�rz typ emaila confirmation
                        var confirmationEmail = new ConfirmationEmail(
                            submission: submission,
                            controlLinkBase: controlLinkBase ?? string.Empty
                        );

                        // Kolejkuj email - dane zostan� automatycznie wype�nione z submission
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
                    _logger.LogError(ex, "Failed to send confirmation email: {ErrorMessage}", ex.Message);
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
            string? activePlanIdStr = await _uow.ParishInfo.GetValueAsync("ActivePlanId");
            if (string.IsNullOrEmpty(activePlanIdStr))
                return null;

            if (!int.TryParse(activePlanIdStr, out int activePlanId))
                return null;

            return await _uow.Submission.GetRandomAsync(s => s.PlanId == activePlanId);
        }

        /// <inheritdoc />
        public async Task<Submission?> FindSubmissionByAddressAsync(int buildingId, int? apartmentNumber, string? apartmentLetter)
        {
            var address = await _uow.Address.GetAsync(
                filter: a => a.BuildingId == buildingId &&
                            a.ApartmentNumber == apartmentNumber &&
                            a.ApartmentLetter == apartmentLetter,
                tracked: false
            );

            if (address == null)
                return null;

            var submission = await _uow.Submission.GetAsync(
                filter: s => s.AddressId == address.Id,
                includeProperties: "Visit.Agenda.Day",
                tracked: false
            );

            return submission;
        }

        /// <inheritdoc />
        public async Task AppendAdminNotesAsync(int submissionId, string text)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                tracked: true
            );

            if (submission == null)
                throw new ArgumentException($"Submission with ID {submissionId} not found.");

            submission.AdminNotes = (submission.AdminNotes ?? "") + text;
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<int> CreateSubmissionDuringVisitAsync(int buildingId, int? apartmentNumber, string? apartmentLetter, int scheduleId)
        {
            // Utw�rz zg�oszenie z danymi
            int? submissionId = await CreateSubmissionAsync(new SubmissionCreationRequestDto
            {
                Building = new Building { Id = buildingId },
                ApartmentNumber = apartmentNumber,
                ApartmentLetter = apartmentLetter,
                Schedule = new Schedule { Id = scheduleId },
                AdminNotes = "Dodano podczas przeprowadzania wizyty danego dnia.",
                Submitter = new Submitter
                {
                    Name = "(-)",
                    Surname = "(-)",
                    Email = null,
                    Phone = null
                },
                Method = SubmitMethod.DuringVisits,
                Created = DateTime.Now,
                SendConfirmationEmail = false,
                DisableAutoAssignment = true
            });

            if (submissionId == null)
                throw new InvalidOperationException("Failed to create submission during visit.");

            return submissionId.Value;
        }

        /// <inheritdoc />
        public async Task WithdrawSubmissionAsync(int id)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == id,
                includeProperties: "Visit",
                tracked: true
            );

            if (submission == null)
                throw new ArgumentException($"Submission with ID {id} not found.");

            await _uow.Visit.WithdrawAsync(submission.Visit.Id);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task RestoreSubmissionAsync(int id)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == id,
                includeProperties: "Visit",
                tracked: true
            );

            if (submission == null)
                throw new ArgumentException($"Submission with ID {id} not found.");

            await _uow.Visit.SetToUnplannedAsync(submission.Visit.Id);
            await _uow.SaveAsync();
        }
    }
}