using Mapster;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Result> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var question = await _Context.Questions
            .Include(x => x.Answers)
            .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        if (!question.IsActive)
            return Result.Failure(QuestionErrors.QuestionNotActive);

        var QuestionIsExists = await _Context.Questions
            .AnyAsync(p => p.PollId == pollId
                   && p.Id != id
                   && p.Content == request.Content,
                   cancellationToken: cancellationToken
             );

        if (QuestionIsExists)
            return Result.Failure(QuestionErrors.DuplicatedQuestionContent);

        // starting of actual update after checking
        question.Content = request.Content;

        // current answers
        var currentAnswers = question.Answers
            .Select(x => x.Content)
            .ToList();

        // add new answers
        var newAnswers = request.Answers
            .Except(currentAnswers)
            .ToList();

        foreach (var answer in newAnswers)
        {
            question.Answers.Add(new Answer
            {
                Content = answer
            });
        }

        // update IsActive status
        foreach (var answer in question.Answers.ToList())
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        }


        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await _Context.Questions
            .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;
        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


}
