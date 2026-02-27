namespace SurveyBasket.Api.Services;

public class ResultService(ApplicationDbContext context) : IResultService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollVotes = await _context.Polls
            .Where(x => x.Id == pollId)
            .Select(x => new PollVotesResponse(

                  x.Tittle,
                  x.Votes.Select(v => new VotesResponse(
                      $"{v.User.FirstName}-{v.User.LastName}",
                      v.SubmittedOn,
                      v.VoteAnswers.Select(c => new QuestionAnswerResponse(
                         c.Question.Content,
                         c.Answer.Content
                      )

                   ))))
            ).SingleOrDefaultAsync(cancellationToken);


        return pollVotes is null
            ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound)
            : Result.Success(pollVotes);
    }
}
