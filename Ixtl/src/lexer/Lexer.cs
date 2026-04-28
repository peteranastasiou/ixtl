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
    return c switch {
      '(' => MakeToken(TokenType.LEFT_PAREN),
      ')' => MakeToken(TokenType.RIGHT_PAREN),
      '{' => MakeToken(TokenType.LEFT_BRACE),
      '}' => MakeToken(TokenType.RIGHT_BRACE),
      '[' => MakeToken(TokenType.LEFT_BRACKET),
      ']' => MakeToken(TokenType.RIGHT_BRACKET),
      ',' => MakeToken(TokenType.COMMA),
      '-' => MakeToken(TokenType.MINUS),
      '+' => MakeToken(TokenType.PLUS),
      ';' => MakeToken(TokenType.SEMICOLON),
      '/' => MakeToken(TokenType.SLASH),
      '*' => MakeToken(TokenType.STAR),
      '.' => MakeToken(TokenType.DOT),
      '!' => MakeToken(MatchNext('=') ? TokenType.BANG_EQUAL : TokenType.BANG),
      '=' => MakeToken(MatchNext('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL),
      '<' => MakeToken(MatchNext('=') ? TokenType.LESS_EQUAL : TokenType.LESS),
      '>' => MakeToken(MatchNext('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER),
      '&' => MatchNext('&') ? MakeToken(TokenType.AND) : MakeErrorToken("Expected '&&'"),
      '|' => MatchNext('|') ? MakeToken(TokenType.OR) : MakeErrorToken("Expected '||'"),
      '"' => MakeStringToken(),
      _ => MakeErrorToken($"Unexpected character: '{c}'"),
    };
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
      case 'b': return CheckKeyword(1, "ool", TokenType.BOOL);
      case 'e': return CheckKeyword(1, "lse", TokenType.ELSE);
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
      case 'i':
        // Could be if or int:
        if (_tokenStr.Length == 2 && _tokenStr[1] == 'f') {
          return TokenType.IF;
        } else if (_tokenStr.Length == 3) {
          return CheckKeyword(1, "nt", TokenType.INT);
        }
        break;
      case 'p': return CheckKeyword(1, "rint", TokenType.PRINT);
      case 's': return CheckKeyword(1, "tr", TokenType.STR);
      case 't': return CheckKeyword(1, "rue", TokenType.TRUE);
      case 'r': return CheckKeyword(1, "eturn", TokenType.RETURN);
      case 'v': return CheckKeyword(1, "oid", TokenType.VOID);
      case 'w': return CheckKeyword(1, "hile", TokenType.WHILE);
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
