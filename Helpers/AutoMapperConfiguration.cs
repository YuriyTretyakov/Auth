using ColibriWebApi.DL;
using ColibriWebApi.ViewModels.Feedback;
using AutoMapper;
using Google.Apis.Calendar.v3.Data;

namespace ColibriWebApi.Helpers
{
    public class AutoMapperConfiguration
    {
        public MapperConfiguration Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ClientMappingProfile>();
            });
            return config;
        }

        public class ClientMappingProfile : Profile
        {
            public ClientMappingProfile()
            {

                CreateMap<FeedBack, ResponseFeedback>()
                    .ForMember(s => s.Name, s => s.MapFrom(m => m.User.Name + " " + m.User.LastName))
                    .ForMember(s => s.Picture, s => s.MapFrom(m => m.User.UserPicture));
                CreateMap<RequestFeedback, FeedBack>().ReverseMap();

                CreateMap<DL.Response, ViewModels.Feedback.Response>()
                    .ForMember(s=>s.Name,s=>s.MapFrom(m=>m.User.Name + " "+ m.User.LastName))
                    .ForMember(s => s.Picture, s => s.MapFrom(m => m.User.UserPicture));

                CreateMap<Event, ExternalApis.Google.Event>()
                    .ForMember(s => s.Start, s => s.MapFrom(m => m.Start.DateTime))
                    .ForMember(s => s.End, s => s.MapFrom(m => m.End.DateTime));
            }
        }
    }
}
