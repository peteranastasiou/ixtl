
namespace Ixtl;

/**
 * Precedence defining order of operations
 */
public enum Precedence {
  NONE,
  ASSIGNMENT,  // =
  OR,          // or
  AND,         // and
  EQUALITY,    // == !=
  COMPARISON,  // < > <= >=
  TERM,        // + -
  FACTOR,      // * /
  UNARY,       // ! -
  CALL,        // . ()
  PRIMARY
}

public static class OperatorPrecedence {
  public static Precedence GetInfixPrecedence(TokenType type) {
    switch( type ) {
        case TokenType.LEFT_PAREN:
        case TokenType.LEFT_BRACKET:
            return Precedence.CALL;

        case TokenType.STAR:
        case TokenType.SLASH:
            return Precedence.FACTOR;

        case TokenType.PLUS:
        case TokenType.MINUS:
            return Precedence.TERM;

        case TokenType.GREATER:
        case TokenType.GREATER_EQUAL:
        case TokenType.LESS:
        case TokenType.LESS_EQUAL:
            return Precedence.COMPARISON;

        case TokenType.BANG_EQUAL:
        case TokenType.EQUAL_EQUAL:
            return Precedence.EQUALITY;

        case TokenType.AND:
            return Precedence.AND;

        case TokenType.OR:
            return Precedence.OR;

        default:
            return Precedence.NONE;
    }
  }
}