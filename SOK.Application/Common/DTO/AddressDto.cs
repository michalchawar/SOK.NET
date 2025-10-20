using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.DTO
{
    public class AddressDto
    {
        public int Id { get; set; }
        
        public int? ApartmentNumber { get; set; }
        public string ApartmentLetter { get; set; } = string.Empty;

        public int BuildingId { get; set; }
        public int BuildingNumber { get; set; }
        public string BuildingLetter { get; set; } = string.Empty;

        public int StreetId { get; set; }
        public string StreetName { get; set; } = string.Empty;
        
        public int StreetSpecifierId { get; set; }
        public string StreetType { get; set; } = string.Empty;

        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;

        public AddressDto(Address address)
        {
            Id = address.Id;
            ApartmentNumber = address.ApartmentNumber;
            ApartmentLetter = address.ApartmentLetter ?? string.Empty;
            
            BuildingId = address.Building.Id;
            BuildingNumber = address.Building.Number;
            BuildingLetter = address.Building.Letter ?? string.Empty;

            StreetId = address.Building.Street.Id;
            StreetName = address.Building.Street.Name;

            StreetSpecifierId = address.Building.Street.Type.Id;
            StreetType = address.Building.Street.Type.FullName;

            CityId = address.Building.Street.City.Id;
            CityName = address.Building.Street.City.Name;
        }
    }
}
