﻿namespace ChatBox.API.Domains.Chatboxes.Dtos;

public class OnlineUserDto
{
    public Guid Id { get; set; }
    public string ConnectionId { get; set; }
    public string FullName { get; set; }
    
    public string Role { get; set; }
    public bool IsOnline { get; set; }
    public int UnreadCount { get; set; }
}