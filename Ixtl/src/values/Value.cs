
namespace Ixtl;

public enum ValueType {
  BOOL,
  INT,
  FLT,
  STR,
  VOID,
  FN
}

public abstract record Value {
  public abstract ValueType Type { get; }

  /**
   * Concrete instances
   */
  public record Bool(bool Data) : Value {
    public override ValueType Type => ValueType.BOOL;
    public override string ToString() => Data.ToString();
  }

  public record Int(int Data) : Value {
    public override ValueType Type => ValueType.INT;
    public override string ToString() => Data.ToString();
  }

  public record Flt(double Data) : Value {
    public override ValueType Type => ValueType.FLT;
    public override string ToString() => $"{Data}f";
  }

  public record Str(string Data) : Value {
    public override ValueType Type => ValueType.STR;
    public override string ToString() => Data;
  }

  public record Void() : Value {
    public override ValueType Type => ValueType.VOID;
    public override string ToString() => $"<void>";
  }

  public static Int AsInt(Value v) {
    return (Int)v;
  }

  public static Str AsStr(Value v) {
    return (Str)v;
  }

  public static Flt AsFlt(Value v) {
    // Conversion here???
    return (Flt)v;
  }

  public static string ValueTypeToStr(ValueType? v) {
    return v switch {
      ValueType.BOOL => "bool",
      ValueType.INT => "int",
      ValueType.FLT => "flt",
      ValueType.STR => "str",
      ValueType.VOID => "void",
      ValueType.FN => "function",
      null => "untyped",
      _ => "invalid",
    };
  }
}

