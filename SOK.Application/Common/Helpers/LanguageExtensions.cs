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
    }
}
