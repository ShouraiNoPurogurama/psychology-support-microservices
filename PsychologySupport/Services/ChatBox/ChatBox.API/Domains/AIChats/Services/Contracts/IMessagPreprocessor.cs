using ChatBox.API.Models;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IMessagPreprocessor
{
    string FormatUserMessageBlock(string userMessage);
}