namespace AdmissionSystem.Enums;

public enum FormOfEducation
{
    FullTime = 0,   // Денна
    PartTime = 1    // Заочна
}

public enum EducationBasis
{
    Budget = 0,   // Бюджет
    Contract = 1  // Контракт
}

public enum DocumentType
{
    Passport = 0,              // Паспорт
    TaxCode = 1,               // ІПН
    EducationDocument = 2,     // Документ про освіту
    Photo = 3,                 // Фото
    NmtCertificate = 4,        // Сертифікат НМТ
    MotivationLetter = 5,      // Мотиваційний лист
    MilitaryDocument = 6       // Військово-обліковий документ
}

public enum DocumentStatus
{
    Missing = 0,         // не подано
    Provided = 1,        // подано
    UnderReview = 2,     // на перевірці
    Verified = 3,        // підтверджено
    Rejected = 4,        // відхилено
    NeedClarification = 5 // треба уточнення
}

public enum UserRole
{
    Administrator = 0,
    Operator = 1,
    Reviewer = 2
}
