using Azure.Core;
using Microsoft.EntityFrameworkCore;

namespace SurveyBasket.Api.Services;

public class PollService(ApplicationDbContext context) : IPollService
{
    private readonly ApplicationDbContext _Context = context;

    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _Context.Polls.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Result<PollResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _Context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure<PollResponse>(PollErrors.PollNotFound);


        return Result.Success(poll.Adapt<PollResponse>());
    }

    public async Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
    {
        var isExists = await _Context.Polls.AnyAsync(p => p.Tittle == request.Tittle, cancellationToken: cancellationToken);

        if (isExists)
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTittle);

        var poll = request.Adapt<Poll>();

        await _Context.AddAsync(poll, cancellationToken);
        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success(poll.Adapt<PollResponse>());
    }

    public async Task<Result> UpdateAsync(int id, PollRequest request, CancellationToken cancellationToken = default)
    {
        var currentPool = await _Context.Polls.FindAsync(id, cancellationToken);
        if (currentPool is null)
            return Result.Failure(PollErrors.PollNotFound);

        var isExists = await _Context.Polls.AnyAsync(p => p.Tittle == request.Tittle && p.Id != id, cancellationToken: cancellationToken);

        if (isExists)
            return Result.Failure(PollErrors.DuplicatedPollTittle);



        currentPool.Tittle = request.Tittle;
        currentPool.Summary = request.Summary;
        currentPool.StartsAt = request.StartsAt;
        currentPool.EndsAt = request.EndsAt;

        //CurrentPoll variable is a poll which is selected in the database

        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _Context.Polls.FindAsync(id, cancellationToken);
        if (poll is null)
            return Result.Failure(error: PollErrors.PollNotFound);

        _Context.Polls.Remove(poll);
        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _Context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        poll.IsPublished = !poll.IsPublished;

        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
