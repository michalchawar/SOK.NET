using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Globalization;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class PrintService : IPrintService
    {
        private readonly IUnitOfWorkParish _uow;

        static PrintService()
        {
            // Rejestracja czcionki Lato (Google Fonts)
            // Jeśli nie masz plików czcionek, QuestPDF użyje domyślnych czcionek systemowych
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var fontsPath = Path.Combine(basePath, "Fonts");

                if (Directory.Exists(fontsPath))
                {
                    // Rejestruj czcionki Lato jeśli istnieją
                    var latoRegular = Path.Combine(fontsPath, "Lato-Regular.ttf");
                    var latoBold = Path.Combine(fontsPath, "Lato-Bold.ttf");

                    if (File.Exists(latoRegular))
                        QuestPDF.Drawing.FontManager.RegisterFont(File.OpenRead(latoRegular));
                    
                    if (File.Exists(latoBold))
                        QuestPDF.Drawing.FontManager.RegisterFont(File.OpenRead(latoBold));
                }
            }
            catch
            {
                // Jeśli nie uda się załadować czcionek, użyj domyślnych
                // QuestPDF automatycznie użyje czcionek systemowych
            }
        }

        public PrintService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<byte[]> GenerateAgendaPdfAsync(int agendaId, int agendaIndex)
        {
            var agenda = await GetAgendaWithDetailsAsync(agendaId);
            if (agenda == null)
                throw new ArgumentException($"Agenda with ID {agendaId} not found.");

            return GenerateAgendaDocument(agenda, agendaIndex);
        }

        /// <inheritdoc />
        public async Task<byte[]> GenerateDayPdfAsync(int dayId)
        {
            var day = await _uow.Day.GetAsync(
                filter: d => d.Id == dayId,
                includeProperties: "Agendas"
            );

            if (day == null)
                throw new ArgumentException($"Day with ID {dayId} not found.");

            var agendas = await _uow.Agenda.GetAllAsync(
                filter: a => a.DayId == dayId,
                includeProperties: "Day,AssignedMembers,Visits.Submission.Address"
            );

            if (!agendas.Any())
                throw new InvalidOperationException("No agendas found for the specified day.");

            // Sortuj agendy według Id (kolejność tworzenia)
            var sortedAgendas = agendas.OrderBy(a => a.Id).ToList();

            return GenerateDayDocument(sortedAgendas);
        }

        private async Task<Agenda?> GetAgendaWithDetailsAsync(int agendaId)
        {
            return await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                includeProperties: "Day,AssignedMembers,Visits.Submission.Address"
            );
        }

        private byte[] GenerateAgendaDocument(Agenda agenda, int agendaIndex)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    
                    page.Content().MultiColumn(multiColumn =>
                    {
                        multiColumn.Spacing(1, Unit.Centimetre);
                        multiColumn.Columns(2);
                        
                        multiColumn.Content().Column(column =>
                        {
                            RenderAgendaContent(column, agenda, agendaIndex);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateDayDocument(List<Agenda> agendas)
        {
            var document = Document.Create(container =>
            {
                for (int i = 0; i < agendas.Count; i++)
                {
                    var agenda = agendas[i];
                    var agendaIndex = i + 1;

                    container.Page(page =>
                    {
                        ConfigurePage(page);
                        page.Content().MultiColumn(multiColumn =>
                        {
                            multiColumn.Spacing(1, Unit.Centimetre);
                            multiColumn.Columns(2);

                            multiColumn.Content().Column(column =>
                            {
                                RenderAgendaContent(column, agenda, agendaIndex);
                            });
                        });
                    });
                }
            });

            return document.GeneratePdf();
        }

        private void ConfigurePage(PageDescriptor page)
        {
            // A4 poziomo (landscape)
            page.Size(PageSizes.A4.Landscape());
            page.Margin(0.75f, Unit.Centimetre);
            page.PageColor(Colors.White);
            
            // Domyślna czcionka dla całego dokumentu
            page.DefaultTextStyle(x => x.FontFamily("Lato"));
        }

        private void RenderAgendaContent(ColumnDescriptor column, Agenda agenda, int agendaIndex)
        {
            var date = agenda.Day.Date;
            var polishCulture = new CultureInfo("pl-PL");
            var generatedDateTime = DateTime.Now;
            var visitsCount = agenda.Visits.Count(v => v.OrdinalNumber.HasValue);

            // Data i czas wygenerowania w lewym górnym rogu
            column.Item().AlignLeft().Text($"Wygenerowano: {generatedDateTime:dd.MM.yyyy HH:mm}")
                .FontSize(8)
                .FontColor(Colors.Grey.Darken1);

            column.Item().PaddingVertical(0.2f, Unit.Centimetre);

            // Przedtytuł
            column.Item().AlignCenter().Text("Plan wizyty duszpasterskiej")
                .FontSize(10)
                .FontColor(Colors.Black);

            // Główny tytuł - data
            column.Item().AlignCenter().Text(date.ToString("d MMMM yyyy", polishCulture))
                .FontSize(18)
                .Bold()
                .FontColor(Colors.Black);

            // Dzień tygodnia
            column.Item().AlignCenter().Text(GetPolishDayName(date.DayOfWeek))
                .FontSize(10)
                .FontColor(Colors.Black);

            column.Item().PaddingVertical(0.3f, Unit.Centimetre);

            // Podtytuł - numer agendy i liczba zgłoszeń
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Ksiądz {agendaIndex}")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Black);

                row.RelativeItem().AlignRight().Text($"Zgłoszenia: {visitsCount}")
                    .FontSize(13)
                    .Bold()
                    .FontColor(Colors.Black);
            });

            // Imię księdza (jeśli przypisany)
            var priest = agenda.AssignedMembers.FirstOrDefault();
            if (priest != null && !string.IsNullOrEmpty(priest.DisplayName))
            {
                column.Item().Text(priest.DisplayName)
                    .FontSize(11)
                    .FontColor(Colors.Black);
            }

            column.Item().PaddingVertical(0.4f, Unit.Centimetre);

            // Tabelka z wizytami
            RenderVisitsTable(column, agenda);

            // Notatki pod tabelką
            RenderNotes(column, agenda);
        }

        private void RenderVisitsTable(ColumnDescriptor column, Agenda agenda)
        {
            // Pobierz wizyty posortowane według OrdinalNumber
            var visits = agenda.Visits
                .Where(v => v.OrdinalNumber.HasValue)
                .OrderBy(v => v.OrdinalNumber!.Value)
                .ToList();

            if (!visits.Any())
            {
                column.Item().Text("Brak wizyt w agendzie")
                    .FontSize(10)
                    .Italic()
                    .FontColor(Colors.Grey.Darken2);
                return;
            }

            // Grupuj wizyty według ulicy, a następnie według bramy
            var streetGroups = visits
                .GroupBy(v => v.Submission.Address.StreetName ?? "Nieznana ulica")
                .Select(streetGroup => new
                {
                    StreetName = streetGroup.Key,
                    Buildings = streetGroup
                        .GroupBy(v => new
                        {
                            BuildingNumber = v.Submission.Address.BuildingNumber,
                            BuildingLetter = v.Submission.Address.BuildingLetter
                        })
                        .Select(buildingGroup => new
                        {
                            BuildingNumber = buildingGroup.Key.BuildingNumber,
                            BuildingLetter = buildingGroup.Key.BuildingLetter,
                            Visits = buildingGroup.ToList()
                        })
                        .ToList()
                })
                .ToList();

            column.Item().Table(table =>
            {
                // Definicja kolumn: Ulica (30%), Brama (20%), Mieszkania (50%)
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);  // Ulica
                    columns.RelativeColumn(2);  // Brama
                    columns.RelativeColumn(5);  // Mieszkania
                });

                // Policz całkowitą liczbę wierszy
                int totalRows = streetGroups.Sum(sg => sg.Buildings.Count);
                int currentRow = 0;

                // Renderuj wizyty pogrupowane
                foreach (var streetGroup in streetGroups)
                {
                    var buildingsCount = streetGroup.Buildings.Count;
                    bool isFirstBuilding = true;

                    foreach (var building in streetGroup.Buildings)
                    {
                        bool isFirstRow = currentRow == 0;
                        bool isLastRow = currentRow == totalRows - 1;

                        // Komórka z nazwą ulicy (rozciągnięta na wszystkie bramy dla tej ulicy)
                        if (isFirstBuilding)
                        {
                            // Sprawdź czy ostatni wiersz tego rowspana to ostatni wiersz tabeli
                            bool isLastRowOfSpan = (currentRow + buildingsCount - 1) == (totalRows - 1);
                            
                            table.Cell().RowSpan((uint)buildingsCount)
                                .BorderLeft(0)
                                .BorderRight(0.5f)
                                .BorderTop(isFirstRow ? 0 : 0.5f)
                                .BorderBottom(isLastRowOfSpan ? 0 : 0.5f)
                                .BorderColor(Colors.Grey.Medium)
                                .Padding(5)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(streetGroup.StreetName)
                                .FontSize(11)
                                .Bold()
                                .FontColor(Colors.Black);

                            isFirstBuilding = false;
                        }

                        // Brama
                        var buildingNumber = FormatBuildingNumber(building.BuildingNumber, building.BuildingLetter);
                        table.Cell()
                            .BorderLeft(0.5f)
                            .BorderRight(0.5f)
                            .BorderTop(isFirstRow ? 0 : 0.5f)
                            .BorderBottom(isLastRow ? 0 : 0.5f)
                            .BorderColor(Colors.Grey.Medium)
                            .Padding(5)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(buildingNumber)
                            .FontSize(10)
                            .Bold()
                            .FontColor(Colors.Black);

                        // Mieszkania (łączone przecinkami)
                        table.Cell()
                            .BorderLeft(0.5f)
                            .BorderRight(0)
                            .BorderTop(isFirstRow ? 0 : 0.5f)
                            .BorderBottom(isLastRow ? 0 : 0.5f)
                            .BorderColor(Colors.Grey.Medium)
                            .Padding(5)
                            .AlignLeft()
                            .AlignMiddle()
                            .Text(text =>
                            {
                                text.DefaultTextStyle(TextStyle.Default.FontSize(10).FontColor(Colors.Black));
                                
                                for (int i = 0; i < building.Visits.Count; i++)
                                {
                                    var visit = building.Visits[i];
                                    var apartmentNumber = FormatApartmentNumber(
                                        visit.Submission.Address.ApartmentNumber,
                                        visit.Submission.Address.ApartmentLetter
                                    );
                                    var hasNotes = !string.IsNullOrWhiteSpace(visit.Submission.AdminNotes);

                                    if (hasNotes)
                                    {
                                        text.Span(apartmentNumber).Bold();
                                        text.Span("*").Bold();
                                    }
                                    else
                                    {
                                        text.Span(apartmentNumber);
                                    }

                                    if (i < building.Visits.Count - 1)
                                    {
                                        text.Span(", ");
                                    }
                                }
                            });

                        currentRow++;
                    }
                }
            });
        }

        private void RenderNotes(ColumnDescriptor column, Agenda agenda)
        {
            // Zbierz wszystkie wizyty z notatkami
            var visitsWithNotes = agenda.Visits
                .Where(v => !string.IsNullOrWhiteSpace(v.Submission.AdminNotes))
                .OrderBy(v => v.OrdinalNumber ?? int.MaxValue)
                .ToList();

            if (!visitsWithNotes.Any())
                return;

            column.Item().PaddingTop(0.5f, Unit.Centimetre);

            foreach (var visit in visitsWithNotes)
            {
                var address = visit.Submission.Address;
                var streetName = address.StreetName ?? "Nieznana";
                var buildingNumber = FormatBuildingNumber(address.BuildingNumber, address.BuildingLetter);
                var apartmentNumber = FormatApartmentNumber(address.ApartmentNumber, address.ApartmentLetter);

                column.Item().PaddingBottom(0.15f, Unit.Centimetre).AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(TextStyle.Default.FontSize(9).FontColor(Colors.Black));
                    
                    // Część adresowa (pogrubiona)
                    text.Span($"{streetName} {buildingNumber} m. {apartmentNumber}: ").Bold();
                    
                    // Notatka
                    text.Span(visit.Submission.AdminNotes);
                });
            }
        }

        private string FormatBuildingNumber(int? number, string? letter)
        {
            if (number == null) return "-";
            return letter != null ? $"{number}{letter}" : $"{number}";
        }

        private string FormatApartmentNumber(int? number, string? letter)
        {
            if (number == null) return "-";
            return letter != null ? $"{number}{letter}" : $"{number}";
        }

        private string GetPolishDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "poniedziałek",
                DayOfWeek.Tuesday => "wtorek",
                DayOfWeek.Wednesday => "środa",
                DayOfWeek.Thursday => "czwartek",
                DayOfWeek.Friday => "piątek",
                DayOfWeek.Saturday => "sobota",
                DayOfWeek.Sunday => "niedziela",
                _ => dayOfWeek.ToString()
            };
        }
    }
}
