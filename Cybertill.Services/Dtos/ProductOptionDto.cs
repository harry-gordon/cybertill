namespace Cybertill.Services.Dtos
{
    public class ProductOptionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Reference { get; set; }
        public double PriceRrp { get; set; }
        public double PriceWeb { get; set; }
        public string ManufacturerNumber { get; set; }

        public ProductOptionDto(int id, int productId, string name, string reference, double priceRrp, double priceWeb, string manufacturerNumber)
        {
            Id = id;
            ProductId = productId;
            Name = name;
            Reference = reference;
            PriceRrp = priceRrp;
            PriceWeb = priceWeb;
            ManufacturerNumber = manufacturerNumber;
        }
    }
}
