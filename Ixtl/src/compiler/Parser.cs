
namespace Ixtl;


public class Parser {
  IInputStream _input = null!;
  Lexer _lexer = null!;
  protected IOutput _output = null!;
  protected Token _curr;
  protected Token _prev;

  public void InitParser(IInputStream input, IOutput output) {
    input.Reset();
    _input = input;
    _lexer = new(input);
    _output = output;
    
  }

  protected void Advance() {
    _prev = _curr;
    // Skip over errors, reporting them
    while (true) {
      _curr = _lexer.ScanToken();
      if (_curr.Type == TokenType.ERROR) {
        ErrorAt(_curr, "");
      }
      else {
        return;
      }
    }
  }

  protected bool Check(TokenType tokenType) {
    return _curr.Type == tokenType;
  }

  protected bool Match(TokenType tokenType) {
    bool matched = Check(tokenType);
    if (matched) {
      Advance();
    }
    return matched;
  }

  protected void Consume(TokenType type, string errorMsg) {
    if (Check(type)) {
      Advance();
      return;
    }
    ErrorAt(_curr, errorMsg);
  }

  protected bool MatchType(out ValueType valueType) {
    ValueType? t = _curr.Type switch {
      TokenType.STR => ValueType.STR,
      TokenType.INT => ValueType.INT,
      TokenType.FLT => ValueType.FLT,
      TokenType.FN => ValueType.FN,
      TokenType.VOID => ValueType.VOID,
      _ => null
    };
    if (t != null) {
      Advance();
      valueType = (ValueType)t;
      return true;
    } else {
      // Forced to give it a value, unused:
      valueType = ValueType.VOID;
      return false;
    }
  }

  protected ValueType ConsumeType() {
    if (MatchType(out ValueType valueType)) {
      return valueType;
    }
    throw ExceptionFromErrorAt(_curr, "Expected a value type.");
  }

  /**
   * -----------------------------------------------------------
   * Error Handling
   * -----------------------------------------------------------
   */

  protected void ErrorAt(Token token, string msg) {
    throw ExceptionFromErrorAt(token, msg);
  }

  protected Exception ExceptionFromErrorAt(Token token, string msg) {
    // Print offending line:
    _output.WriteLine(_input.GetLine(token.Line));
    // Indicate position of error:
    for(int c = 0; c < token.Col - 2; c++) {
      _output.Write("-");
    }
    _output.WriteLine("^");

    if (token.Type == TokenType.ERROR) {
      msg = token.Str!;
    }

    _output.WriteLine($"[{token.Line}] Error: {msg}");

    // TODO better error messages
    throw new InvalidOperationException();
  }

}