using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje budynek, który może być przypisany do parafii i zawierać wiele adresów oraz mieszkań.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Unikalny identyfikator budynku (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer budynku (np. 1, 2, 10).
        /// Krotka (Street, Number, Letter) jest unikalna.
        /// </summary>
        [Range(1, 300)]
        public int Number { get; set; }

        /// <summary>
        /// Litera budynku (np. A, B, C), jeśli występuje.
        /// </summary>
        [MaxLength(3)]
        public string? Letter { get; set; } = null;

        /// <summary>
        /// Liczba pięter w budynku. Wartość -1 oznacza brak danych.
        /// </summary>
        [DefaultValue(-1)]
        public int FloorCount { get; set; } = -1;

        /// <summary>
        /// Liczba mieszkań w budynku. Wartość -1 oznacza brak danych.
        /// </summary>
        [DefaultValue(-1)]
        public int ApartmentCount { get; set; } = -1;

        /// <summary>
        /// Najwyższy numer mieszkania w budynku. Wartość -1 oznacza brak danych.
        /// </summary>
        [DefaultValue(-1)]
        public int HighestApartmentNumber { get; set; } = -1;

        /// <summary>
        /// Określa, czy budynek posiada windę. Jest to używane do planowania wizyt w budynku.
        /// Wizyty w budynkach bez windy są planowane od parteru w górę, podczas gdy 
        /// w budynkach z windą planowane są od najwyższego piętra w dół.
        /// </summary>
        [DefaultValue(false)]
        public bool HasElevator { get; set; } = false;

        /// <summary>
        /// Określa, czy budynek jest dostępny do wyboru w formularzu internetowym dla niezalogowanego użytkownika.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowSelection { get; set; } = true;

        /// <summary>
        /// Identyfikator ulicy, przy której znajduje się budynek.
        /// </summary>
        public int StreetId { get; set; }

        /// <summary>
        /// Ulica, przy której znajduje się budynek (relacja nawigacyjna).
        /// </summary>
        public Street Street { get; set; } = default!;

        /// <summary>
        /// Lista przypisań dni powiązanych z budynkiem. To klasa pomocnicza relacji wiele-do-wielu między dniem a budynkami.
        /// </summary>
        public ICollection<BuildingAssignment> BuildingAssignments { get; set; } = new List<BuildingAssignment>();

        /// <summary>
        /// Lista dni, do których budynek jest przypisany. Każdy budynek może być przypisany do wielu dni w różnych harmonogramach.
        /// </summary>
        public ICollection<Day> Days { get; set; } = new List<Day>();

        /// <summary>
        /// Lista jednostek adresowych w budynku.
        /// </summary>
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}