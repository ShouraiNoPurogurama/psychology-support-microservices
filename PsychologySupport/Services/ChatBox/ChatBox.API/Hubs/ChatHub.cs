using System.Collections.Concurrent;
using System.Security.Claims;
using BuildingBlocks.Messaging.Events.Auth;
using ChatBox.API.Data;
using ChatBox.API.Dtos;
using ChatBox.API.Extensions;
using ChatBox.API.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Hubs;

public class ChatHub(
    ChatBoxDbContext dbContext,
    IRequestClient<GetUserDataRequest> getUserDataClient,
    IRequestClient<GetAllUsersDataRequest> getAllUsersDataClient) : Hub
{
    private static readonly ConcurrentDictionary<string, OnlineUserDto> _onlineUsers = new();

    public override async Task OnConnectedAsync()
    {
        var principal = GetUserFromToken();
        if (principal == null) return;

        var userId = principal.GetUserId();
        var roles = principal.GetUserRole();
        var receiverId = Context.GetHttpContext()?.Request.Query["receiverId"].ToString();
        var connectionId = Context.ConnectionId;

        if (string.IsNullOrEmpty(userId)) return;

        var currentUser = await getUserDataClient.GetResponse<GetUserDataResponse>(new GetUserDataRequest(userId))
            .ContinueWith(u => u.Result.Message);

        _onlineUsers.AddOrUpdate(userId,
            new OnlineUserDto
            {
                Id = Guid.Parse(userId),
                ConnectionId = connectionId,
                FullName = currentUser.FullName,
                Role = roles
            },
            (key, existingUser) =>
            {
                existingUser.ConnectionId = connectionId;
                return existingUser;
            });

        if (roles.Contains("User") && !string.IsNullOrEmpty(receiverId) && _onlineUsers.ContainsKey(receiverId))
        {
            await Clients.Client(_onlineUsers[receiverId].ConnectionId).SendAsync("NotifyUserConnected", _onlineUsers[userId]);
        }

        if (!string.IsNullOrEmpty(receiverId))
        {
            await LoadMessages(receiverId);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        await Clients.Group(userId).SendAsync("Users", await GetAllUsers());
    }

    private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
    {
        var principal = GetUserFromToken();
        if (principal == null) return Enumerable.Empty<OnlineUserDto>();

        var currentUserId = principal.GetUserId();
        var currentUserRole = principal.GetUserRole();
        var onlineUsersSet = new HashSet<string>(_onlineUsers.Keys);

        if (string.IsNullOrEmpty(currentUserId)) return Enumerable.Empty<OnlineUserDto>();

        var userIds = currentUserRole.Contains("Doctor")
            ? await dbContext.DoctorPatients.Where(d => d.DoctorUserId == Guid.Parse(currentUserId))
                .Select(d => d.PatientUserId)
                .Distinct()
                .ToListAsync()
            : await dbContext.DoctorPatients.Where(d => d.PatientUserId == Guid.Parse(currentUserId))
                .Select(d => d.DoctorUserId)
                .Distinct()
                .ToListAsync();

        var users = await getAllUsersDataClient
            .GetResponse<GetOnlineUsersDataResponse>(new GetAllUsersDataRequest(userIds))
            .ContinueWith(u => u.Result.Message);

        return users.Users.Select(u => new OnlineUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                IsOnline = onlineUsersSet.Contains(u.Id.ToString()),
                UnreadCount = dbContext.Messages.Count(x =>
                    x.ReceiverUserId.ToString() == currentUserId && x.SenderUserId == u.Id && !x.IsRead)
            })
            .OrderByDescending(a => a.IsOnline)
            .ThenByDescending(a => a.UnreadCount)
            .ToList();
    }

    public async Task SendMessage(MessageRequestDto message)
    {
        var principal = GetUserFromToken();
        if (principal == null) return;

        var senderId = principal.GetUserId();
        if (string.IsNullOrEmpty(senderId)) return;

        var recipientId = message.ReceiverId;

        var newMsg = new Message
        {
            SenderUserId = Guid.Parse(senderId),
            ReceiverUserId = recipientId,
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            Content = message.Content
        };

        dbContext.Messages.Add(newMsg);
        await dbContext.SaveChangesAsync();

        await Clients.Group(recipientId.ToString()).SendAsync("ReceiveNewMessage", newMsg);
        await Clients.Group(senderId).SendAsync("ReceiveNewMessage", newMsg);
    }

    public async Task NotifyTyping(string recipientId)
    {
        var principal = GetUserFromToken();
        if (principal == null) return;

        var senderId = principal.GetUserId();
        if (string.IsNullOrEmpty(senderId)) return;

        if (_onlineUsers.TryGetValue(recipientId, out var recipient))
        {
            await Clients.Client(recipient.ConnectionId).SendAsync("NotifyTypingToUser", senderId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var principal = GetUserFromToken();
        if (principal == null) return;

        var userId = principal.GetUserId();
        if (string.IsNullOrEmpty(userId)) return;

        _onlineUsers.TryRemove(userId, out _);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
    }

    public async Task LoadMessages(string recipientId, int pageNumber = 1)
    {
        var principal = GetUserFromToken();
        if (principal == null) return;

        var currentUserId = principal.GetUserId();
        if (string.IsNullOrEmpty(currentUserId)) return;

        int pageSize = 10;

        var messages = await dbContext.Messages
            .Where(x => (x.ReceiverUserId.ToString() == currentUserId && x.SenderUserId.ToString() == recipientId) ||
                        (x.SenderUserId.ToString() == currentUserId && x.ReceiverUserId.ToString() == recipientId))
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(x => x.CreatedDate)
            .Select(x => new MessageResponseDto
            {
                Id = x.Id,
                Content = x.Content,
                CreatedDate = x.CreatedDate,
                ReceiverId = x.ReceiverUserId,
                SenderId = x.SenderUserId
            })
            .ToListAsync();

        if (_onlineUsers.TryGetValue(currentUserId, out var user))
        {
            await Clients.Client(user.ConnectionId).SendAsync("ReceiveMessageList", messages);
        }

        await dbContext.Messages
            .Where(x => x.ReceiverUserId.ToString() == currentUserId && x.SenderUserId.ToString() == recipientId && !x.IsRead)
            .ForEachAsync(msg => msg.IsRead = true);

        await dbContext.SaveChangesAsync();
    }

    private ClaimsPrincipal? GetUserFromToken()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null) return null;

        var token = httpContext.Request.Query["access_token"].ToString().Replace("Bearer ", "");
        return ClaimsPrincipalExtensions.GetClaimsFromToken(token);
    }
}