namespace SOK.Application.Common.Helpers
{
    public static class RegExpressions
    {
        public const string PhoneNumberPattern = @"^\+?[0-9\s\-]{9,11}$";
        public const string PostalCodePattern = @"^\d{2}-\d{3}$";
        public const string EmailPattern = @"^[^@\s]{3,100}@[^@\s]{3,100}\.[^@\s]{2,10}$";
        public const string BuildingNumberPattern = @"^([1-2][0-9]{0,2}|[1-9][0-9]{0,1})[a-zA-Z]{0,3}$";
        public const string ApartmentNumberPattern = @"^([1-2][0-9]{0,2}|[1-9][0-9]{0,1})[a-zA-Z]{0,3}$";
    }
}