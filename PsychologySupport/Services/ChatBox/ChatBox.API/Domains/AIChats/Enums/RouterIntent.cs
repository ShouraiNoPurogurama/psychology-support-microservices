namespace ChatBox.API.Domains.AIChats.Enums;

public enum RouterIntent
{
    CONVERSATION,
    SAFETY_REFUSAL,       // từ chối an toàn
    RAG_PERSONAL_MEMORY,  // RAG vào user memory (cá nhân hoá)
    RAG_TEAM_KNOWLEDGE,    // RAG vào knowledge nội bộ/dự án
    TOOL_CALLING // gọi tính năng như lấy Url bài test, phát nhạc,...
}