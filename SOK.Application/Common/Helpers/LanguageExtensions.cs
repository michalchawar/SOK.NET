using System.Collections;

namespace SOK.Application.Common.Helpers
{
    /// <summary>
    /// Klasa do rozszerzania funkcjonalności związanych z językiem programowania.
    /// </summary>
    public static class LanguageExtensions
    {
        /// <summary>
        /// Przemienia zwykłe <see cref="IEnumerable"/> na <see cref="IEnumerable"/> z indeksem.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="IEnumerable"/> z parami <T, int>.
        /// </returns>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));

        /// <summary>
        /// Pobiera wartość pod kluczem <paramref name="key"/> ze słownika, a jeśli nie istnieje zwraca <paramref name="defaultValue"/>.
        /// </summary>
        /// <typeparam name="TKey">Typ kluczy słownika.</typeparam>
        /// <typeparam name="TValue">Typ wartości słownika.</typeparam>
        /// <param name="dictionary">Słownik.</param>
        /// <param name="key">Klucz, którego wartość należy pobrać.</param>
        /// <param name="defaultValue">Domyślna wartość zwracana.</param>
        /// <returns>
        /// Wartość pod kluczem <paramref name="key"/>, jeśli istnieje, w przeciwnym przypadku <paramref name="defaultValue"/>.
        /// </returns>
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Zamienia pierwszą literę każdego wyrazu w łańcuchu na wielką literę.
        /// </summary>
        /// <param name="input">Łańcuch znaków do przetworzenia.</param>
        /// <returns>
        /// Łańcuch znaków z pierwszą literą każdego wyrazu zamienioną na wielką literę. Rozbija również wyrazy po myślnikach.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static string FirstCharToUpper(this string input) => 
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Join(" ", 
                        input.Split([' '], StringSplitOptions.RemoveEmptyEntries)
                             .Select(word => word.Length <= 1 ? word.ToLower() : string.Join("-",
                                 word.Split('-', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(part => part.Length > 1 ? string.Concat(part[0].ToString().ToUpper(), part.ToLower().AsSpan(1)) : part.ToUpper()))))
            };

        /// <summary>
        /// Zwraca polską nazwę dnia tygodnia.
        /// </summary>
        /// <param name="dayOfWeek">Dzień tygodnia, dla którego ma zostać zwrócona polska nazwa.</param>
        /// <returns>
        /// Polska nazwa dnia tygodnia.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetPolishName(this DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Poniedziałek",
                DayOfWeek.Tuesday => "Wtorek",
                DayOfWeek.Wednesday => "Środa",
                DayOfWeek.Thursday => "Czwartek",
                DayOfWeek.Friday => "Piątek",
                DayOfWeek.Saturday => "Sobota",
                DayOfWeek.Sunday => "Niedziela",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        
        /// <summary>
        /// Normalizuje polskie diakrytyki w łańcuchu znaków, zamieniając je na ich odpowiedniki bez znaków diakrytycznych.
        /// </summary>
        /// <param name="input">Łańcuch znaków do normalizacji.</param>
        /// <returns>
        /// Łańcuch znaków bez polskich znaków diakrytycznych.
        /// </returns>
        public static string NormalizePolishDiacritics(this string input)
        {
            var replacements = new Dictionary<char, char>
            {
                {'ą', 'a'},
                {'ć', 'c'},
                {'ę', 'e'},
                {'ł', 'l'},
                {'ń', 'n'},
                {'ó', 'o'},
                {'ś', 's'},
                {'ź', 'z'},
                {'ż', 'z'},
                {'Ą', 'A'},
                {'Ć', 'C'},
                {'Ę', 'E'},
                {'Ł', 'L'},
                {'Ń', 'N'},
                {'Ó', 'O'},
                {'Ś', 'S'},
                {'Ź', 'Z'},
                {'Ż', 'Z'}
            };

            var output = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                if (replacements.ContainsKey(input[i]))
                {
                    output[i] = replacements[input[i]];
                }
                else
                {
                    output[i] = input[i];
                }
            }

            return new string(output);
        }

        /// <summary>
        /// Zwraca odpowiednią formę odmiany rzeczownika w języku polskim na podstawie podanej liczby.
        /// </summary>
        /// <param name="number">Liczba, na podstawie której ma zostać wybrana odpowiednia forma odmiany.</param>
        /// <param name="form1">Forma dla liczby 1 (np. "wizyta").</param>
        /// <param name="form2">Forma dla liczb 2-4 (np. "wizyty").</param>
        /// <param name="form5">Forma dla liczb 5 i więcej (np. "wizyt").</param>
        /// <returns>Odpowiednia forma odmiany rzeczownika.</returns>
        public static string GetDeclination(int number, string form1, string form2, string form5)
        {
            int n = Math.Abs(number) % 100;
            int n1 = n % 10;

            if (n > 10 && n < 20)
            {
                return form5;
            }
            if (n1 > 1 && n1 < 5)
            {
                return form2;
            }
            if (n1 == 1)
            {
                return form1;
            }
            return form5;
        }

        /// <summary>
        /// Zaokrągla obiekt <see cref="TimeOnly"/> do najbliższej wielokrotności <paramref name="n"/> minut.
        /// </summary>
        /// <param name="timeOnly">Obiekt <see cref="TimeOnly"/> do zaokrąglenia.</param>
        /// <param name="n">Liczba minut, do której ma zostać zaokrąglony obiekt <see cref="TimeOnly"/>.</param>
        /// <param name="roundDown">Określa, czy zaokrąglenie ma być w dół (domyślnie w górę).</param>
        /// <returns>Zaokrąglony obiekt <see cref="TimeOnly"/>.</returns>
        public static TimeOnly RoundTo(this TimeOnly timeOnly, int n, bool roundDown = false)
        {
            if (roundDown)
            {
                return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(n * Math.Floor(timeOnly.ToTimeSpan().TotalMinutes / n)));
            }
            else
            {
                return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(n * Math.Ceiling(timeOnly.ToTimeSpan().TotalMinutes / n)));
            }
        }
        

        /// <summary>
        /// Zwraca <see cref="null"/> lub przycina łańcuch znaków.
        /// </summary>
        /// <param name="input">Łańcuch znaków do przetworzenia.</param>
        /// <returns>
        /// <see cref="null"/>, jeśli <paramref name="input"/> jest <see cref="null"/> lub pusty po przycięciu, 
        /// w przeciwnym razie przycięty łańcuch znaków.
        /// </returns>
        public static string? NullOrTrimmed(this string? input)
        {
            if (input == null)
                return null;

            var trimmed = input.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }
    }
}
