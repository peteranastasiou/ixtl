
namespace Ixtl;

public enum DeclarationType {
  FUNCTION,
  GLOBAL_VAR
};

public struct Declaration {
  public short Id;
  public string Name;
  public DeclarationType DeclType;
  public List<ValueType>? InputTypes;
  public ValueType? OutputType;

  public override readonly string ToString() {
    return DeclType switch {
      DeclarationType.FUNCTION => $"Declaration #{Id} Function: {Name}, Params: [{string.Join(", ", InputTypes!)}], Out: {OutputType}",
      DeclarationType.GLOBAL_VAR => $"Declaration #{Id} Global Var: {Name}, Type: {OutputType}",
      _ => throw new NotImplementedException(),
    };
  }
}
