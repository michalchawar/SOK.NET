using app.Models;
using app.Data;
using Microsoft.EntityFrameworkCore;

public class ParishMappingService
{
    private readonly sokAppContext _context;

    public ParishMappingService(sokAppContext context)
    {
        _context = context;
    }

    public async Task<Parish> GetParishAsync()
    {
        var infos = await _context.ParishInfo
            .ToDictionaryAsync(pi => pi.Name, pi => pi.Value);

        var city = new City { Name = infos["Address.City.Name"], DisplayName = infos.GetValueOrDefault("Address.City.DisplayName") };
        var streetSpecifier = new StreetSpecifier { FullName = infos["Street.Specifier.FullName"], Abbreviation = infos.GetValueOrDefault("Address.StreetSpecifier.Abbreviation") };
        var street = new Street { Name = infos["Address.Street.Name"], PostalCode = infos.GetValueOrDefault("Address.Street.PostalCode"), Type = streetSpecifier };
        var building = new Building { Number = int.Parse( infos["Address.Building.Number"] ), Letter = infos.GetValueOrDefault("Address.Building.Letter"), Street = street };

        var parish = new Parish
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
                ApartmentNumber = int.Parse( infos.GetValueOrDefault("Address.ApartmentNumber", "0") ),
                ApartmentLetter = infos.GetValueOrDefault("Address.ApartmentLetter"),
                Building = building,
                Street = street,
                City = city
            }
        };

        return parish;
    }
}