using ChatBox.API.Domains.AIChats.Dtos.AI;

namespace ChatBox.API.Domains.AIChats.Dtos.Sessions;

public record CreateSessionResponseDto(Guid Id, string Name, AIMessageResponseDto InitialMessage);