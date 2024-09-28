namespace Apartments.Application.Dtos.RentTransactionDtos;

public class RentTransactionDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ApartmentId { get; set; }
    public string ApartmentOwner { get; set; } = string.Empty;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public decimal RentAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
