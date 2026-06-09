using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class ChatMessageRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<ChatMessage>(context, mapper), IChatMessageRepository;
