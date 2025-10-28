namespace ChatBox.API.Domains.AIChats.Enums;

public enum RouterIntent
{
    DIRECT_ANSWER,        // trả lời ngay
    SMALL_TALK,           // xã giao
    SAFETY_REFUSAL,       // từ chối an toàn
    RAG_PERSONAL_MEMORY,  // RAG vào user memory (cá nhân hoá)
    RAG_TEAM_KNOWLEDGE    // RAG vào knowledge nội bộ/dự án
}