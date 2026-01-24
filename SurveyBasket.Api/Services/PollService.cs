
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace SurveyBasket.Api.Services;

public class PollService(ApplicationDbContext context) : IPollService
{
    private readonly ApplicationDbContext _Context = context;

    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _Context.Polls.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Poll?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await _Context.Polls.FindAsync(id, cancellationToken);

    public async Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken = default)
    {
        await _Context.AddAsync(poll, cancellationToken);
        await _Context.SaveChangesAsync(cancellationToken);

        return poll;
    }

    public async Task<bool> UpdateAsync(int id, Poll poll, CancellationToken cancellationToken = default)
    {
        var currentPool = await GetAsync(id, cancellationToken);
        if (currentPool is null)
            return false;

        currentPool.Tittle = poll.Tittle;
        currentPool.Summary = poll.Summary;
        currentPool.StartsAt = poll.StartsAt;
        currentPool.EndsAt = poll.EndsAt;

        await _Context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await GetAsync(id, cancellationToken);
        if (poll is null)
            return false;

        _Context.Polls.Remove(poll);
        await _Context.SaveChangesAsync(cancellationToken);

        return true;

    }
    public async Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var pool = await GetAsync(id, cancellationToken);
        if (pool is null)
            return false;

        pool.IsPublished = !pool.IsPublished;


        await _Context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
