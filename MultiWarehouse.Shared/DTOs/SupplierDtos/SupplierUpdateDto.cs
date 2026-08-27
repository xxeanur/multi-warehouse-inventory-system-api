namespace MultiWarehouse.Shared.DTOs.SupplierDtos
{
    public class SupplierUpdateDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string Country { get; set; } = "Türkiye";
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string TaxNumber { get; set; } = string.Empty;
        public string TaxOffice { get; set; } = string.Empty;
    }
}