# Przykład konfiguracji dla Docker Compose

## Prosty klucz (zalecane dla nowych instalacji)

```yaml
services:
  web:
    environment:
      - Crypto__Keys__1=MojeTajneHasloDoParafii2026!
```

## Wiele kluczy (podczas rotacji)

```yaml
services:
  web:
    environment:
      - Crypto__Keys__1=StaryKlucz
      - Crypto__Keys__2=NowyKlucz
```

## Bezpieczne przechowywanie w .env

**.env:**
```env
CRYPTO_KEY_V1=StaryKlucz
CRYPTO_KEY_V2=NowyKlucz
```

**docker-compose.yml:**
```yaml
services:
  web:
    environment:
      - Crypto__Keys__1=${CRYPTO_KEY_V1}
      - Crypto__Keys__2=${CRYPTO_KEY_V2}
```

## Produkcja z Docker Secrets

**docker-compose.prod.yml:**
```yaml
services:
  web:
    secrets:
      - crypto_key_v1
      - crypto_key_v2
    environment:
      - Crypto__Keys__1=/run/secrets/crypto_key_v1
      - Crypto__Keys__2=/run/secrets/crypto_key_v2

secrets:
  crypto_key_v1:
    external: true
  crypto_key_v2:
    external: true
```

**Tworzenie secret:**
```bash
echo "MojeTajneHaslo" | docker secret create crypto_key_v1 -
```
