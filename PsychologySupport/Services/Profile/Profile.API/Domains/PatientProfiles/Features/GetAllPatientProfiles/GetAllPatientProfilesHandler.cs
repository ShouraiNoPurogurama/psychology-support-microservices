using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.Data.Pii;
using Profile.API.Data.Public;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Enum;

namespace Profile.API.Domains.PatientProfiles.Features.GetAllPatientProfiles;

public record GetAllPatientProfilesQuery(
    int PageIndex,
    int PageSize,
    string? Search = "", // FullName,PhoneNumber
    string? SortBy = "fullname", // sort fullname or createdat(MedicalRecord)
    string? SortOrder = "asc", // asc or desc
    UserGender? Gender = null, // filter
    MedicalRecordStatus? MedicalRecordStatus = null, // filter Processing,Done
    PersonalityTrait? PersonalityTrait = null) : IQuery<GetAllPatientProfilesResult>;

public record GetAllPatientProfilesResult(PaginatedResult<GetPatientProfileDto> PaginatedResult);

public class GetAllPatientProfilesHandler : IQueryHandler<GetAllPatientProfilesQuery, GetAllPatientProfilesResult>
{
    private readonly ProfileDbContext _context;
    private readonly PiiDbContext _piiDbContext;

    public GetAllPatientProfilesHandler(ProfileDbContext context, PiiDbContext piiDbContext)
    {
        _context = context;
        _piiDbContext = piiDbContext;
    }

    public async Task<GetAllPatientProfilesResult> Handle(GetAllPatientProfilesQuery request, CancellationToken cancellationToken)
    {
        //TODO tí quay lại sửa
        
        // var query =
        //     from patient in _context.PatientProfiles
        //     // join person in _piiDbContext.PersonProfiles
        //     //     on patient.UserId equals person.UserId
        //     select new
        //     {
        //         Patient = patient,
        //         Person = person,
        //         Job = patient.Job,
        //         MedicalRecords = patient.MedicalRecords,
        //         MedicalHistory = patient.MedicalHistory
        //     };

        // //TODO quay lại sửa sau
        // //====== FILTERS ======
        // if (request.Gender is not null)
        //     // query = query.Where(x => x.Patient.Gender == request.Gender);
        //
        // if (request.PersonalityTrait is not null)
        //     query = query.Where(x => x.Patient.PersonalityTraits == request.PersonalityTrait);
        //
        // if (request.MedicalRecordStatus is not null)
        //     query = query.Where(x => x.MedicalRecords.Any(m => m.Status == request.MedicalRecordStatus));
        //
        // if (!string.IsNullOrWhiteSpace(request.Search))
        // {
        //     var search = request.Search.Trim().ToLower();
        //     query = query.Where(x =>
        //         x.Person.FullName!.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
        //         x.Person.ContactInfo.PhoneNumber!.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        // }
        //
        // // ====== SORT ======
        // if (!string.IsNullOrEmpty(request.SortBy))
        // {
        //     if (request.SortBy.ToLower() == "fullname")
        //     {
        //         query = request.SortOrder?.ToLower() == "desc"
        //             ? query.OrderByDescending(x => x.Person.FullName)
        //             : query.OrderBy(x => x.Person.FullName);
        //     }
        //     else if (request.SortBy.ToLower() == "createdat")
        //     {
        //         query = request.SortOrder?.ToLower() == "desc"
        //             ? query.OrderByDescending(x => x.MedicalRecords.Min(m => m.CreatedAt))
        //             : query.OrderBy(x => x.MedicalRecords.Min(m => m.CreatedAt));
        //     }
        // }
        //
        // // ====== PAGINATION ======
        // var totalCount = await query.CountAsync(cancellationToken);
        //
        // var pageItems = await query
        //     .Skip((request.PageIndex - 1) * request.PageSize)
        //     .Take(request.PageSize)
        //     .ToListAsync(cancellationToken);
        //
        // // ====== MAPPING ======
        // var dtos = pageItems.Select(x => new GetPatientProfileDto(
        //     UserId : x.Patient.UserId,
        //     Id : x.Patient.Id,
        //     FullName : x.Person.FullName!,
        //     Gender : x.Person.Gender,
        //     BirthDate : x.Person.BirthDate!.Value,
        //     ContactInfo : x.Person.ContactInfo,
        //     MedicalRecords : x.Patient.MedicalRecords.Adapt<List<MedicalRecordDto>>(),
        //     MedicalHistory : x.Patient.MedicalHistory.Adapt<MedicalHistoryDto>(),
        //     Job : x.Patient.Job,
        //     Allergies: x.Patient.Allergies,
        //     PersonalityTraits: x.Patient.PersonalityTraits
        //     ));
        //
        // var result = new PaginatedResult<GetPatientProfileDto>(request.PageIndex, request.PageSize, totalCount, dtos);
        //
        // return new GetAllPatientProfilesResult(result);

        return null;
    }
}