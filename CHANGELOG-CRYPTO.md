# Changelog - System Rotacji Kluczy Kryptograficznych

## [2.0.0] - 2026-01-23

### 🔐 Nowy System Kryptograficzny

#### ✨ Dodane
- **Uproszczone klucze**: Możliwość używania zwykłych haseł tekstowych zamiast kluczy Base64
- **PBKDF2**: Automatyczne generowanie kluczy 256-bit z dowolnego hasła
- **Rotacja kluczy**: Pełne wsparcie dla wersjonowania i rotacji kluczy szyfrowania
- **Wersjonowanie**: Każdy zaszyfrowany ciąg ma prefix wersji (`v1:`, `v2:`, etc.)
- **CLI Commands**: Nowe komendy do zarządzania rotacją kluczy
  - `rotate-keys` - Re-enkrypcja connection stringów do nowej wersji klucza
  - `key-report` - Raport użycia wersji kluczy
- **Backward Compatibility**: Stare dane bez prefiksu wersji automatycznie traktowane jako v1
- **Pole KeyVersion**: Nowa kolumna w tabeli `Parishes` do śledzenia wersji kluczy

#### 📝 Zmiany w Interfejsach
- `ICryptoService.Encrypt()` - Zwraca dane z prefiksem wersji
- `ICryptoService.Encrypt(string, int)` - Nowe przeciążenie z explicite określoną wersją
- `ICryptoService.GetCurrentKeyVersion()` - Pobiera aktualną wersję klucza
- `ICryptoService.Reencrypt(string, int)` - Migracja danych między wersjami kluczy

#### 🗃️ Baza Danych
- **Migration**: `AddKeyVersionToParishEntry` - Dodaje kolumnę `KeyVersion INT NOT NULL DEFAULT 1` do tabeli `Parishes`

#### 📚 Dokumentacja
- `docs/KEY_ROTATION.md` - Pełna dokumentacja rotacji kluczy
- `docs/CRYPTO_MIGRATION.md` - Przewodnik migracji ze starego systemu
- `docs/DOCKER_CRYPTO_CONFIG.md` - Przykłady konfiguracji dla Docker
- Zaktualizowano `README.md` z sekcją o kryptografii

#### 🧪 Testy
- Dodano 5 nowych testów dla rotacji kluczy w `CryptoServiceTests`
- Zaktualizowano istniejące testy do nowego formatu
- Wszystkie 100 testów przechodzą ✅

#### ⚙️ Konfiguracja
- **Nowy format** (zalecany):
  ```json
  {
    "Crypto": {
      "Keys": {
        "1": "ProsteHaslo123",
        "2": "NoweHaslo456"
      }
    }
  }
  ```
- **Stary format** (nadal wspierany):
  ```json
  {
    "Crypto": {
      "Key": "Base64EncodedKey=="
    }
  }
  ```

### 🔧 Poprawki
- Usunięto wymaganie dokładnej długości klucza (32 bajty)
- Poprawiono obsługę błędów podczas odszyfrowywania z brakującym kluczem
- Lepsze komunikaty błędów dla problemów z konfiguracją kluczy

### 🚀 Ulepszone Bezpieczeństwo
- **100,000 iteracji PBKDF2** - Ochrona przed atakami brute-force
- **Salt**: Stały salt `SOK.NET-Parish-Crypto-Salt-v1` dla deterministycznych kluczy
- **SHA-256**: Użycie silnego algorytmu haszującego w PBKDF2

### ⚠️ Breaking Changes
**Brak!** System jest w pełni kompatybilny wstecz:
- Stare klucze Base64 nadal działają
- Stare zaszyfrowane dane odczytywane automatycznie jako v1
- Nie wymaga żadnych zmian w istniejących instalacjach

### 📦 Zależności
Brak nowych zależności - używamy wbudowanych funkcji .NET:
- `System.Security.Cryptography.Rfc2898DeriveBytes`
- `System.Security.Cryptography.Aes`

### 🎯 Use Cases
1. **Nowa instalacja**: Użyj prostego hasła w `.env`
2. **Regularna rotacja**: Co 6-12 miesięcy zmień klucz
3. **Kompromitacja klucza**: Natychmiastowa migracja do nowego klucza
4. **Multi-environment**: Różne klucze dla dev/staging/prod

### 🔜 Planowane Funkcje
- [ ] Automatyczna rotacja w określonych odstępach czasu
- [ ] Integracja z Azure Key Vault / AWS Secrets Manager
- [ ] Powiadomienia o zbliżającym się "wieku" klucza
- [ ] Audit log dla operacji kryptograficznych

---

## Instrukcje Migracji

### Dla Nowych Użytkowników
```env
CRYPTO_KEY=MojeTajneHaslo2026!
```

### Dla Istniejących Użytkowników
Zobacz [docs/CRYPTO_MIGRATION.md](docs/CRYPTO_MIGRATION.md)
