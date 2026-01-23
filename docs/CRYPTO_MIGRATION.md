# Migracja na Nowy System Kryptograficzny

## Czym się różni od starego systemu?

### Stary System ❌
```json
{
  "Crypto": {
    "Key": "J0BQomj4ichywU/Lm2FlG4CMzVqHclmxydZOz2KQBDI="
  }
}
```
- Wymagał klucza w Base64 (44 znaki)
- Musiałeś generować dokładnie 32 bajty
- Błąd długości = crash aplikacji
- Brak możliwości rotacji kluczy

### Nowy System ✅
```json
{
  "Crypto": {
    "Keys": {
      "1": "MojeProsteHaslo123"
    }
  }
}
```
- **Dowolne hasło tekstowe** - nie musisz używać Base64
- Automatyczne generowanie klucza 256-bit z PBKDF2
- **Rotacja kluczy** - możliwość zmiany bez utraty danych
- **Backward compatibility** - stare dane nadal działają

## Dla Nowych Instalacji

Po prostu ustaw proste hasło w `.env`:

```env
CRYPTO_KEY=MojeTajneHasloDoParafii2026!
```

I gotowe! 🎉

## Dla Istniejących Instalacji

### Opcja 1: Kontynuuj ze Starym Kluczem (Zalecane)

Jeśli masz działający system ze starym kluczem w Base64, **nie musisz nic zmieniać**. System automatycznie:
- Wykryje starą konfigurację `Crypto:Key`
- Potraktuje ją jako klucz wersji 1
- Wszystko będzie działać jak dotychczas

**Nowa konfiguracja w `appsettings.json`:**
```json
{
  "Crypto": {
    "Keys": {
      "1": "J0BQomj4ichywU/Lm2FlG4CMzVqHclmxydZOz2KQBDI="
    }
  }
}
```

Zamień `Crypto:Key` na `Crypto:Keys:1` i używaj swojego starego klucza Base64.

### Opcja 2: Migracja do Nowego Klucza

Jeśli chcesz przejść na prosty klucz tekstowy:

**Krok 1:** Dodaj nowy klucz obok starego
```json
{
  "Crypto": {
    "Keys": {
      "1": "J0BQomj4ichywU/Lm2FlG4CMzVqHclmxydZOz2KQBDI=",
      "2": "MojeNoweProsteHaslo2026!"
    }
  }
}
```

**Krok 2:** Uruchom migrację
```bash
dotnet run --project SOK.Web -- rotate-keys --version 2
```

**Krok 3:** Po pomyślnej migracji usuń stary klucz
```json
{
  "Crypto": {
    "Keys": {
      "2": "MojeNoweProsteHaslo2026!"
    }
  }
}
```

## Przykłady Użycia

### Wygeneruj Silne Hasło

```bash
# Linux/Mac
openssl rand -base64 32

# PowerShell
-join ((33..126) | Get-Random -Count 32 | ForEach-Object {[char]$_})

# Lub po prostu wymyśl długie hasło:
ParafiaSwietegoJana2026!SecurePassword
```

### Sprawdź Status Kluczy

```bash
dotnet run --project SOK.Web -- key-report
```

### Re-enkrypcja (Dry Run)

```bash
dotnet run --project SOK.Web -- rotate-keys --dry-run
```

## FAQ

**Q: Czy muszę migrować natychmiast?**  
A: Nie, stare klucze działają bez zmian.

**Q: Czy mogę użyć polskich znaków w haśle?**  
A: Tak, ale zalecamy angielskie znaki dla kompatybilności.

**Q: Co jeśli zapomnę starego klucza?**  
A: Nie będziesz mógł odszyfrować danych. **Zachowaj backup!**

**Q: Jak często rotować klucze?**  
A: Zalecamy co 6-12 miesięcy lub po podejrzeniu kompromitacji.

## Wsparcie

Szczegółowa dokumentacja: [docs/KEY_ROTATION.md](docs/KEY_ROTATION.md)

Problemy? Otwórz issue na GitHub.
