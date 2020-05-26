using Authorization.Identity;
using Authorization.ViewModels.Feedback;
using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace Authorization.DL
{
    public class FeedBackRepository
    {
        private readonly AuthDbContext _context;
        private readonly IMapper _mapper;

        public FeedBackRepository(AuthDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddFeedBack(RequestFeedback feedback,string name, string userPic)
        {
            var feedbackDb = _mapper.Map<RequestFeedback, FeedBack.FeedBack>(feedback);
            feedbackDb.CreatedOn = DateTime.UtcNow;
            feedbackDb.Name = name;
            feedbackDb.Picture = userPic;
            _context.Feedback.Add(feedbackDb);
            await SaveChangesAsync();
        }

        public  ResponseFeedback[] GetAllFeedbacks()
        {
            var feedbacks = _mapper.Map<FeedBack.FeedBack[], ResponseFeedback[]>(_context.Feedback.ToArray());
            return feedbacks;
        }

        //public async Task<IAsyncResult> GetFeedbacksByUser()
        //{

        //}

        public async Task<bool> SaveChangesAsync()
        {
            return ((await _context.SaveChangesAsync()) > 0);
        }
    }
}
