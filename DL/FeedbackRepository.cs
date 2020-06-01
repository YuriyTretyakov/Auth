using Authorization.Identity;
using Authorization.ViewModels.Feedback;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task AddFeedBack(RequestFeedback feedback,User user)
        {
            var feedbackDb = _mapper.Map<RequestFeedback, FeedBack.FeedBack>(feedback);
            feedbackDb.CreatedOn = DateTime.UtcNow;
            feedbackDb.User = user;
            _context.Feedback.Add(feedbackDb);
            await SaveChangesAsync();
        }

        public  ResponseFeedback[] GetAllFeedbacks()
        {
            var feedbacks = _context.Feedback.Include(t => t.User).Include(t=>t.Responses).Include("Responses.User");

            var sortedFeedbacksAndComments = feedbacks.OrderByDescending(r => r.CreatedOn).ToList();

         //   feedbacks.Include(f => f.Responses).Include("Responses.User");

            sortedFeedbacksAndComments.ForEach(f => f.Responses = f.Responses.OrderByDescending(r => r.CreatedOn).ToList());

            var feedbacksSorted = _mapper.Map<FeedBack.FeedBack[], ResponseFeedback[]>(sortedFeedbacksAndComments.ToArray());
            return feedbacksSorted;
        }

        public async Task AddCommentToFeedback(AddCommentRequest requestModel,User user)
        {

            var comment = new FeedBack.Response
            {
                User = user,
                CreatedOn = DateTime.UtcNow,
                Details = requestModel.Text,
                FeedbackId = requestModel.FeedbackId
            };


            var feedback = _context.Feedback.Include(t => t.User).Include(t => t.Responses);

            var concreteFeedback = _context.Feedback.FirstOrDefault(f => f.Id == requestModel.FeedbackId);


            if (concreteFeedback.Responses == null)
                concreteFeedback.Responses = new HashSet<FeedBack.Response>();

            concreteFeedback.Responses.Add(comment);
            await SaveChangesAsync();

        }

        public async Task<bool> SaveChangesAsync()
        {
            return ((await _context.SaveChangesAsync()) > 0);
        }
    }
}
