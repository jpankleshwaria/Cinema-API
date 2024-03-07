using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Features.Model.RequestModels
{
    public class BuySeatsRequest
    {
        [Required]
        public Guid ReservationId { get; set; }
    }
}
