using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AElf.OpenTelemetry;
using AElf.OpenTelemetry.ExecutionTime;
using Microsoft.Extensions.Logging;
using AISmart.Application.Grains.Authors;
using Orleans;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AISmart.Authors;

[AggregateExecutionTime]
public class AuthorAppService : ApplicationService, IAuthorAppService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly AuthorManager _authorManager;
    private readonly IClusterClient _clusterClient;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<AuthorAppService> _logger;

    public AuthorAppService(
        IAuthorRepository authorRepository,
        AuthorManager authorManager,
        IClusterClient clusterClient,
        IInstrumentationProvider instrumentationProvider,
        ILogger<AuthorAppService> logger)
    {
        _authorRepository = authorRepository;
        _authorManager = authorManager;
        _clusterClient = clusterClient;
        _activitySource = instrumentationProvider.ActivitySource;
        _logger = logger;
    }

    public async Task<AuthorDto> GetAsync(Guid id)
    {
        var author = await _authorRepository.GetAsync(id);
        return ObjectMapper.Map<Author, AuthorDto>(author);
    }
    
    public virtual async Task<PagedResultDto<AuthorDto>> GetListAsync(GetAuthorListDto input)
    {
        using var myActivity = _activitySource.StartActivity($"{nameof(AuthorAppService)}.GetListAsync");
        
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(Author.Name);
        }
        
        _logger.LogInformation("GetListAsync called with input: {@Input}", input);

        var authors = await _authorRepository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter
        );

        var totalCount = input.Filter == null
            ? await _authorRepository.CountAsync()
            : await _authorRepository.CountAsync(
                author => author.Name.Contains(input.Filter));

        return new PagedResultDto<AuthorDto>(
            totalCount,
            ObjectMapper.Map<List<Author>, List<AuthorDto>>(authors)
        );
    }

    public async Task<AuthorDto> CreateAsync(CreateAuthorDto input)
    {
        var author = await _authorManager.CreateAsync(
            input.Name,
            input.BirthDate,
            input.ShortBio
        );

        await _authorRepository.InsertAsync(author);

        return ObjectMapper.Map<Author, AuthorDto>(author);
    }
    
    public async Task UpdateAsync(Guid id, UpdateAuthorDto input)
    {
        var author = await _authorRepository.GetAsync(id);

        if (author.Name != input.Name)
        {
            await _authorManager.ChangeNameAsync(author, input.Name);
        }

        author.BirthDate = input.BirthDate;

        var messageValidatorGrain = _clusterClient.GetGrain<IMessageValidator>(author.Id);
        var isOffensive = await messageValidatorGrain.IsOffensive(input.ShortBio);
        
        if (isOffensive)
        {
            throw new UserFriendlyException("ShortBio is offensive!");
        }
        
        author.ShortBio = input.ShortBio;

        // immediately save changes to database
        // reference: https://abp.io/docs/8.2/framework/infrastructure/concurrency-check?_redirected=B8ABF606AA1BDF5C629883DF1061649A
        await _authorRepository.UpdateAsync(author, autoSave:true);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        await _authorRepository.DeleteAsync(id);
    }

}
