using Application.Commands.IoT;
using Application.DataTransferObjects.Requests.IoT;
using AutoMapper;

namespace Infrastructure.MappingProfiles;

public class IoTProfile : Profile
{
    public IoTProfile()
    {
        CreateMap<IoTDataRequest, MeasurementPointMonitoringCommand>();
        CreateMap<IoTDataRequest, MeasurementPointEventCommand>();
    }
}