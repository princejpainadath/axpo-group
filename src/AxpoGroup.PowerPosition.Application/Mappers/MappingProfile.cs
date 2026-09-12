using AutoMapper;
using Axpo;
using AxpoGroup.PowerPosition.Application.DTOs;

namespace AxpoGroup.PowerPosition.Application.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PowerPeriod, PowerPeriodDto>();

            CreateMap<PowerTrade, PowerTradeDto>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Periods, opt => opt.MapFrom(src => src.Periods));

            CreateMap<Domain.Entities.PowerPosition, PowerPositionDto>()
                .ForMember(dest => dest.LocalTime, opt => opt.MapFrom(src => src.LocalTime))
                .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.Volume));

        }
    }
}
