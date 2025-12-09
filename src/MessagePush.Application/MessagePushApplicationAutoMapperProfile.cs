using AutoMapper;
using MessagePush.DeviceInfo.Dtos;
using MessagePush.Entities.Es;

namespace MessagePush;

public class MessagePushApplicationAutoMapperProfile : Profile
{
    public MessagePushApplicationAutoMapperProfile()
    {
        CreateMap<DeviceInfoDto, Entities.Es.DeviceInfo>();
        CreateMap<UserDeviceInfoDto, UserDeviceIndex>()
            .ForMember(t => t.RegistrationToken, m => m.MapFrom(f => f.Token));
        CreateMap<SwitchNetworkDto, ExitWalletDto>();
    }
}