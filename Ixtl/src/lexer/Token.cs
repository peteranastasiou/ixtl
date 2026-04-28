
namespace Ixtl;

public enum TokenType {
  // Single-character tokens:
  LEFT_PAREN, RIGHT_PAREN,      // ()
  LEFT_BRACE, RIGHT_BRACE,      // {}
  LEFT_BRACKET, RIGHT_BRACKET,  // []
  COMMA, MINUS, PLUS,
  SEMICOLON, SLASH, STAR,
  DOT,
  // One or more character tokens:
  BANG, BANG_EQUAL,
  EQUAL, EQUAL_EQUAL,
  GREATER, GREATER_EQUAL,
  LESS, LESS_EQUAL,
  // Names:
  IDENTIFIER,
  // Values:
  STR_VALUE, INT_VALUE, FLT_VALUE, // TODO
  TRUE, FALSE,
  // Operations:
  AND, OR,
  // Structure keywords:
  IF, ELSE,
  FOR, WHILE,
  RETURN,
  // Type keywords:
  BOOL, STR, INT, FLT, FN, VOID,
  // Special tokens:
  ERROR, EOF,

  // Deprecated, use native funcs:
  PRINT
}

public readonly struct Token {
  public readonly TokenType Type;
  public readonly int Line;
  public readonly int Col;
  public readonly string? Str;

  public Token() {
    Str = null;
  }

  public Token(TokenType type, int line, int col, string? str = null) {
    Type = type;
    Line = line;
    Col = col;
    Str = str;
  }

  public override string ToString() {
    return $"Token: {Type} {Str}";
  }
}
