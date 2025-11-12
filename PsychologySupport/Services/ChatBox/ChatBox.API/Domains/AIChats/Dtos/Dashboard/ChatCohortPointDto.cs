namespace ChatBox.API.Domains.AIChats.Dtos.Dashboard;

public sealed record ChatCohortPointDto(
    int Week,          // 0 = Week0
    int Active,        // số user còn active tuần k
    double Percent     // Active / CohortSize * 100, làm tròn 1 chữ số
);