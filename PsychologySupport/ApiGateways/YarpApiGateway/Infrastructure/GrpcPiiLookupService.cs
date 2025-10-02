using YarpApiGateway.Features.TokenExchange.Contracts;

namespace YarpApiGateway.Infrastructure;

using Pii.API.Protos;
using Grpc.Core;

public class GrpcPiiLookupService(PiiService.PiiServiceClient piiClient, ILogger<GrpcPiiLookupService> logger) : IPiiLookupService
{
    public async Task<string?> ResolveAliasIdBySubjectRefAsync(string subjectRef)
    {
        try
        {
            var request = new ResolveAliasIdBySubjectRefRequest { SubjectRef = subjectRef };
            
            var response = await piiClient.ResolveAliasIdBySubjectRefAsync(request);
            
            logger.LogInformation("*** Resolved AliasId for SubjectRef {SubjectRef}: {AliasId}", subjectRef, response.AliasId ?? "null");
            
            
            return string.IsNullOrEmpty(response.AliasId) ? null : response.AliasId;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<string?> ResolvePatientIdBySubjectRefAsync(string subjectRef)
    {
        try
        {
            var request = new ResolvePatientIdBySubjectRefRequest { SubjectRef = subjectRef };
            
            var response = await piiClient.ResolvePatientIdBySubjectRefAsync(request);
            
            return string.IsNullOrEmpty(response.PatientId) ? null : response.PatientId;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}