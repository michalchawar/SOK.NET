using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOK.Infrastructure.Persistence.Migrations.Parish
{
    /// <inheritdoc />
    public partial class AddAddressCacheAndTriggersAndFilterStringForAddressAndSubmitter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuildingLetter",
                table: "Addresses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuildingNumber",
                table: "Addresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "Addresses",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetName",
                table: "Addresses",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetType",
                table: "Addresses",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilterableString",
                table: "Submitters",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                computedColumnSql: "LOWER(CONCAT_WS(' ', COALESCE(Name, ''), COALESCE(Surname, ''), COALESCE(Name, ''), COALESCE(Email, ''), COALESCE(Phone, '')))",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "FilterableString",
                table: "Addresses",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                computedColumnSql: "LOWER(CONCAT_WS(' ', COALESCE(StreetType, ''), COALESCE(StreetName, ''), CONCAT(COALESCE(BuildingNumber, ''), COALESCE(BuildingLetter, '')), CONCAT(COALESCE(ApartmentNumber, ''), COALESCE(ApartmentLetter, '')), COALESCE(CityName, '')))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submitters_FilterableString",
                table: "Submitters",
                column: "FilterableString");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_FilterableString",
                table: "Addresses",
                column: "FilterableString");


            // Manually add the SQL functions and triggers needed for this migration:

            // Trigger 1: INSERT / UPDATE na Addresses
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_Address_InsertOrUpdate_Cache
                ON Addresses
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE a
                    SET 
                        a.BuildingNumber = b.Number,
                        a.BuildingLetter = b.Letter,
                        a.StreetName = s.Name,
                        a.StreetType = ss.FullName,
                        a.CityName = c.Name
                    FROM Addresses a
                    INNER JOIN inserted i ON a.Id = i.Id
                    INNER JOIN Buildings b ON a.BuildingId = b.Id
                    INNER JOIN Streets s ON b.StreetId = s.Id
                    INNER JOIN StreetSpecifiers ss ON s.StreetSpecifierId = ss.Id
                    INNER JOIN Cities c ON s.CityId = c.Id;
                END;
                ");

            // Trigger 2: UPDATE na Buildings
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_Building_Update_AddressCache
                ON Buildings
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE a
                    SET 
                        a.BuildingNumber = b.Number,
                        a.BuildingLetter = b.Letter,
                        a.StreetName = s.Name,
                        a.StreetType = ss.FullName,
                        a.CityName = c.Name
                    FROM Addresses a
                    INNER JOIN Buildings b ON a.BuildingId = b.Id
                    INNER JOIN inserted i ON b.Id = i.Id
                    INNER JOIN Streets s ON b.StreetId = s.Id
                    INNER JOIN StreetSpecifiers ss ON s.StreetSpecifierId = ss.Id
                    INNER JOIN Cities c ON s.CityId = c.Id;
                END;
                ");

            // Trigger 3: UPDATE na Streets
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_Street_Update_AddressCache
                ON Streets
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE a
                    SET 
                        a.StreetName = s.Name,
                        a.StreetType = ss.FullName,
                        a.CityName = c.Name
                    FROM Addresses a
                    INNER JOIN Buildings b ON a.BuildingId = b.Id
                    INNER JOIN Streets s ON b.StreetId = s.Id
                    INNER JOIN inserted i ON s.Id = i.Id
                    INNER JOIN StreetSpecifiers ss ON s.StreetSpecifierId = ss.Id
                    INNER JOIN Cities c ON s.CityId = c.Id;
                END;
                ");

            // Trigger 4: UPDATE na Cities
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_City_Update_AddressCache
                ON Cities
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE a
                    SET 
                        a.CityName = c.Name
                    FROM Addresses a
                    INNER JOIN Buildings b ON a.BuildingId = b.Id
                    INNER JOIN Streets s ON b.StreetId = s.Id
                    INNER JOIN StreetSpecifiers ss ON s.StreetSpecifierId = ss.Id
                    INNER JOIN Cities c ON s.CityId = c.Id
                    INNER JOIN inserted i ON c.Id = i.Id;
                END;
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submitters_FilterableString",
                table: "Submitters");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_FilterableString",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "FilterableString",
                table: "Submitters");

            migrationBuilder.DropColumn(
                name: "FilterableString",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "BuildingLetter",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "StreetName",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "StreetType",
                table: "Addresses");


            // Manually drop the SQL functions and triggers added in this migration:
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS TR_Address_InsertOrUpdate_Cache;
                DROP TRIGGER IF EXISTS TR_Building_Update_AddressCache;
                DROP TRIGGER IF EXISTS TR_Street_Update_AddressCache;
                DROP TRIGGER IF EXISTS TR_City_Update_AddressCache;
            ");
        }
    }
}
