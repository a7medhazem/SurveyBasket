using Mapster;
using SurveyBasket.Api.Entites;

namespace SurveyBasket.Api.Services;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _Context = context;


    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _Context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _Context.Questions
            .Where(x => x.PollId == pollId)
            .Include(x => x.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }

    public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {

        var question = await _Context.Questions
            .Where(x => x.PollId == pollId && x.Id == id)
            .Include(x => x.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);


        return Result.Success(question);
    }


    public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _Context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var questionIsExists = await _Context.Questions.AnyAsync(x => x.Content == request.Content && x.Id == pollId, cancellationToken);

        if (questionIsExists)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        //after checking about pollid and duplicated question content

        var question = request.Adapt<Question>();
        question.PollId = pollId;

        // request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));//I'm used  mapping

        await _Context.AddAsync(question, cancellationToken);
        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());

    }


}
