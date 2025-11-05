using System.Collections;

namespace SOK.Infrastructure.Extensions
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
    }
}
