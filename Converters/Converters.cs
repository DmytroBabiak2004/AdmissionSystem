using AdmissionSystem.Enums;
using AdmissionSystem.Models;
using AdmissionSystem.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AdmissionSystem.Converters;

public class ApplicationStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Draft => new SolidColorBrush(Color.FromRgb(189, 189, 189)),
                ApplicationStatus.Submitted => new SolidColorBrush(Color.FromRgb(100, 181, 246)),
                ApplicationStatus.UnderReview => new SolidColorBrush(Color.FromRgb(255, 213, 79)),
                ApplicationStatus.NeedsInfo => new SolidColorBrush(Color.FromRgb(255, 171, 64)),
                ApplicationStatus.DocumentsConfirmed => new SolidColorBrush(Color.FromRgb(77, 182, 172)),
                ApplicationStatus.Rejected => new SolidColorBrush(Color.FromRgb(229, 115, 115)),
                ApplicationStatus.AdmittedToCompetition => new SolidColorBrush(Color.FromRgb(129, 199, 132)),
                ApplicationStatus.RecommendedForEnrollment => new SolidColorBrush(Color.FromRgb(67, 160, 71)),
                ApplicationStatus.Enrolled => new SolidColorBrush(Color.FromRgb(27, 94, 32)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ApplicationStatusToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Enrolled => new SolidColorBrush(Colors.White),
                ApplicationStatus.RecommendedForEnrollment => new SolidColorBrush(Colors.White),
                _ => new SolidColorBrush(Colors.Black)
            };
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ApplicationStatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is ApplicationStatus s ? s.ToDisplayString() : value?.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class FormOfEducationToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is FormOfEducation f ? f.ToDisplayString() : value?.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class EducationBasisToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is EducationBasis b ? b.ToDisplayString() : value?.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class DocumentTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is DocumentType d ? d.ToDisplayString() : value?.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVerifiedTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? "✓ Підтверджено" : "✗ Не підтверджено";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVerifiedColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b
            ? new SolidColorBrush(Color.FromRgb(67, 160, 71))
            : new SolidColorBrush(Color.FromRgb(229, 115, 115));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RecommendationToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Бюджет" => new SolidColorBrush(Color.FromRgb(67, 160, 71)),
            "Контракт" => new SolidColorBrush(Color.FromRgb(100, 181, 246)),
            "Резерв" => new SolidColorBrush(Color.FromRgb(255, 213, 79)),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ActiveToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? "Активна" : "Неактивна";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ActiveToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b
            ? new SolidColorBrush(Color.FromRgb(67, 160, 71))
            : new SolidColorBrush(Color.FromRgb(229, 115, 115));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class DocStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ApplicantDocument doc)
        {
            if (!doc.IsProvided)
                return "Не подано";

            if (doc.IsVerified)
                return "Підтверджено";

            return "Подано";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
public class EnumToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return (int)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Binding.DoNothing;

        return Enum.ToObject(targetType, value);
    }
}