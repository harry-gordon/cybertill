namespace Cybertill.Services.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ProductDto(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
