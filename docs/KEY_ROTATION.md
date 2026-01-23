# 🔐 Rotacja Kluczy Szyfrowania

## Przegląd

System SOK.NET obsługuje rotację kluczy szyfrowania dla zaszyfrowanych connection stringów parafii. Mechanizm umożliwia:
- **Wersjonowanie kluczy** - każdy klucz ma swój numer wersji
- **Automatyczną detekcję** - odszyfrowanie automatycznie wykrywa użyty klucz
- **Stopniową migrację** - dane można migrować do nowego klucza bez przestoju
- **Wiele aktywnych kluczy** - stare klucze działają do czasu migracji wszystkich danych

## Konfiguracja Kluczy

### Pojedynczy Klucz (Stara Konfiguracja - Deprecated)

```json
{
  "Crypto": {
    "Key": "MojeHaslo123"
  }
}
```

### Wiele Kluczy (Zalecane)

```json
{
  "Crypto": {
    "Keys": {
      "1": "StaryKluczKtoryJeszczeJestPotrzebny",
      "2": "NowyKluczDoKtoregoMigrujemy",
      "3": "NajnowszyKluczDlaNowychtDanych"
    }
  }
}
```

**Uwaga:** Aktualnym kluczem do szyfrowania nowych danych jest ten o **najwyższym numerze** wersji.

## Proces Rotacji Kluczy

### Krok 1: Dodaj Nowy Klucz

Zaktualizuj `appsettings.json` dodając nowy klucz:

```json
{
  "Crypto": {
    "Keys": {
      "1": "StaryKlucz",
      "2": "NowyKlucz"  // ← Dodaj tutaj
    }
  }
}
```

### Krok 2: Sprawdź Aktualny Stan

Wyświetl raport z wersjami kluczy używanych przez parafie:

```bash
dotnet run --project SOK.Web -- key-report
```

Przykładowy output:
```
=== Key Versions Report ===
Current system key version: v2

Key Version v1: 15 parishes
  - Parafia Św. Jana (UID: 123e4567-e89b-12d3-a456-426614174000)
  - Parafia Św. Marii (UID: 987fcdeb-51a2-43b7-9012-345678901234)
  ...

Total parishes: 15
```

### Krok 3: Test Migracji (Dry Run)

Przed faktyczną migracją, wykonaj test:

```bash
dotnet run --project SOK.Web -- rotate-keys --dry-run
```

### Krok 4: Wykonaj Migrację

Migruj wszystkie connection stringi do najnowszego klucza:

```bash
dotnet run --project SOK.Web -- rotate-keys
```

Lub do konkretnej wersji:

```bash
dotnet run --project SOK.Web -- rotate-keys --version 2
```

### Krok 5: Weryfikacja

Ponownie sprawdź raport:

```bash
dotnet run --project SOK.Web -- key-report
```

Wszystkie parafie powinny być na nowej wersji.

### Krok 6: Usuń Stare Klucze (Opcjonalnie)

Po pomyślnej migracji możesz usunąć stare klucze z konfiguracji:

```json
{
  "Crypto": {
    "Keys": {
      "2": "NowyKlucz"  // Klucz v1 został usunięty
    }
  }
}
```

**⚠️ UWAGA:** Usuń stary klucz dopiero po **potwierdzeniu**, że wszystkie dane zostały zmigrowane!

## Komendy CLI

### `rotate-keys`

Re-enkryptuje connection stringi wszystkich parafii do nowego klucza.

**Opcje:**
- `--version N` - Docelowa wersja klucza (domyślnie: najnowsza)
- `--dry-run` - Symulacja bez zapisu do bazy

**Przykłady:**
```bash
# Migruj do najnowszego klucza
dotnet run --project SOK.Web -- rotate-keys

# Migruj do konkretnej wersji
dotnet run --project SOK.Web -- rotate-keys --version 2

# Test bez zmian w bazie
dotnet run --project SOK.Web -- rotate-keys --dry-run
```

### `key-report`

Wyświetla raport użycia wersji kluczy.

**Przykład:**
```bash
dotnet run --project SOK.Web -- key-report
```

