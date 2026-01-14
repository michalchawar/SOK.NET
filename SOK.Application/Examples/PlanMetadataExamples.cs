using SOK.Application.Common.Helpers;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Examples
{
    /// <summary>
    /// Przykłady użycia metod do zarządzania metadanymi planu.
    /// </summary>
    public class PlanMetadataExamples
    {
        private readonly IPlanService _planService;

        public PlanMetadataExamples(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Przykład ustawiania dat związanych z planem kolędy.
        /// </summary>
        public async Task SetPlanDatesExample(Plan plan)
        {
            // Ustawienie dat zbierania zgłoszeń
            await _planService.SetDateTimeMetadataAsync(plan, PlanMetadataKeys.SubmissionsStartDate, new DateTime(2024, 11, 1));
            await _planService.SetDateTimeMetadataAsync(plan, PlanMetadataKeys.SubmissionsEndDate, new DateTime(2024, 12, 20));

            // Ustawienie dat kolędy
            await _planService.SetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsStartDate, new DateTime(2024, 12, 26));
            await _planService.SetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsEndDate, new DateTime(2025, 1, 6));
        }

        /// <summary>
        /// Przykład ustawiania domyślnych godzin kolędy.
        /// </summary>
        public async Task SetDefaultVisitTimesExample(Plan plan)
        {
            // Domyślne godziny od poniedziałku do piątku: 16:00 - 20:00
            await _planService.SetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeWeekdays, new TimeOnly(16, 0));
            await _planService.SetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeWeekdays, new TimeOnly(20, 0));

            // Domyślne godziny w sobotę: 10:00 - 18:00
            await _planService.SetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSaturday, new TimeOnly(10, 0));
            await _planService.SetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSaturday, new TimeOnly(18, 0));

            // Domyślne godziny w niedzielę: 14:00 - 18:00
            await _planService.SetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSunday, new TimeOnly(14, 0));
            await _planService.SetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSunday, new TimeOnly(18, 0));
        }

        /// <summary>
        /// Przykład odczytywania metadanych planu.
        /// </summary>
        public async Task<string> GetPlanMetadataExample(Plan plan)
        {
            // Pobieranie dat
            DateTime? submissionsStart = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.SubmissionsStartDate);
            DateTime? submissionsEnd = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.SubmissionsEndDate);
            DateTime? visitsStart = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsStartDate);
            DateTime? visitsEnd = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsEndDate);

            // Pobieranie godzin dla dni tygodnia
            TimeOnly? weekdayStart = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeWeekdays);
            TimeOnly? weekdayEnd = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeWeekdays);

            // Pobieranie godzin dla soboty
            TimeOnly? saturdayStart = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSaturday);
            TimeOnly? saturdayEnd = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSaturday);

            // Pobieranie godzin dla niedzieli
            TimeOnly? sundayStart = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSunday);
            TimeOnly? sundayEnd = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSunday);

            return $"Plan: {plan.Name}\n" +
                   $"Zgłoszenia: {submissionsStart?.ToShortDateString()} - {submissionsEnd?.ToShortDateString()}\n" +
                   $"Kolęda: {visitsStart?.ToShortDateString()} - {visitsEnd?.ToShortDateString()}\n" +
                   $"Godziny pn-pt: {weekdayStart} - {weekdayEnd}\n" +
                   $"Godziny sobota: {saturdayStart} - {saturdayEnd}\n" +
                   $"Godziny niedziela: {sundayStart} - {sundayEnd}";
        }

        /// <summary>
        /// Przykład usuwania metadanych.
        /// </summary>
        public async Task DeleteMetadataExample(Plan plan)
        {
            // Usunięcie konkretnej metadanej
            await _planService.DeleteMetadataAsync(plan, PlanMetadataKeys.SubmissionsStartDate);
        }

        /// <summary>
        /// Sprawdzenie czy zgłoszenia są aktualnie otwarte.
        /// </summary>
        public async Task<bool> AreSubmissionsOpenExample(Plan plan)
        {
            DateTime? startDate = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.SubmissionsStartDate);
            DateTime? endDate = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.SubmissionsEndDate);

            if (!startDate.HasValue || !endDate.HasValue)
                return false;

            DateTime now = DateTime.Now;
            return now >= startDate.Value && now <= endDate.Value;
        }

        /// <summary>
        /// Pobranie odpowiednich godzin kolędy w zależności od dnia tygodnia.
        /// </summary>
        public async Task<(TimeOnly? start, TimeOnly? end)> GetDefaultTimesForDay(Plan plan, DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Saturday => (
                    await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSaturday),
                    await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSaturday)
                ),
                DayOfWeek.Sunday => (
                    await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSunday),
                    await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSunday)
                ),
                _ => (
                    await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeWeekdays),
                    await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeWeekdays)
                )
            };
        }
    }
}
