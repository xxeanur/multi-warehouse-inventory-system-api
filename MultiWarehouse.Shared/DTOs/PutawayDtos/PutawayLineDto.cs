namespace MultiWarehouse.Shared.DTOs.PutawayDtos
{
    public class PutawayLineDto
    {
        public Guid DocumentLineId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ShelfId { get; set; }
        public int Quantity { get; set; }
    }
}

