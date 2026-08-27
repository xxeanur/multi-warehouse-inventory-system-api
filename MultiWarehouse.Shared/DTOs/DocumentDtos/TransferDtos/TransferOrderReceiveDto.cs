namespace MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos
{
    public class TransferOrderReceiveDto
    {
        public Guid TransferOrderId { get; set; }


        public List<TransferReceiveLineDto> ReceivedLines { get; set; } = new();
    }

    public class TransferReceiveLineDto
    {
        public Guid TransferOrderLineId { get; set; }
        public int Quantity { get; set; }
    }
}