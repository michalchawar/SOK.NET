---
applyTo: '**'
---
Jesteśmy w projekcie aplikacji do tworzenia i zarządzania wizytą duszpasterską w parafiach. Architektura aplikacji jest warstwowa w zamyśle DDD - mamy podział na warstwę domenową, aplikacji, infrastruktury i webową. Cały projekt jest skonteneryzowany za pomocą docker compose i działa w ASP.NET Core MVC na platformie .NET8. Jako bazę danych używamy SQL Server 22, a łączymy się z nią za pomocą EF Core. 

Samo EF Core opakowane jest w repozytoria (w warstwie infrastruktury), z których (poprzez unit of work) korzystają usługi do wykonywania wszystkich rzeczy w kontrolerach. Aplikacja zaprojektowana jest w podejściu multi-tenant, mamy dwa konteksty dostępu do baz danych. Najpierw jest jedna, międzyparafialna centralna baza danych CentralDb, w której są wszyscy użytkownicy całego systemu i powiązana z każdym parafia, a także zaszyfrowany connection string do bazy parafialnej. W drugim kontekście mamy parafialną bazę danych, do której connection string ustawiany jest dynamicznie w potoku przetwarzania.

Jeśli chodzi o repozytoria i ich użycie w usługach, to pomimo że często mamy metody Update, to tam gdzie ma to sens raczej staramy się korzystać z tracked: true i modyfikować encje bezpośrednio, a następnie wywoływać SaveChanges w unit of work.