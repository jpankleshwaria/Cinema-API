namespace ApiApplication.Features.Model.ResponseModels
{
    public class ReservationResult
    {
        public bool Success { get; set; }
        public Reservation Reservation { get; set; }
        public string ErrorMessage { get; set; }
    }
}
