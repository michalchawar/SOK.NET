# SOK.Application.Tests

Projekt testów jednostkowych dla aplikacji SOK.NET (System Organizacji Kolędy).

## Technologie

- **xUnit** - framework do testowania
- **Moq** - biblioteka do tworzenia mocków
- **FluentAssertions** - biblioteka do czytelnych asercji

## Struktura testów

### Services/ - Testy serwisów aplikacji
- **VisitServiceTests.cs** - testy serwisu zarządzania wizytami (4 testy)
  - Przypisywanie wizyt do dni
  - Walidacja danych wejściowych
  - Obsługa błędów (nieistniejące wizyty/dni)
  - Flagowanie wysyłania emaili
  
- **SubmissionServiceTests.cs** - testy serwisu zgłoszeń (19 testów)
  - Pobieranie zgłoszeń po ID i UniqueId
  - Walidacja formatu GUID
  - **Tworzenie zgłoszeń (CreateSubmissionAsync):**
    - Walidacja istnienia budynku i harmonogramu
    - Obsługa istniejących vs nowych zgłaszających
    - Automatyczne przypisywanie wizyt (auto-assignment)
    - Wysyłanie emaili potwierdzających
    - Formatowanie danych zgłaszającego
    - Tworzenie powiązanych obiektów (Visit, FormSubmission)
    - Obsługa uwag użytkownika (NotesStatus)
    - **Duplikacja adresów:**
      - Ten sam adres w tym samym planie → InvalidOperationException
      - Ten sam adres w różnych planach → dozwolone
  
- **CryptoServiceTests.cs** - testy serwisu kryptografii (7 testów)
  - Szyfrowanie/deszyfrowanie tekstów
  - Obsługa polskich znaków
  - Różne długości tekstu
  - Losowość IV (każde szyfrowanie daje inny wynik)

- **ParishInfoServiceTests.cs** - testy serwisu konfiguracji parafii (6 testów)
  - Pobieranie wartości opcji konfiguracyjnych
  - Aktualizacja istniejących opcji
  - Tworzenie nowych opcji
  - Pobieranie słownika wszystkich opcji
  - Pobieranie wybranych opcji

- **PlanServiceTests.cs** - testy serwisu planów (17 testów)
  - Pobieranie planu po ID
  - Tworzenie, aktualizacja i usuwanie planów
  - Ustawianie i pobieranie aktywnego planu
  - Czyszczenie aktywnego planu
  - Walidacja paginacji (page size, page number)
  - Zarządzanie zbieraniem zgłoszeń (toggle, status)

- **ParishMemberServiceTests.cs** - testy serwisu członków parafii (11 testów)
  - Pobieranie członka po ID
  - Pobieranie członka z ClaimsPrincipal
  - Aktualizacja członków
  - Pobieranie wszystkich w danej roli
  - Pobieranie wszystkich z filtrami (agendas, plans, submissions)
  - Walidacja paginacji
  - Pobieranie wybranych opcji

- **PlanServiceTests.cs** - testy serwisu planów (17 testów)
  - Pobieranie planu po ID
  - Tworzenie, aktualizacja i usuwanie planów
  - Ustawianie i pobieranie aktywnego planu
  - Czyszczenie aktywnego planu
  - Walidacja paginacji (page size, page number)
  - Zarządzanie zbieraniem zgłoszeń (toggle, status)

- **ParishMemberServiceTests.cs** - testy serwisu członków parafii (11 testów)
  - Pobieranie członka po ID
  - Pobieranie członka z ClaimsPrincipal
  - Aktualizacja członków
  - Pobieranie wszystkich w danej roli
  - Pobieranie wszystkich z filtrami (agendas, plans, submissions)
  - Walidacja paginacji

### Domain/ - Testy encji domenowych
- **VisitEntityTests.cs** - testy encji Visit (6 testów)
  - Domyślne wartości (status = Unplanned)
  - Walidacja OrdinalNumber (zakres 1-300)
  - Wszystkie możliwe statusy wizyty
  
