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
    }
}