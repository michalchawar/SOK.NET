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

        public string StreetName { get; set; } = string.Empty;
        
        public string StreetType { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        public AddressDto(Address address)
        {
            Id = address.Id;
            ApartmentNumber = address.ApartmentNumber;
            ApartmentLetter = address.ApartmentLetter ?? string.Empty;
            
            BuildingId = address.BuildingId;
            BuildingNumber = address.BuildingNumber ?? -1;
            BuildingLetter = address.BuildingLetter ?? string.Empty;

            StreetName = address.StreetName ?? string.Empty;
            StreetType = address.StreetType ?? string.Empty;

            CityName = address.CityName ?? string.Empty;
        }
    }
}
