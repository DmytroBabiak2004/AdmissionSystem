using AdmissionSystem.Enums;

namespace AdmissionSystem.Services;

public static class EnumExtensions
{
    public static string ToDisplayString(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.Draft => "Чернетка",
        ApplicationStatus.Submitted => "Подано",
        ApplicationStatus.UnderReview => "На перевірці",
        ApplicationStatus.NeedsInfo => "Потрібне уточнення",
        ApplicationStatus.DocumentsConfirmed => "Документи підтверджено",
        ApplicationStatus.Rejected => "Відхилено",
        ApplicationStatus.AdmittedToCompetition => "Допущено до конкурсу",
        ApplicationStatus.RecommendedForEnrollment => "Рекомендовано до зарахування",
        ApplicationStatus.Enrolled => "Зараховано",
        _ => status.ToString()
    };

    public static string ToDisplayString(this FormOfEducation form) => form switch
    {
        FormOfEducation.FullTime => "Денна",
        FormOfEducation.PartTime => "Заочна",
        _ => form.ToString()
    };

    public static string ToDisplayString(this EducationBasis basis) => basis switch
    {
        EducationBasis.Budget => "Бюджет",
        EducationBasis.Contract => "Контракт",
        _ => basis.ToString()
    };

    public static string ToDisplayString(this DocumentType type) => type switch
    {
        DocumentType.Passport => "Паспорт",
        DocumentType.TaxCode => "ІПН",
        DocumentType.EducationDocument => "Документ про освіту",
        DocumentType.Photo => "Фото",
        DocumentType.NmtCertificate => "Сертифікат НМТ",
        DocumentType.MotivationLetter => "Мотиваційний лист",
        DocumentType.MilitaryDocument => "Військово-обліковий документ",
        _ => type.ToString()
    };

    public static string ToDisplayString(this UserRole role) => role switch
    {
        UserRole.Administrator => "Адміністратор",
        UserRole.Operator => "Оператор",
        UserRole.Reviewer => "Перевіряючий",
        _ => role.ToString()
    };
}
