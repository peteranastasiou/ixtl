
namespace Ixtl;

using static OperatorPrecedence;

public class Compiler: Parser {
  Chunk _chunk = null!;
  List<Declaration> _declarations = null!;

  public struct Context {
    // Expected value type resulting from an expression
    // Used for type checking 
    // null means any value type allowed
    public ValueType? expectedType;
  }

  public bool Compile(string name, IInputStream input, IOutput output, out Chunk chunk) {
    _chunk = new();
    chunk = _chunk;

    // First Pass
    DeclarationParser dp = new();
    if (!dp.Parse(name, input, output, out _declarations)) {
      return false;
    }

    // Must happen after first pass to reset input stream correctly
    InitParser(input, output);

    // Second Pass
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

  /**
   * -----------------------------------------------------------
   * Compiling
   * -----------------------------------------------------------
   */

  void ParseProgram() {
    while (!Match(TokenType.EOF)) {
      // Top level statements can be either global variables or functions
      // Both start with <Type> <Identifier> before they differentiate
      ValueType t = ConsumeType();
      byte nameIdx = MakeIdentifierLiteral();
      if (Match(TokenType.LEFT_PAREN)) {
        // its a function
        ParseFunction(t, nameIdx);
      }
      else if (Match(TokenType.EQUAL)) {
        // its a global variable definition
        ParseGlobal(t, nameIdx);
      }
      else {
        ErrorAt(_curr, "Expected either ( or =");
      }
    }
  }

  void ParseFunction(ValueType vtype, byte nameIdx) {
    // Check for params
    if(!Check(TokenType.RIGHT_PAREN)) {
      do {
        ParseParam();
      } while (Match(TokenType.COMMA));
    }
    Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters.");
    Consume(TokenType.LEFT_BRACE, "Expected '{' before function body.");
    ParseBlock();
    EmitInstr(OpCode.RETURN);
  }

  void ParseGlobal(ValueType vtype, byte nameIdx) {
    // Initial value:
    ParseExpr(vtype);
    EmitInstr(OpCode.DEFINE_GLOBAL_VAR, nameIdx, (byte) vtype);
    Consume(TokenType.SEMICOLON, "Expected ';' after variable definition.");
  }

  void ParseParam() {
    ValueType vtype = ConsumeType();
    byte nameIdx = MakeIdentifierLiteral();
    // TODO parameters
    // TODO check parameter against expected type if fn already used by code
  }

  void ParseBlock() {
    // Parse statements until hit the closing brace
    while(!Check(TokenType.RIGHT_BRACE) && !Check(TokenType.EOF)) {
      ParseStatement();
    }
    Consume(TokenType.RIGHT_BRACE, "Expected '}' at end of block.");
  }

  void ParseStatement() {
    if (MatchType(out ValueType valueType)) {
      // its a local variable declaration
      _output.WriteLine("TODO consume local variable");
    }
    else if (Match(TokenType.PRINT)) {
      ParsePrint();
    }
    else {
      // Expression-statement
      ParseExpr();
      Consume(TokenType.SEMICOLON, "Expected ';' after statement");
      // Discard result:
      EmitInstr(OpCode.POP);
    }
  }

  // Deprecated, inject host func instead
  void ParsePrint() {
    Consume(TokenType.LEFT_PAREN, "Expected '(' after print");
    // 1 argument
    ParseExpr();
    Consume(TokenType.RIGHT_PAREN, "Expected ')' after print argument");
    Consume(TokenType.SEMICOLON, "Expected ';' after statement.");
    EmitInstr(OpCode.PRINT);
  }

  void ParseExpr(ValueType? retType = null) {
    // Parse expressions with precedence >= ASSIGNMENT
    Parse(Precedence.ASSIGNMENT, retType);
  }

  void ParseVarReference(bool canAssign) {
    byte globalIdx = AddLiteral(new Value.Str(_prev.Str!));

    if (canAssign && Match(TokenType.EQUAL)) {
      ParseExpr();
      EmitInstr(OpCode.SET_GLOBAL, globalIdx);
    } else {
      EmitInstr(OpCode.GET_GLOBAL, globalIdx);
    }
  }

  void Parse(Precedence prec, ValueType? retType) {
    Advance();

    bool canAssign = prec <= Precedence.ASSIGNMENT;
    
    // An expression must start with a operand or prefix operation:
    if (!PrefixOperation(_prev.Type, canAssign, retType)) {
      ErrorAt(_prev, "Expected an expression.");
    }

    // Then perform infix operations from left to right
    // until we hit a lower precedence operation
    while(prec <= GetInfixPrecedence(_curr.Type)) {
      Advance();
      // Perform the infix operation, potentially calling this fn recursively
      InfixOperation(_prev.Type, retType);
    }

    // Detect stray assigment
    if (canAssign && Match(TokenType.EQUAL)) {
      ErrorAt(_prev, "Invalid assignment target");
    }

  }

  // TODO tail return in all following blocks

  /**
   * -----------------------------------------------------------
   * Operations
   * -----------------------------------------------------------
   */
  bool PrefixOperation(TokenType type, bool canAssign, ValueType? retType) {
    switch( type ){
        // Control flow
        // case TokenType.LEFT_PAREN:    grouping_(); return true;
        // case TokenType.LEFT_BRACKET:  list_(); return true;
        // case TokenType.LEFT_BRACE:    expressionBlock_(); return true;
        // case TokenType.IF:            ifExpression_(); return true;
        // case TokenType.FN:            funcAnonymous_(); return true;

        // Math
        // case TokenType.MINUS:         unary_(); return true;
        // case TokenType.BANG:          unary_(); return true;

        // Values
        case TokenType.STR_VALUE:     MakeStrValue();   return true;
        case TokenType.INT_VALUE:     MakeIntValue();   return true;
        case TokenType.FLT_VALUE:     MakeFloatValue(); return true;
        case TokenType.TRUE:          EmitTrue();       return true;
        case TokenType.FALSE:         EmitFalse();      return true;

        // Variables
        case TokenType.IDENTIFIER:    ParseVarReference(canAssign);return true;

        // Built-in functions
        case TokenType.PRINT:         ParsePrint(); return true;
        default: return false;
    }
  }

  bool InfixOperation(TokenType type, ValueType? retType) {
    switch( type ){
        // case Token::LEFT_PAREN:      call_();                           return true;
        // case Token::LEFT_BRACKET:    index_();                          return true;

        case TokenType.STAR:            BinaryOperation(OpCode.MULTIPLY, retType);         return true;
        case TokenType.SLASH:           BinaryOperation(OpCode.DIVIDE, retType);           return true;

        case TokenType.PLUS:            BinaryOperation(OpCode.ADD, retType);     return true;
        case TokenType.MINUS:           BinaryOperation(OpCode.SUBTRACT, retType);         return true;

        // case Token::GREATER:         binary_(OpCode::GREATER);          return true;
        // case Token::GREATER_EQUAL:   binary_(OpCode::GREATER_EQUAL);    return true;
        // case Token::LESS:            binary_(OpCode::LESS);             return true;
        // case Token::LESS_EQUAL:      binary_(OpCode::LESS_EQUAL);       return true;
        // case Token::BANG_EQUAL:      binary_(OpCode::NOT_EQUAL);        return true;
        // case Token::EQUAL_EQUAL:     binary_(OpCode::EQUAL);            return true;

        // case Token::AND:             and_();                            return true;

        // case Token::OR:              or_();                             return true;
        default: return false;
    }
  }

  void BinaryOperation(OpCode opCode, ValueType? retType) {
    // _prev token is the binary operation: check its precedence
    int prec = (int)GetInfixPrecedence(_prev.Type);

    // parse the second operand, and stop when the precendence is equal or lower
    // Note: Stopping when precedence is equal causes math to be left associative: 1+2+3 = (1+2)+3
    Parse((Precedence)(prec + 1), retType);

    // Now we will have both operands on the stack, emit the binary op now:
    EmitInstr(opCode);
  }

  /**
   * -----------------------------------------------------------
   * Constructing Values
   * -----------------------------------------------------------
   */

  byte MakeIdentifierLiteral() {
    Consume(TokenType.IDENTIFIER, "Expected a valid name");
    return AddLiteral(new Value.Str(_prev.Str!));
  }

  void MakeIntValue() {
    if (int.TryParse(_prev.Str, out int num)) {
      byte literalIdx = AddLiteral(new Value.I32(num));
      EmitInstr(OpCode.LITERAL, literalIdx);
    } else {
      ErrorAt(_prev, "Invalid floating point value.");
    }
  }

  void MakeFloatValue() {
    if (double.TryParse(_prev.Str, out double num)) {
      byte literalIdx = AddLiteral(new Value.Flt(num));
      EmitInstr(OpCode.LITERAL, literalIdx);
    } else {
      ErrorAt(_prev, "Invalid floating point value.");
    }
  }

  void MakeStrValue() {
    byte literalIdx = AddLiteral(new Value.Str(_prev.Str!));
    EmitInstr(OpCode.LITERAL, literalIdx);
  }

  byte AddLiteral(Value v) {
    int index = _chunk.Literals.Count;
    if (index >= 254) {
      ErrorAt(_prev, "Too many literals in this chunk.");
    }
    _chunk.Literals.Add(v);
    WriteDebugLine($"Literal[{index}]: {v}");
    return (byte) index;
  }

  /**
   * -----------------------------------------------------------
   * Outputting Instructions
   * -----------------------------------------------------------
   */

  void EmitInstr(OpCode opCode) {
    WriteDebugLine($"OpCode: {(byte)opCode}  # {opCode}");
    _chunk.Code.Add((byte)opCode);
  }

  void EmitInstr(OpCode opCode, byte b) {
    EmitInstr(opCode);
    WriteDebugLine($"  opnd: {b}");
    _chunk.Code.Add(b);
  }

  void EmitInstr(OpCode opCode, byte a, byte b) {
    EmitInstr(opCode, a);
    WriteDebugLine($"  opnd: {b}");
    _chunk.Code.Add(b);
  }

  void EmitTrue() {
    EmitInstr(OpCode.TRUE);
  }

  void EmitFalse() {
    EmitInstr(OpCode.FALSE);
  }

  /**
   * -----------------------------------------------------------
   * Debug Printing
   * -----------------------------------------------------------
   */
  void WriteDebugLine(string msg) {
    _output.WriteDebugLine("compiler", msg);
  }
}

