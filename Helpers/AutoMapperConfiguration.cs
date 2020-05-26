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
                CreateMap<RequestFeedback,FeedBack>().ForMember(m=>m.Id,s=>s.Ignore()).ReverseMap();
                CreateMap<ResponseFeedback, FeedBack>().ForMember(s => s.Id, d => d.Ignore()).ReverseMap();
            }
        }
    }
}
