using System.ComponentModel.DataAnnotations;

namespace StudyFlow.Enums;
public enum Category
{
    [Display(Name = "University Work")]
    UniversityWork,
    [Display(Name = "Personal Development")]
    Personal,
    [Display(Name = "Projects")]
    Projects,
    [Display(Name = "Programming")]
    Programming
}