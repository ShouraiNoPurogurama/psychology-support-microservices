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
        // var userName = principal.GetUserName();
        var receiverId = Context.GetHttpContext()?.Request.Query["receiverId"].ToString();
        var connectionId = Context.ConnectionId;

        if (string.IsNullOrEmpty(userId)) return;

        var currentUser = await getUserDataClient.GetResponse<GetUserDataResponse>(new GetUserDataRequest(userId))
            .ContinueWith(u => u.Result.Message);

        if (_onlineUsers.ContainsKey(userId))
        {
            _onlineUsers[userId].ConnectionId = connectionId;
        }
        else
        {
            var user = new OnlineUserDto
            {
                Id = Guid.Parse(userId),
                ConnectionId = connectionId,
                FullName = currentUser.FullName,
                Role = roles
            };

            _onlineUsers.TryAdd(userId, user);
            // await Clients.AllExcept(connectionId).SendAsync("NotifyUserConnected", user);

            // Chỉ thông báo cho bác sĩ nếu người kết nối là bệnh nhân
            if (roles.Contains("User") && !string.IsNullOrEmpty(receiverId))
            {
                await Clients.Client(_onlineUsers[receiverId].ConnectionId).SendAsync("NotifyUserConnected", user);
            }
        }

        if (!string.IsNullOrEmpty(receiverId))
        {
            await LoadMessages(receiverId);
        }

        // Add user to SignalR user group
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await Clients.All.SendAsync("Users", await GetAllUsers());
    }

    private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
    {
        var principal = GetUserFromToken();
        if (principal == null) return Enumerable.Empty<OnlineUserDto>();

        var currentUserId = principal.GetUserId();
        var currentUserRole = principal.FindFirst(ClaimTypes.Role)?.Value;
        var onlineUsersSet = new HashSet<string>(_onlineUsers.Keys);

        GetOnlineUsersDataResponse? users = null;

        if (currentUserRole.Contains("Doctor"))
        {
            var onlinePatientIdsOfDoctor = await dbContext.DoctorPatients
                .Where(d => d.DoctorUserId == Guid.Parse(currentUserId))
                .GroupBy(d => d.PatientUserId)
                .Select(d => d.First())
                .Select(d => d.PatientUserId)
                .ToListAsync();

            users = await getAllUsersDataClient
                .GetResponse<GetOnlineUsersDataResponse>(new GetAllUsersDataRequest(onlinePatientIdsOfDoctor))
                .ContinueWith(u => u.Result.Message);

            var onlineUsers = users.Users
                .Select(u => new OnlineUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    IsOnline = onlineUsersSet.Contains(u.Id.ToString()),
                    UnreadCount = dbContext.Messages.Count(
                        x => x.ReceiverUserId.ToString() == currentUserId && x.SenderUserId == u.Id && !x.IsRead)
                })
                .OrderByDescending(a => a.IsOnline)
                .ThenByDescending(a => a.UnreadCount)
                .ToList()
                ;

            return onlineUsers;
        }
        else if (currentUserRole.Contains("User"))
        {
            var onlineDoctorIdsOfPatient = await dbContext.DoctorPatients
                .Where(d => d.PatientUserId == Guid.Parse(currentUserId))
                .GroupBy(d => d.DoctorUserId)
                .Select(d => d.First())
                .Select(d => d.DoctorUserId)
                .ToListAsync();

            users = await getAllUsersDataClient
                .GetResponse<GetOnlineUsersDataResponse>(new GetAllUsersDataRequest(onlineDoctorIdsOfPatient))
                .ContinueWith(u => u.Result.Message);

            var onlineUsers = users.Users
                .Select(u => new OnlineUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    IsOnline = onlineUsersSet.Contains(u.Id.ToString()),
                    UnreadCount = dbContext.Messages.Count(
                        x => x.ReceiverUserId.ToString() == currentUserId && x.SenderUserId == u.Id && !x.IsRead)
                })
                .OrderByDescending(a => a.IsOnline)
                .ThenByDescending(a => a.UnreadCount)
                .ToList()
                ;

            return onlineUsers;
        }

        return new List<OnlineUserDto>();
    }

    public async Task SendMessage(MessageRequestDto message)
    {
        var principal = GetUserFromToken();
        if (principal == null) return;

        var senderId = principal.GetUserId();
        if (string.IsNullOrEmpty(senderId)) return;

        var recipientId = message.ReceiverId;

        var newMsg = new Message()
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
    }

    public async Task NotifyTyping(string recipientId)
    {
        var principal = GetUserFromToken();
        if (principal == null) return;

        var senderId = principal.GetUserId();
        if (string.IsNullOrEmpty(senderId)) return;

        var connectionId = _onlineUsers.Values
            .FirstOrDefault(x => x.Id.ToString() == recipientId)
            ?.ConnectionId;

        if (connectionId == null) return;
        await Clients.Client(connectionId).SendAsync("NotifyTypingToUser", senderId);
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
            .Where(x => (x.ReceiverUserId.ToString() == currentUserId &&
                         x.SenderUserId.ToString() == recipientId) ||
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

        // Mark messages as read
        foreach (var message in messages)
        {
            var msg = await dbContext.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);
            if (msg != null && msg.ReceiverUserId.ToString() == currentUserId)
            {
                msg.IsRead = true;
                dbContext.Messages.Update(msg);
                await dbContext.SaveChangesAsync();
            }
        }

        await Clients.Client(_onlineUsers[currentUserId].ConnectionId).SendAsync("ReceiveMessageList", messages);
    }

    private ClaimsPrincipal? GetUserFromToken()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null) return null;

        var token = httpContext.Request.Query["access_token"].ToString().Replace("Bearer ", "");
        return ClaimsPrincipalExtensions.GetClaimsFromToken(token);
    }
}