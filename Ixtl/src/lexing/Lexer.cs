
namespace Ixtl;
using System.Text;

public class Lexer {
  readonly IInputStream InputStream;

  // Current token
  StringBuilder TokenStr;

  // Position
  int Col;
  int Line;

  public Lexer(IInputStream inputStream) {
    InputStream = inputStream;
    TokenStr = new StringBuilder();
    Line = 0;
    Col = 0;
  }

  public Token ScanToken() {
    // first, gobble up whitespace and comments:
    SkipWhitespace();

    // Reset token buffer
    TokenStr.Clear();

    if (IsAtEnd()) return MakeToken(TokenType.END);

    char c = NextChar();

    // TODO if (IsAlpha(c)) return MakeIdentifierOrKeywordToken();
    if (IsDigit(c)) return MakeNumberToken();

    // Symbols
    switch (c) {
      case '(': return MakeToken(TokenType.LEFT_PAREN);
      case ')': return MakeToken(TokenType.RIGHT_PAREN);
      case '{': return MakeToken(TokenType.LEFT_BRACE);
      case '}': return MakeToken(TokenType.RIGHT_BRACE);
      case '[': return MakeToken(TokenType.LEFT_BRACKET);
      case ']': return MakeToken(TokenType.RIGHT_BRACKET);
      case ',': return MakeToken(TokenType.COMMA);
      case '-': return MakeToken(TokenType.MINUS);
      case '+': return MakeToken(TokenType.PLUS);
      case ';': return MakeToken(TokenType.SEMICOLON);
      case '/': return MakeToken(TokenType.SLASH);
      case '*': return MakeToken(TokenType.STAR);
      case '.': return MakeToken(TokenType.DOT);
      case '!': return MakeToken(MatchNext('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
      case '=': return MakeToken(MatchNext('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
      case '<': return MakeToken(MatchNext('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
      case '>': return MakeToken(MatchNext('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
    }

    Console.WriteLine($"Unexpected char: '{c}'");
    return MakeErrorToken("Unexpected character.");
  }

  void SkipWhitespace() {
    while (true) {
      switch (Peek()) {
        case '\n':
          IncrementLine();
          goto case ' '; // Fall-through
        case ' ':
        case '\r':
        case '\t':
          NextChar();
          break;

        case '#':
          // comment out the rest of the line:
          while (Peek() != '\n' && !IsAtEnd()) {
            NextChar();
          }
          break;

        default:
          return;
      }
    }
  }

  /**
   * Lexer helpers
   */
  char Peek() {
    return InputStream.Peek();
  }

  bool IsAtEnd() {
    return InputStream.Peek() == '\0';
  }

  char NextChar() {
    char c = InputStream.Next();
    Col++;
    TokenStr.Append(c);
    return c;
  }

  bool MatchNext(char expected) {
    if (IsAtEnd()) return false;
    if (InputStream.Peek() == expected) {
      InputStream.Next();
      return true;
    }
    return false;
  }

  void IncrementLine() {
    Line++;
    Col = 0;
  }

  /**
   * Token Type Identification helpers
   */
  bool IsAlpha(char c) {
    // A-Z | a-z
    return Char.IsAsciiLetter(c);
  }

  bool IsDigit(char c) {
    // 0-9
    return Char.IsAsciiDigit(c);
  }

  /**
   * Token creation helpers
   */
  Token MakeToken(TokenType type) {
    return new Token(type, Line, Col);
  }

  Token MakeNumberToken() {
    // Integer part
    while (IsDigit(Peek())) {
      NextChar();
    }
    TokenType type = TokenType.INT_VALUE;

    // Fractional part
    if (Peek() == '.') {
      type = TokenType.FLT_VALUE;
      NextChar(); // eat the '.'

      // Must have at least one digit after the '.'
      if (!IsDigit(Peek())) return MakeErrorToken("Malformed number");

      while (IsDigit(Peek())) {
        NextChar();
      }
    }

    return new Token(type, Line, Col, TokenStr.ToString());
  }

  Token MakeErrorToken(string msg) {
    return new Token(TokenType.ERROR, Line, Col, msg);
  }
}
