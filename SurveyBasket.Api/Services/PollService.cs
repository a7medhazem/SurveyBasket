namespace SurveyBasket.Api.Services;

public class PollService(ApplicationDbContext context) : IPollService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
          await _context.Polls
              .AsNoTracking()
              .ProjectToType<PollResponse>()
              .ToListAsync(cancellationToken);

    public async Task<IEnumerable<PollResponse>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
          await _context.Polls
              .Where(x => x.IsPublished
                       && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                       && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow))
              .AsNoTracking()
              .ProjectToType<PollResponse>()
              .ToListAsync(cancellationToken);

    public async Task<Result<PollResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure<PollResponse>(PollErrors.PollNotFound);


        return Result.Success(poll.Adapt<PollResponse>());
    }

    public async Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
    {
        var isExists = await _context.Polls.AnyAsync(p => p.Tittle == request.Tittle, cancellationToken: cancellationToken);

        if (isExists)
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTittle);

        var poll = request.Adapt<Poll>();

        await _context.AddAsync(poll, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(poll.Adapt<PollResponse>());
    }

    public async Task<Result> UpdateAsync(int id, PollRequest request, CancellationToken cancellationToken = default)
    {
        var currentPool = await _context.Polls.FindAsync(id, cancellationToken);
        if (currentPool is null)
            return Result.Failure(PollErrors.PollNotFound);

        var isExists = await _context.Polls.AnyAsync(p => p.Tittle == request.Tittle && p.Id != id, cancellationToken: cancellationToken);

        if (isExists)
            return Result.Failure(PollErrors.DuplicatedPollTittle);



        currentPool.Tittle = request.Tittle;
        currentPool.Summary = request.Summary;
        currentPool.StartsAt = request.StartsAt;
        currentPool.EndsAt = request.EndsAt;

        //CurrentPoll variable is a poll which is selected in the database

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);
        if (poll is null)
            return Result.Failure(error: PollErrors.PollNotFound);

        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        poll.IsPublished = !poll.IsPublished;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
