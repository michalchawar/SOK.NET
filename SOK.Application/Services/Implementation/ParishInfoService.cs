using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class ParishInfoService : IParishInfoService
    {
        private readonly IUnitOfWorkParish _uow;

        public ParishInfoService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<string?> GetValueAsync(string optionName)
        {
            ParishInfo? option = await _uow.ParishInfo.GetAsync(pi => pi.Name == optionName);
            return option?.Value ?? null;
        }

        /// <inheritdoc />
        public async Task UpdateValueAsync(string optionName, string value)
        {
            var option = await _uow.ParishInfo.GetAsync(pi => pi.Name == optionName);

            if (option != null)
            {
                option.Value = value;
                _uow.ParishInfo.Update(option);
            }
            else
            {
                var newOption = new ParishInfo
                {
                    Name = optionName,
                    Value = value
                };
                _uow.ParishInfo.Add(newOption);
            }

            await _uow.SaveAsync();
        }

        /// <inheritdoc/>
        public async Task<ParishDto> BindParishAsync()
        {
            var infos = await _uow.ParishInfo
                .ToDictionaryAsync();

            var city = new City { Name = infos["Address.City.Name"], DisplayName = infos.GetValueOrDefault("Address.City.DisplayName") };
            var streetSpecifier = new StreetSpecifier { FullName = infos["Street.Specifier.FullName"], Abbreviation = infos.GetValueOrDefault("Address.StreetSpecifier.Abbreviation") };
            var street = new Street { Name = infos["Address.Street.Name"], PostalCode = infos.GetValueOrDefault("Address.Street.PostalCode"), Type = streetSpecifier };
            var building = new Building { Number = int.Parse(infos["Address.Building.Number"]), Letter = infos.GetValueOrDefault("Address.Building.Letter"), Street = street };

            var parish = new ParishDto
            {
                UniqueId = Guid.Parse(infos["UniqueId"]).ToString(),
                FullName = infos["FullName"],
                DioceseName = infos["Diocese.Name"],
                Address = new Address
                {
                    ApartmentNumber = int.Parse(infos.GetValueOrDefault("Address.ApartmentNumber", "0")),
                    ApartmentLetter = infos.GetValueOrDefault("Address.ApartmentLetter"),
                    Building = building
                }
            };

            return parish;
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, string>> GetDictionaryAsync() => await _uow.ParishInfo.ToDictionaryAsync();

        /// <inheritdoc />
        public Task<Dictionary<string, string>> GetValuesAsync(IEnumerable<string> options)
        {
            return _uow.ParishInfo.GetValuesAsDictionaryAsync(options);
        }
    }
}
