namespace AdmissionSystem.Enums;

public enum ApplicationStatus
{
    Draft = 0,           // Чернетка
    Submitted = 1,       // Подано
    UnderReview = 2,     // На перевірці
    NeedsInfo = 3,       // Потрібне уточнення
    DocumentsConfirmed = 4, // Документи підтверджено
    Rejected = 5,        // Відхилено
    AdmittedToCompetition = 6, // Допущено до конкурсу
    RecommendedForEnrollment = 7, // Рекомендовано до зарахування
    Enrolled = 8         // Зараховано
}
