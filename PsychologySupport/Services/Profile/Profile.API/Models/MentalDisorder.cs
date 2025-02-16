namespace Profile.API.Models;

public class MentalDisorder
{
    
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<SpecificMentalDisorder> SpecificMentalDisorders { get; set; }
}