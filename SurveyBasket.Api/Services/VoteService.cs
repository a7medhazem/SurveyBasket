using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Services;

public class VoteService(ApplicationDbContext context) : IVoteService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken)
    {
        // Check if the current user has already voted in this poll (PollId)
        var hasVote = await _context.Votes
            .AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken: cancellationToken);

        if (hasVote)
            return Result.Failure(VoteErrors.DuplicatedVote);

        // Validate that the poll exists, is published, and currently within its active date range
        var pollIsExists = await _context.Polls
            .AnyAsync(x => x.Id == pollId
                   && x.IsPublished
                   && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                   && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        if (!pollIsExists)
            return Result.Failure(PollErrors.PollNotFound);

        // Validate that the submitted question IDs exactly match the available active questions for the given poll
        var availableQuestions = await _context.Questions
            .Where(x => x.PollId == pollId && x.IsActive)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (!request.Answers.Select(x => x.QuestionId).SequenceEqual(availableQuestions))
            return Result.Failure(VoteErrors.InvalidQuestions);

        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            VoteAnswers = request.Answers.Adapt<IEnumerable<VoteAnswer>>().ToList()
        };

        await _context.Votes.AddAsync(vote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);


        return Result.Success();
    }
}
