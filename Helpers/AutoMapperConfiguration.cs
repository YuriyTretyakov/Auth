using Authorization.DL.FeedBack;
using Authorization.ViewModels.Feedback;
using AutoMapper;

namespace Authorization.Helpers
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

                CreateMap<DL.FeedBack.Response, ViewModels.Feedback.Response>()
                    .ForMember(s=>s.Name,s=>s.MapFrom(m=>m.User.Name + " "+ m.User.LastName))
                    .ForMember(s => s.Picture, s => s.MapFrom(m => m.User.UserPicture));
                
            }
        }
    }
}
