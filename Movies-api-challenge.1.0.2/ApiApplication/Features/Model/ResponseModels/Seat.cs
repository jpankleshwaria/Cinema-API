namespace ApiApplication.Features.Model.ResponseModels
{
    public class SelectedSeats {
        public short Row { get; set; }
        public short SeatNumber { get; set; }
    }
    public class Seat
    {
        public int AuditoriumId { get; set; }
        public int Row { get; set; }
        public int SeatNumber { get; set; }
        public bool IsReserved { get; set; }
        public bool IsSold { get; set; }
    }
}
