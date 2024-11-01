namespace JCalc;

public enum CellType {

    /// <summary>
    /// Empty cell
    /// </summary>
    NONE,

    /// <summary>
    /// Number
    /// </summary>
    NUMBER,

    /// <summary>
    /// Text
    /// </summary>
    TEXT
}

public enum CellAlignment {

    /// <summary>
    /// Numbers right aligned, text left aligned
    /// </summary>
    GENERAL,

    /// <summary>
    /// Left alignment
    /// </summary>
    LEFT,

    /// <summary>
    /// Right alignment
    /// </summary>
    RIGHT,

    /// <summary>
    /// Centered
    /// </summary>
    CENTRE
}

public enum CellFormat {
    GENERAL,
    CONTINUOUS,
    EXPONENTIAL,
    FIXED,
    INTEGER,
    CURRENCY,
    BAR,
    PERCENT
}

public class CellValue {

    /// <summary>
    /// Cell type
    /// </summary>
    public CellType Type { get; set; }

    /// <summary>
    /// String representation of content
    /// </summary>
    public string StringValue { get; set; } = string.Empty;
}