using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class AddressRepository : Repository<Address, ParishDbContext>, IAddressRepository
    {
        public AddressRepository(ParishDbContext db) : base(db) {}

        public async Task<Address?> GetFullAsync(
            Expression<Func<Address, bool>> filter, 
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);
            query = query
                .Include(a => a.Building)
                    .ThenInclude(b => b.Street)
                        .ThenInclude(s => s.Type)
                .Include(a => a.Building)
                    .ThenInclude(b => b.Street)
                        .ThenInclude(s => s.City);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Address?> GetRandomAsync()
        {
            var query = GetQueryable(tracked: false);
            query = query
                .Include(a => a.Building)
                    .ThenInclude(b => b.Street)
                        .ThenInclude(s => s.Type)
                .Include(a => a.Building)
                    .ThenInclude(b => b.Street)
                        .ThenInclude(s => s.City);

            var rand = new Random();
            var skipCount = (int)(rand.NextDouble() * dbSet.Count());

            query = query.Skip(skipCount);

            return await query.FirstOrDefaultAsync();
        }

        //public async Task CreateAddress

        //public async Task CreateAddressFromRawAsync(Address address)
        //{
        //    if (address == null) throw new ArgumentNullException("Address to create must not be null.");


        //    bool addressAlreadyNonExistent = false;

        //    // Szukamy lub tworzymy miasto
        //    City? city = await _db.Cities
        //        .FirstOrDefaultAsync(c => c.Name == address.Building.Street.City.Name);
        //    if (city == null) {
        //        // Nie znaleźliśmy miasta, zatem dalsza część adresu na pewno nie istnieje
        //        addressAlreadyNonExistent = true;
        //        city = new City { Name = address.Building.Street.City.Name };
        //    }

        //    // Szukamy lub tworzymy typ ulicy
        //    StreetSpecifier? streetType = await _db.StreetSpecifiers
        //        .FirstOrDefaultAsync(sp => sp.FullName == address.Building.Street.Type.FullName);
        //    if (streetType == null)
        //    {
        //        // Nie znaleźliśmy rodzaju ulicy, zatem dalsza część adresu na pewno nie istnieje
        //        addressAlreadyNonExistent = true;
        //        streetType = new StreetSpecifier { FullName = address.Building.Street.Type.FullName };
        //    }

        //    // Szukamy lub tworzymy ulicę
        //    Street? street = await _db.Streets
        //        .FirstOrDefaultAsync(s => s.CityId == city.Id && s.StreetSpecifierId == streetType.Id
        //                               && s.Name == address.Building.Street.Name);
        //    if (addressAlreadyNonExistent || street == null)
        //    {
        //        // Jeśli nie znaleźliśmy ulicy, to dalsza część adresu na pewno nie istnieje
        //        addressAlreadyNonExistent = true;
        //        street = new Street { Name = address.Building.Street.Name, Type = streetType, City = city };
        //    }

        //    // Szukamy lub tworzymy budynek (bramę)
        //    Building? building = await _db.Buildings
        //        .FirstOrDefaultAsync(b => b.StreetId == street.Id && b.Number == address.Building.Number 
        //                               && b.Letter == address.Building.Letter);
        //    if (addressAlreadyNonExistent || building == null)
        //    {
        //        // Jeśli nie znaleźliśmy budynku, to dalsza część adresu na pewno nie istnieje
        //        addressAlreadyNonExistent = true;
        //        building = new Building { Number = address.Building.Number, Letter = address.Building.Letter, 
        //            Street = street };
        //    }

        //    Address? dbAddress = await _db.Addresses
        //        .FirstOrDefaultAsync(a => a.BuildingId == building.Id && a.ApartmentNumber == address.ApartmentNumber
        //                               && a.ApartmentLetter == address.ApartmentLetter);

        //    if (addressAlreadyNonExistent == false && dbAddress != null)
        //        throw new InvalidOperationException("The address already exists. Cannot create one.");

        //    if (addressAlreadyNonExistent || dbAddress == null)
        //    {
        //        dbAddress = new Address
        //        {
        //            ApartmentNumber = address.ApartmentNumber,
        //            ApartmentLetter = address.ApartmentLetter,
        //            Building = building
        //        };
        //    }
        //}

        //public async Task<Address?> CreateAndGetAddressAsync(
        //    int apartmentNumber,
        //    int buildingNumber, 
        //    string street, 
        //    string streetSpecifier, 
        //    string city,
        //    string apartmentLetter = "",
        //    string buildingLetter = "")
        //{
        //    City? cityEntity = 
        //        await _db.Cities.FirstOrDefaultAsync(c => c.Name == city);

        //    StreetSpecifier? spEntity = 
        //        await _db.StreetSpecifiers.FirstOrDefaultAsync(sp => sp.FullName == streetSpecifier);

        //    IQueryable<Street> streetQuery = _db.Streets;
        //    if (cityEntity != null)
        //        streetQuery = streetQuery.Where(s => s.CityId == cityEntity.Id);
        //    if (spEntity != null)
        //        streetQuery = streetQuery.Where(s => s.StreetSpecifierId == spEntity.Id);
        //    Street? streetEntity = await streetQuery.FirstOrDefaultAsync(s => s.Name == street);

        //    IQueryable<Building> buildingQuery = _db.Buildings;
        //    if (streetEntity != null)
        //        buildingQuery = buildingQuery.Where(b => b.StreetId == streetEntity.Id);
        //    Building? buildingEntity = await buildingQuery.FirstOrDefaultAsync(b => b.Number == );

        //    if (streetEntity == null)
        //    {
        //        streetEntity = new Street { Name = street, }
        //    }

        //    if (spEntity == null)
        //    {
        //        spEntity = new StreetSpecifier { FullName = streetSpecifier };
        //        _db.Add(spEntity);
        //    }

        //    if (cityEntity == null)
        //    {
        //        cityEntity = new City { Name = city };
        //    }
        //}
    }
}
