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