## Bezpieczeństwo

### Najlepsze Praktyki

1. **Silne hasła:** Używaj długich, losowych haseł (min. 32 znaki)
   ```bash
   # Generowanie bezpiecznego hasła
   openssl rand -base64 32
   ```

2. **Zmienne środowiskowe:** Nie commituj kluczy do repozytorium
   ```json
   {
     "Crypto": {
       "Keys": {
         "1": "${CRYPTO_KEY_V1}",
         "2": "${CRYPTO_KEY_V2}"
       }
     }
   }
   ```

3. **Regularna rotacja:** Rotuj klucze co 6-12 miesięcy

4. **Backup:** Przed rotacją wykonaj backup bazy danych

### Bezpieczne Przechowywanie

W środowisku produkcyjnym zalecane jest użycie:
- **Azure Key Vault**
- **AWS Secrets Manager**
- **HashiCorp Vault**

## Architektura Techniczna

### Format Zaszyfrowanych Danych

```
v{KeyVersion}:{Base64EncryptedData}
```

Przykład:
```
v2:aBcDeFgHiJkLmNoPqRsTuVwXyZ0123456789+/==
```

### Detekcja Wersji

System automatycznie wykrywa wersję klucza podczas odszyfrowywania:
- Dane z prefiksem `vN:` → używa klucza wersji N
- Dane bez prefiksu → używa klucza wersji 1 (backward compatibility)

### Generowanie Kluczy

Klucze są generowane z haseł przy użyciu **PBKDF2**:
- **Algorytm:** PBKDF2-HMAC-SHA256
- **Iteracje:** 100,000
- **Salt:** `SOK.NET-Parish-Crypto-Salt-v1`
- **Długość klucza:** 256 bitów (32 bajty)

## Rozwiązywanie Problemów

### Błąd: "Decryption key version X not found"

**Przyczyna:** Próba odszyfrowania danych kluczem, którego nie ma w konfiguracji.

**Rozwiązanie:** Dodaj brakujący klucz do `appsettings.json`:
```json
{
  "Crypto": {
    "Keys": {
      "1": "StaryKlucz",  // ← Dodaj brakujący klucz
      "2": "NowyKlucz"
    }
  }
}
```

### Błąd: "No encryption keys configured"

**Przyczyna:** Brak konfiguracji kluczy.

**Rozwiązanie:** Dodaj sekcję `Crypto:Keys` do `appsettings.json`.

### Dane nie odszyfrowują się poprawnie

**Możliwe przyczyny:**
1. Zmieniono hasło klucza w konfiguracji
2. Zmieniono salt (stały w kodzie)
3. Uszkodzone dane w bazie

**Rozwiązanie:** Przywróć oryginalne hasło klucza lub przywróć dane z backupu.

## Migracja ze Starego Systemu

Jeśli masz dane zaszyfrowane starym systemem (bez prefiksów wersji):

1. Skonfiguruj klucz v1 z aktualnym hasłem:
   ```json
   {
     "Crypto": {
       "Keys": {
         "1": "TwojAktualnyKlucz"
       }
     }
   }
   ```

2. Dane bez prefiksu będą automatycznie odczytane jako v1

3. Wykonaj rotację do v2:
   ```bash
   dotnet run --project SOK.Web -- rotate-keys --version 2
   ```

## Pytania i Odpowiedzi

**Q: Czy mogę usunąć stary klucz od razu po dodaniu nowego?**  
A: Nie! Musisz najpierw zmigrować wszystkie dane używając `rotate-keys`.

**Q: Co się stanie jeśli zapomnę hasła starego klucza?**  
A: Nie będziesz w stanie odszyfrować danych zaszyfrowanych tym kluczem. Dlatego ważny jest backup!

**Q: Czy rotacja wymaga restartu aplikacji?**  
A: Nie do odczytu starych danych. Tak, aby używać nowego klucza do szyfrowania.

**Q: Ile kluczy mogę mieć jednocześnie?**  
A: Teoretycznie nieograniczoną ilość, ale zalecamy max 2-3 (stary + nowy + opcjonalnie awaryjny).
