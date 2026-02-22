
namespace Ixtl;

public class DeclarationParser: Parser {
  List<Declaration> _declarations = null!;

  public bool Parse(string name, IInputStream input, IOutput output, out List<Declaration> declarations) {
    InitParser(input, output);
    _declarations = [];
    declarations = _declarations;
    try {
      // Get the first token
      Advance();

      // Parse top level declarations
      ParseProgram();

      return true;
    }
    catch (InvalidOperationException) {
      return false;
    }
  }

  void ParseProgram() {
    while (!Match(TokenType.EOF)) {
      // Top level statements can be either global variables or functions
      // Both start with <Type> <Identifier> before they differentiate
      ValueType t = ConsumeType();
      string name = ParseName();
      if (Match(TokenType.LEFT_PAREN)) {
        // its a function
        ParseFunction(t, name);
      }
      else if (Match(TokenType.EQUAL)) {
        // its a global variable definition
        ParseGlobal(t, name);
      }
      else {
        ErrorAt(_curr, "Expected either ( or =");
      }
    }
  }

  string ParseName() {
    Consume(TokenType.IDENTIFIER, "Expected a valid name");
    return _prev.Str!;
  }

  void ParseFunction(ValueType vtype, string name) {
    List<ValueType> paramTypes = [];

    // Check for params
    if (!Check(TokenType.RIGHT_PAREN)) {
      do {
        paramTypes.Add(ConsumeType());
        ParseName();
      } while (Match(TokenType.COMMA));
    }
    Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters.");
    Consume(TokenType.LEFT_BRACE, "Expected '{' before function body.");
    
    // Skim through content only tracking {} nesting
    int depth = 1;
    while(depth > 0) {
      if (Check(TokenType.EOF)) {
        ErrorAt(_curr, "Unterminated function block");
      } else if (Check(TokenType.LEFT_BRACE)) {
        depth ++;
      } else if (Check(TokenType.RIGHT_BRACE)) {
        depth --;
      }
      Advance();
    }

    _declarations.Add(new() {
      DeclType = DeclarationType.FUNCTION,
      InputTypes = paramTypes,
      OutputType = vtype,
      Name = name
    });
  }

  void ParseGlobal(ValueType vtype, string name) {
    // Skim until we hit ';' or EOF
    while(!Check(TokenType.SEMICOLON) && !Check(TokenType.EOF)) {
      Advance();
    }
    Consume(TokenType.SEMICOLON, "Expected ';' after variable definition.");

    _declarations.Add(new() {
      DeclType = DeclarationType.GLOBAL_VAR,
      OutputType = vtype,
      Name = name
    });
  }
}
