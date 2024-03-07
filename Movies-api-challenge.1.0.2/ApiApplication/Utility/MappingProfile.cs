using ApiApplication.Database.Entities;
using ApiApplication.Features.Auditorium.Queries.GetAuditoriumById;
using ApiApplication.Features.Model.ResponseModels;
using ApiApplication.Features.Seats.Command;
using ApiApplication.Features.Shows.Command.CreateShow;
using AutoMapper;
using System;
using System.Reflection;

namespace ApiApplication.Utility
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AuditoriumEntity, GetAuditoriumByIdQuery>().ReverseMap();
            CreateMap<ShowtimeEntity, CreateShowCommand>().ReverseMap();
            CreateMap<Reservation, ReserveSeatsCommand>().ReverseMap();
            CreateMap<Reservation, ConfirmReservationCommand>().ReverseMap();

            CreateMap<SelectedSeats, SeatEntity>()
                .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.Row))
                .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.SeatNumber));

            CreateMap<SeatEntity, Seat>()
                .ForMember(dest => dest.AuditoriumId, opt => opt.MapFrom(src => src.AuditoriumId))
                .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.Row))
                .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.SeatNumber))
                .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(src => true)) 
                .ForMember(dest => dest.IsSold, opt => opt.MapFrom(src => false)); 

            CreateMap<Reservation, TicketEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ReservationId)) 
            .ForMember(dest => dest.ShowtimeId, opt => opt.MapFrom(src => src.ShowtimeId)) 
            .ForMember(dest => dest.Seats, opt => opt.MapFrom(src => src.SeatNumbers)) 
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => DateTime.Now)) 
            .ForMember(dest => dest.Paid, opt => opt.MapFrom(src => false)); 

            CreateMap<TicketEntity, Reservation>()
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.SeatCount, opt => opt.MapFrom(src => src.Seats.Count)) 
                .ForMember(dest => dest.SeatNumbers, opt => opt.MapFrom(src => src.Seats)) 
                .ForMember(dest => dest.ShowtimeId, opt => opt.MapFrom(src => src.ShowtimeId)) 
                .ForMember(dest => dest.Showtime, opt => opt.MapFrom(src => src.Showtime)) 
                .ForMember(dest => dest.Movie, opt => opt.MapFrom(src => src.Showtime.Movie)) 
                .ForMember(dest => dest.SessionDate, opt => opt.MapFrom(src => src.Showtime.SessionDate)); 

        }
    }
}
