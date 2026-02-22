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

    // Identifiers and Keywords
    if (IsAlpha(c)) return MakeIdentifierOrKeywordToken();

    // Numbers
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
      case '"': return MakeStringToken();
    }

    return MakeErrorToken($"Unexpected character: '{c}'");
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
   * -----------------------------------------------------------
   * Input Stream Helpers
   * -----------------------------------------------------------
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
   * -----------------------------------------------------------
   * Token Type Identification helpers
   * -----------------------------------------------------------
   */

  static bool IsAlpha(char c) {
    // A-Z | a-z | _
    return Char.IsAsciiLetter(c) || c == '_';
  }

  static bool IsDigit(char c) {
    // 0-9
    return Char.IsAsciiDigit(c);
  }

  /**
   * -----------------------------------------------------------
   * Token creation helpers
   * -----------------------------------------------------------
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

  Token MakeStringToken() {
    while(Peek() != '"' && !IsAtEnd()) {
      if(Peek() == '\n') IncrementLine();
      if(Peek() == '\\') NextChar();
      NextChar();
    }
    if (IsAtEnd()) {
      return MakeErrorToken("Unterminated string");
    }
    // Consume terminating quote
    NextChar();

    // Get content of brackets:
    var content = _tokenStr.ToString()[1..^1];
    return new Token(TokenType.STR_VALUE, _line, _col, content);
  }

  Token MakeIdentifierOrKeywordToken() {
    while (IsAlpha(Peek()) || IsDigit(Peek())) {
      NextChar();
    }

    // Detect which keyword or whether it is an identifier
    TokenType type = GetIdentifierOrKeywordType();

    // Only capture the string if its an identier:
    string? str = type == TokenType.IDENTIFIER ? _tokenStr.ToString() : null;
    return new Token(type, _line, _col, str);
  }

  Token MakeErrorToken(string msg) {
    return new Token(TokenType.ERROR, _line, _col, msg);
  }

  /**
   * -----------------------------------------------------------
   * Keyword Trie
   * -----------------------------------------------------------
   */

  TokenType GetIdentifierOrKeywordType() {
    // Use a trie to scan for keywords efficiently:
    switch (_tokenStr[0]) {
      case 'i': return CheckKeyword(1, "32", TokenType.I32);
      case 'f':
        // Could be false, flt, fn or for:
        if (_tokenStr.Length > 1) {
          switch (_tokenStr[1]) {
            case 'a': return CheckKeyword(2, "lse", TokenType.FALSE);
            case 'l': return CheckKeyword(2, "t", TokenType.FLT);
            case 'o': return CheckKeyword(2, "r", TokenType.FOR);
            case 'n': return TokenType.FN;
          }
        }
        break;
      case 'p': return CheckKeyword(1, "rint", TokenType.PRINT);
      case 's': return CheckKeyword(1, "tr", TokenType.STR);
      case 't': return CheckKeyword(1, "rue", TokenType.TRUE);
      case 'v': return CheckKeyword(1, "oid", TokenType.VOID);
    }
    // Not a keyword
    return TokenType.IDENTIFIER;
  }

  TokenType CheckKeyword(int offset, string rest, TokenType type) {
    // Check length
    if (_tokenStr.Length != offset + rest.Length) {
      // Nope, must be identifier
      return TokenType.IDENTIFIER;
    }
    // Check content
    for (int i = 0; i < rest.Length; i++) {
      if (_tokenStr[offset + i] != rest[i]) return TokenType.IDENTIFIER;
    }
    // Its a match
    return type;
  }
}
