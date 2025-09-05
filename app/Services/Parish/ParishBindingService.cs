using app.Models.Parish;
using app.Models.Parish.Entities;
using app.Data;
using Microsoft.EntityFrameworkCore;

namespace app.Services.Parish
{
    /// <summary>
    /// Klasa do mapowania danych z bazy danych na obiekt <see cref="Parish"/>.
    /// Pobiera jednostkowe informacje z informacji <see cref="ParishInfo"/> bie¿¹cej parafii
    /// i tworzy obiekt <see cref="Parish"/> z odpowiednimi w³aœciwoœciami 
    /// (w tym z obiektami <see cref="Diocese"/> i <see cref="Address"/>).
    /// </summary>
    public class ParishBindingService
    {
        private readonly ParishDbContext _context;

        public ParishBindingService(ParishDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Pobiera informacje o parafii z bazy danych i binduje je na obiekt <see cref="Parish"/>.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// zawieraj¹cy obiekt <see cref="Models.Parish.Parish"/>, reprezentuj¹cy wybran¹ parafiê.
        /// </returns>
        /// <remarks>
        /// Wybrana parafia jest okreœlona przez kontekst dostêpu do bazy danych
        /// <see cref="ParishDbContext"/>, a konkretnie jego aktualn¹ konfiguracjê.
        /// </remarks>
        public async Task<Models.Parish.Parish> GetParishAsync()
        {
            var infos = await _context.ParishInfo
                .ToDictionaryAsync(pi => pi.Name, pi => pi.Value);

            var city = new City { Name = infos["Address.City.Name"], DisplayName = infos.GetValueOrDefault("Address.City.DisplayName") };
            var streetSpecifier = new StreetSpecifier { FullName = infos["Street.Specifier.FullName"], Abbreviation = infos.GetValueOrDefault("Address.StreetSpecifier.Abbreviation") };
            var street = new Street { Name = infos["Address.Street.Name"], PostalCode = infos.GetValueOrDefault("Address.Street.PostalCode"), Type = streetSpecifier };
            var building = new Building { Number = int.Parse(infos["Address.Building.Number"]), Letter = infos.GetValueOrDefault("Address.Building.Letter"), Street = street };

            var parish = new Models.Parish.Parish
            {
                UniqueId = Guid.Parse(infos["UniqueId"]),
                FullName = infos["FullName"],
                Diocese = new Diocese
                {
                    Name = infos["Diocese.Name"],
                    DisplayName = infos.GetValueOrDefault("Diocese.DisplayName")
                },
                Address = new Address
                {
                    ApartmentNumber = int.Parse(infos.GetValueOrDefault("Address.ApartmentNumber", "0")),
                    ApartmentLetter = infos.GetValueOrDefault("Address.ApartmentLetter"),
                    Building = building,
                    Street = street,
                    City = city
                }
            };

            return parish;
        }
    }
}