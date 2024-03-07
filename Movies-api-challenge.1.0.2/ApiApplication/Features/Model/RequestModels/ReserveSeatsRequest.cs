using System.ComponentModel.DataAnnotations;
using System;
using ApiApplication.Database.Entities;
using System.Collections.Generic;
using ApiApplication.Features.Model.ResponseModels;

namespace ApiApplication.Features.Model.RequestModels
{
    public class ReserveSeatsRequest
    {

        public int ShowTimeId {get; set; }

        public List<SelectedSeats> Seats { get; set; }

    }
}
