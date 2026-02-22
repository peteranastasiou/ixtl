
namespace Ixtl;

public enum DeclarationType {
  FUNCTION,
  GLOBAL_VAR
};

public struct Declaration {
  public string Name;
  public DeclarationType DeclType;
  public List<ValueType>? InputTypes;
  public ValueType? OutputType;

  public override readonly string ToString() {
    return DeclType switch {
      DeclarationType.FUNCTION => $"Declaration Function: {Name}, Params: [{string.Join(", ", InputTypes!)}], Out: {OutputType}",
      DeclarationType.GLOBAL_VAR => $"Declaration Global Var: {Name}, Type: {OutputType}",
      _ => throw new NotImplementedException(),
    };
  }
}
