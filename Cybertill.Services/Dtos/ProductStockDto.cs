namespace Cybertill.Services.Dtos
{
    public class ProductStockDto
    {
        public int OptionId { get; set; }
        public int LocationId { get; set; }
        public int Stock { get; set; }
        public int Reserved { get; set; }

        /// <summary>
        /// This is how we calculate "available" stock of an item, per Cybertill's advice
        /// </summary>
        public int Available => Stock - Reserved;

        public override string ToString()
        {
            return $"stock: {Stock}, reserved: {Reserved}, available: {Available}";
        }
    }
}
