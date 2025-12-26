using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRManagementSystem.ViewModels;

public class CreateEmployeeLevelViewModel
{
    public string Name { get; set; }
    public int MaxAnnualLeave { get; set; }

    public List<SelectListItem>? WeekDays { get; set; } =
        Enum.GetValues(typeof(DayOfWeek))
            .Cast<DayOfWeek>()
            .Select(d => new SelectListItem
            {
                Text = d.ToString(),
                Value = d.ToString()
            })
            .ToList();

    public List<string>? ExcludedDays { get; set; }
}
