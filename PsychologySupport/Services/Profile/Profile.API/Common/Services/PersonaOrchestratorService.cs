using Grpc.Core;
using Profile.API.Domains.Pii.Services;
using Profile.API.Domains.Public.PatientProfiles.Services;
using Profile.API.Protos;
using Google.Protobuf.WellKnownTypes; 

namespace Profile.API.Common.Services;

/// <summary>
/// Service gRPC (Facade) điều phối việc lấy dữ liệu PII và Public Profile.
/// </summary>
public class PersonaOrchestratorService(
    PatientProfileLocalService profileLocalService,
    PiiLocalService piiLocalService,
    ILogger<PersonaOrchestratorService> logger)
    : Protos.PersonaOrchestratorService.PersonaOrchestratorServiceBase
{
    /// <summary>
    /// Lấy thông tin tóm tắt (Snapshot) của một persona (user)
    /// </summary>
    public override async Task<GetPersonaSnapshotResponse> GetPersonaSnapshot(
        GetPersonaSnapshotRequest request,
        ServerCallContext context)
    {
        var subjectRef = request.SubjectRef;
        if (string.IsNullOrEmpty(subjectRef))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Alias ID không được để trống."));
        }

        logger.LogInformation("Bắt đầu lấy Persona Snapshot cho Subject Ref: {AliasId}", subjectRef);

        try
        {
            // 1. Gọi cả hai service CÙNG LÚC (song song) để tiết kiệm thời gian
            var profile = await profileLocalService.GetPublicProfileBySubjectRefAsync(subjectRef, context.CancellationToken);
            var pii = await piiLocalService.GetPiiProfileBySubjectRefAsync(subjectRef, context.CancellationToken);


            // 2. Tạo response
            var response = new GetPersonaSnapshotResponse();

            // 3. Map dữ liệu từ Public Profile (Job)
            if (profile.JobTitle != null)
            {
                response.JobTitle = profile.JobTitle;
            }

            // 4. Map dữ liệu từ PII (BirthDate, Gender)
            // Phải chuyển sang UTC để Timestamp hoạt động chính xác
            var utcBirthDate = DateTime.SpecifyKind(
                pii.BirthDate.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );
            response.BirthDate = Timestamp.FromDateTime(utcBirthDate);

            // Map Gender (string -> enum Gender của proto)
            response.Gender = MapGender(pii.Gender);

            logger.LogInformation("Lấy Persona Snapshot thành công cho AliasId: {AliasId}", subjectRef);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi đang lấy Persona Snapshot cho AliasId: {AliasId}", subjectRef);
            throw new RpcException(new Status(StatusCode.Internal, $"Lỗi máy chủ khi xử lý: {ex.Message}"));
        }
    }

    /// <summary>
    /// Hàm helper để map string Gender (từ PiiLocalService)
    /// sang enum Gender (của PersonaService.proto)
    /// </summary>
    private Gender MapGender(string? piiGenderString)
    {
        // piiGenderString là kết quả của .ToString() từ enum BuildingBlocks.Enums.UserGender
        // (Ví dụ: "Male", "Female", "Else", "NotSet")
        return piiGenderString switch
        {
            "Male" => Gender.Male,
            "Female" => Gender.Female,
            "Else" => Gender.Other,

            _ => Gender.Unspecified
        };
    }
}