using Mapster;
using Microsoft.EntityFrameworkCore;
using SurveyBasket.Api.Entites;
using System.Collections.Generic;

namespace SurveyBasket.Api.Services;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;


    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _context.Questions
            .Where(x => x.PollId == pollId)
            .Include(x => x.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }


    /// <summary>
    /// Retrieves available active questions for a specific poll 
    /// only if the poll is published, within its active date range, 
    /// and the user has not already voted.
    /// </summary>
    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string UserId, CancellationToken cancellationToken = default)
    {
        // Check if the current user has already voted in this poll (PollId)
        var hasVote = await _context.Votes
            .AnyAsync(x => x.PollId == pollId && x.UserId == UserId, cancellationToken: cancellationToken);

        if (hasVote)
            return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DuplicatedVote);

        // Validate that the poll exists, is published, and currently within its active date range
        var pollIsExists = await _context.Polls
            .AnyAsync(x => x.Id == pollId
                   && x.IsPublished
                   && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                   && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        if (!pollIsExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);


        var questions = await _context.Questions
              .Where(x => x.PollId == pollId && x.IsActive)
              .Include(x => x.Answers)
              .Select(q => new QuestionResponse(
                  q.Id,
                  q.Content,
                  q.Answers
                      .Where(a => a.IsActive)
                      .Select(a => new AnswerResponse(a.Id, a.Content))
              ))
              .AsNoTracking()
              .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }


    public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
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
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var questionIsExists = await _context.Questions.AnyAsync(x => x.Content == request.Content && x.Id == pollId, cancellationToken);

        if (questionIsExists)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        //after checking about pollid and duplicated question content

        var question = request.Adapt<Question>();
        question.PollId = pollId;

        // request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));//I'm used  mapping

        await _context.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());

    }

    public async Task<Result> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .Include(x => x.Answers)
            .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        if (!question.IsActive)
            return Result.Failure(QuestionErrors.QuestionNotActive);

        var QuestionIsExists = await _context.Questions
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


        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


}
