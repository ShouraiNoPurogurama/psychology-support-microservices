// using BuildingBlocks.CQRS;
// using LifeStyles.API.Data;
// using LifeStyles.API.Dtos;
//
// namespace LifeStyles.API.Features.DASS21LifestyleInput.BuildDASS21LifestyleInput;
//
// public record BuildDASS21LifestyleInputQuery(
//     Guid PatientProfileId
// ) : IQuery<BuildDASS21LifestyleInputResult>;
//
// public record BuildDASS21LifestyleInputResult(
//     Guid PatientProfileId,
//     List<PatientImprovementGoalDto> Goals,
//     
// );
//
// public class BuildDASS21LifestyleInputHandler : IQueryHandler<BuildDASS21LifestyleInputQuery, BuildDASS21LifestyleInputResult>
// {
//     private readonly LifeStylesDbContext _context;
//     
//     public Task<BuildDASS21LifestyleInputResult> Handle(BuildDASS21LifestyleInputQuery request, CancellationToken cancellationToken)
//     {
//         throw new NotImplementedException();
//     }
// }