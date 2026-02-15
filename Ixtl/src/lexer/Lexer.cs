
namespace Ixtl;
using System.Text;

public class Lexer {
  readonly IInputStream _inputStream;

  // Current token
  readonly StringBuilder _tokenStr;

  // Position
  int _col;
  int _line;

  public Lexer(IInputStream inputStream) {
    _inputStream = inputStream;
    _tokenStr = new StringBuilder();
    _line = 0;
    _col = 0;
  }

  public Token ScanToken() {
    // first, gobble up whitespace and comments:
    SkipWhitespace();

    // Reset token buffer
    _tokenStr.Clear();

    if (IsAtEnd()) return MakeToken(TokenType.EOF);

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

      // Temporary
      case 'P': return MakeToken(TokenType.PRINT);
      case 'R': return MakeToken(TokenType.RETURN);
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
    return _inputStream.Peek();
  }

  bool IsAtEnd() {
    return _inputStream.Peek() == '\0';
  }

  char NextChar() {
    char c = _inputStream.Next();
    _col++;
    _tokenStr.Append(c);
    return c;
  }

  bool MatchNext(char expected) {
    if (IsAtEnd()) return false;
    if (_inputStream.Peek() == expected) {
      _inputStream.Next();
      return true;
    }
    return false;
  }

  void IncrementLine() {
    _line++;
    _col = 0;
  }

  /**
   * Token Type Identification helpers
   */
  static bool IsAlpha(char c) {
    // A-Z | a-z
    return Char.IsAsciiLetter(c);
  }

  static bool IsDigit(char c) {
    // 0-9
    return Char.IsAsciiDigit(c);
  }

  /**
   * Token creation helpers
   */
  Token MakeToken(TokenType type) {
    return new Token(type, _line, _col);
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

    return new Token(type, _line, _col, _tokenStr.ToString());
  }

  Token MakeErrorToken(string msg) {
    return new Token(TokenType.ERROR, _line, _col, msg);
  }
}
