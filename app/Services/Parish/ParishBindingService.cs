using app.Models.Parish;
using app.Data;
using Microsoft.EntityFrameworkCore;

namespace app.Services.Parish
{
    /// <summary>
    /// Klasa do mapowania danych z bazy danych na obiekt Parafia.
    /// Pobiera jednostkowe informacje z tabeli ParishInfo bie¿¹cej parafii
    /// i tworzy obiekt Parish z odpowiednimi w³aœciwoœciami 
    /// (w tym z obiektami typu Diocese i Address).
    /// </summary>
    public class ParishBindingService
    {
        private readonly ParishDbContext _context;

        public ParishBindingService(ParishDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Pobiera informacje o parafii z bazy danych i mapuje je na obiekt Parish.
        /// </summary>
        /// <returns>Obiekt Parish, reprezentuj¹cy wybran¹ w kontekœcie ParishDbContext parafiê</returns>
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