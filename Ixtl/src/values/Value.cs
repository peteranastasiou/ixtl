
namespace Ixtl;

public enum ValueType {
  BOOL,
  I32,
  U32,
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

  public record I32(int Data) : Value {
    public override ValueType Type => ValueType.I32;
    public override string ToString() => Data.ToString();
  }

  public record U32(uint Data) : Value {
    public override ValueType Type => ValueType.U32;
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

  public static Str AsStr(Value v) {
    return (Str)v;
  }

  public static Flt AsFlt(Value v) {
    return (Flt)v;
  }
}

