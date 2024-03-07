using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Features.Model.RequestModels
{
    public class CreateShowRequest
    {
        [Required]
        public string MovieTitle { get; set; }
        [Required]
        public DateTime SessionDate { get; set; }
        [Required]
        public int AuditoriumId { get; set; }
    }
}
