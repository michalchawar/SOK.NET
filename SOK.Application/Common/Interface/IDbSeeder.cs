using Microsoft.AspNetCore.Identity;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Służy do zaludniania bazy danych początkowymi danymi.
    /// </summary>
    public interface IDbSeeder
    {
        /// <summary>
        /// Tworzy początkowe dane w bazie danych.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task SeedAsync();

        /// <summary>
        /// Zaludnia bazę danych parafii przykładowymi danymi.
        /// </summary>
        /// <param name="parishUid">Unikalny identyfikator parafii.</param>
        /// <param name="adminId">Identyfikator administratora parafii.</param>
        /// <param name="seedOnlyBaseInfo">Określa, czy uzupełnić tylko podstawowe informacje.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task SeedParishDbAsync(
            string parishUid, 
            string adminId, 
            bool seedOnlyBaseInfo = false
        );
    }
}