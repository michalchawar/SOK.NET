using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class VisitRepository : UpdatableRepository<Visit, ParishDbContext>, IVisitRepository
    {
        public VisitRepository(ParishDbContext db) : base(db) {}

        public async Task SetToUnplannedAsync(int visitId)
        {
            var visit = await GetAsync(
                v => v.Id == visitId,
                includeProperties: "Submission",
                tracked: true);
            if (visit is null)
            {
                throw new InvalidOperationException($"Wizyta o identyfikatorze {visitId} nie istnieje.");
            }

            visit.OrdinalNumber = null;
            visit.PeopleCount = null;
            visit.AgendaId = null;
            visit.Status = VisitStatus.Unplanned;

            // Zdarzy się tylko po wcześniejszym wycofaniu wizyty (Withdrawn)
            if (visit.ScheduleId is null)
            {
                var historyItem = await _db.VisitSnapshots
                    .Where(vs => vs.VisitId == visitId)
                    .Where(vs => vs.ScheduleId != null)
                    .OrderByDescending(vs => vs.ChangeTime)
                    .FirstOrDefaultAsync();

                if (historyItem is not null)
                {
                    visit.ScheduleId = historyItem.ScheduleId;
                }
                else
                {
                    var plan = await _db.Plans
                        .Where(p => p.Id == visit.Submission.PlanId)
                        .Include(p => p.Schedules)
                        .Include(p => p.DefaultSchedule)
                        .FirstOrDefaultAsync();

                    if (plan is null)
                    {
                        throw new InvalidOperationException($"Plan o identyfikatorze {visit.Submission.PlanId} nie istnieje.");
                    }

                    if (plan.DefaultScheduleId is not null)
                    {
                        visit.ScheduleId = plan.DefaultScheduleId;
                    }
                    else if (plan.Schedules.Any())
                    {
                        visit.ScheduleId = plan.Schedules.First().Id;
                    }

                    else
                    {
                        throw new InvalidOperationException($"Plan o identyfikatorze {plan.Id} nie posiada żadnego harmonogramu.");
                    }
                }
            }
        }

        public async Task WithdrawAsync(int visitId)
        {
            var visit = await GetAsync(v => v.Id == visitId, tracked: true);
            if (visit is null)
            {
                throw new InvalidOperationException($"Wizyta o identyfikatorze {visitId} nie istnieje.");
            }

            visit.OrdinalNumber = null;
            visit.PeopleCount = null;
            visit.AgendaId = null;
            visit.ScheduleId = null;
            visit.Status = VisitStatus.Withdrawn;
        }
    }
}