- **SubmissionEntityTests.cs** - testy encji Submission (9 testów)
  - Generowanie UniqueId
  - Walidacja długości pól (SubmitterNotes, AdminMessage, etc.)
  - Statusy realizacji uwag
  
- **AddressEntityTests.cs** - testy encji Address (6 testów)
  - Walidacja numeru apartamentu (zakres 1-300)
  - Walidacja litery apartamentu (max 3 znaki)

## Uruchamianie testów

```bash
# Wszystkie testy
dotnet test

# Z pełnym logowaniem
dotnet test --verbosity normal

# Tylko projekt testowy
dotnet test .\SOK.Application.Tests\SOK.Application.Tests.csproj

# Tylko testy CreateSubmissionAsync
dotnet test --filter "FullyQualifiedName~CreateSubmissionAsync"
```

## Statystyki pokrycia

Aktualnie projekt zawiera **60 testów jednostkowych** pokrywających:
- **3 serwisy** z warstwy aplikacji (28 testów)
  - VisitService - 4 testy
  - SubmissionService - 17 testów (w tym 13 dla CreateSubmissionAsync)
  - CryptoService - 7 testów
- **3 encje** z warstwy domenowej (21 testów)
  - Visit - 6 testów
  - Submission - 9 testów
  - Address - 6 testów

## Najważniejsze scenariusze testowe

### CreateSubmissionAsync - kompleksowe testowanie tworzenia zgłoszeń

1. **Walidacja danych wejściowych**:
   - Budynek musi istnieć w bazie
   - Harmonogram musi istnieć w bazie
   
2. **Obsługa zgłaszającego (Submitter)**:
   - Sprawdzanie czy już istnieje w bazie
   - Automatyczne formatowanie danych (trim, capitalize)
   
3. **Auto-przypisywanie wizyt**:
   - Sprawdzanie BuildingAssignment
   - Automatyczne wywołanie AssignVisitToDay
   - Respektowanie flagi DisableAutoAssignment
   
4. **Wysyłanie emaili**:
   - Sprawdzanie globalnych ustawień
   - Walidacja adresu email
   - Respektowanie flagi SendConfirmationEmail

5. **Tworzenie powiązanych obiektów**:
   - Visit z statusem Unplanned
   - FormSubmission z danymi archiwalnymi
   - Address (nowy lub istniejący)

## Wzorce testowania

### Arrange-Act-Assert (AAA)
Wszystkie testy używają wzorca AAA dla czytelności:

```csharp
[Fact]
public void Example_Test()
{
    // Arrange - przygotowanie danych testowych
    var service = new MyService();
    
    // Act - wykonanie testowanej akcji
    var result = service.DoSomething();
    
    // Assert - weryfikacja wyniku
    result.Should().Be(expected);
}
```

### Mockowanie zależności
Używamy Moq do mockowania repozytoriów i innych serwisów:

```csharp
var mockRepo = new Mock<IRepository>();
mockRepo.Setup(r => r.GetAsync(...)).ReturnsAsync(expectedData);
```

### Helper Methods
Dla złożonych testów (np. CreateSubmissionAsync) używamy metod pomocniczych:

```csharp
private void SetupValidBuilding() { ... }
private void SetupValidSchedule() { ... }
private SubmissionCreationRequestDto CreateValidSubmissionDto() { ... }
```

### FluentAssertions
Czytelne asercje:

```csharp
result.Should().NotBeNull();
result.Id.Should().Be(1);
act.Should().ThrowAsync<ArgumentException>();
capturedSubmission.NotesStatus.Should().Be(NotesFulfillmentStatus.Pending);
```

## Uwagi

Testy **nie** wymagają bazy danych - wszystkie zależności są mockowane. Skupiają się na:
- Logice biznesowej w serwisach
- Walidacji w encjach domenowych
- Obsłudze błędów
- Interakcjach między komponentami (poprzez weryfikację wywołań mocków)

Dla testów integracyjnych (z bazą danych) należy utworzyć osobny projekt testowy.

