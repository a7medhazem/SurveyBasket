
namespace SurveyBasket.Api.Services;

public class PollService : IPollService
{
    private static readonly List<Poll> _Polls = [
        new Poll(){
            Id=1,
            Tittle="poll 1",
            Description="description of poll 1"
        }
        ];

    public IEnumerable<Poll> GetAll() => _Polls;

    public Poll? Get(int id) => _Polls.SingleOrDefault(x => x.Id == id);

    public Poll Add(Poll poll)
    {
        poll.Id = _Polls.Count + 1;
        _Polls.Add(poll);
        return poll;

    }

    public bool Update(int id, Poll poll)
    {
        var currentPool = Get(id);
        if (currentPool is null)
            return false;

        currentPool.Tittle = poll.Tittle;
        currentPool.Description = poll.Description;

        return true;
    }

    public bool Delete(int id)
    {
        var poll = Get(id);
        if (poll is null)
            return false;

        _Polls.Remove(poll);
        return true;

    }
}
