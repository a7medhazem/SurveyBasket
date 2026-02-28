using System.Linq;

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

    public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollExists = await _context.Polls
             .AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!pollExists)
            return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

        var votesPerDay = await _context.Votes
             .Where(v => v.PollId == pollId)
             .GroupBy(v => DateOnly.FromDateTime(v.SubmittedOn))
             .Select(g => new VotesPerDayResponse(
                 g.Key,
                 g.Count()
             ))
             .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);
    }
}
