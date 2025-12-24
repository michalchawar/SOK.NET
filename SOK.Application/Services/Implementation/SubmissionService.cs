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
        private readonly IVisitService _visitService;

        public SubmissionService(
            IUnitOfWorkParish uow, 
            IEmailService emailService,
            IParishInfoService parishInfoService,
            IVisitService visitService
            )
        {
            _uow = uow;
            _emailService = emailService;
            _parishInfoService = parishInfoService;
            _visitService = visitService;

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
            
            // Jeśli adres istnieje, to nie może mieć zgłoszenia
            if (address != null)
            {
                address = await _uow.Address.GetAsync(a => a.Id == address.Id, includeProperties: "Submissions", tracked: true);
                if (address!.Submissions.Any(s => s.PlanId == schedule.Plan.Id))
                    throw new InvalidOperationException($"Cannot create submission for address: '{address.ToString()}'. Address already has a submission.");
                
                address.Submissions.Add(submission);
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

            // Sprawdź czy istnieje BuildingAssignment z włączonym auto-assign
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
                    Console.WriteLine($"Failed to auto-assign visit {submission.Visit.Id} to day {buildingAssignment.DayId}: {ex.Message}");
                }
            }

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
            // Utwórz zgłoszenie z danymi
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
    }
}
