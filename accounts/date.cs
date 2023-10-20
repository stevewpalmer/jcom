using System.Globalization;
using System.Text.Json.Serialization;

namespace JAccounts;

public class TDate : IComparable {

    /// <summary>
    /// Default constructor for TDate object that initialises
    /// with the current date.
    /// </summary>
    public TDate() {
        Year = DateTime.Now.Year;
        Month = DateTime.Now.Month;
        Day = DateTime.Now.Day;
    }

    /// <summary>
    /// Default constructor for TDate object that initialises
    /// with the specified date.
    /// </summary>
    public TDate(int year, int month, int day) {
        Year = year;
        Month = month;
        Day = day;
    }

    /// <summary>
    /// Given a month index in the range 1 to 12, return the name of
    /// the month in the current culture.
    /// </summary>
    /// <param name="month">Index of month</param>
    /// <returns>Name of month</returns>
    public static string MonthName(int month) => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

    /// <summary>
    /// Given a month index in the range 1 to 12, return the short name of
    /// the month in the current culture.
    /// </summary>
    /// <param name="month">Index of month</param>
    /// <returns>Short name of month</returns>
    public static string ShortMonthName(int month) => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);

    /// <summary>
    /// Year
    /// </summary>
    [JsonInclude]
    public int Year { get; set; }

    /// <summary>
    /// Month
    /// </summary>
    [JsonInclude]
    public int Month { get; set; }

    /// <summary>
    /// Day
    /// </summary>
    [JsonInclude]
    public int Day { get; set; }

    /// <summary>
    /// Add comparer for sorting.
    /// </summary>
    /// <param name="otherDate">Date to compare against</param>
    /// <returns></returns>
    public int CompareTo(object? otherDate) {
        return otherDate is TDate other ? new DateTime(Year, Month, Day)
            .CompareTo(new DateTime(other.Year, other.Month, other.Day)) : -1;
    }
}