
namespace Ixtl;

using static OperatorPrecedence;

public class Compiler: Parser {
  Dictionary<string, Declaration> _declarations = null!;

  // The program we are writing to
  Program _program = null!;

  // Current function within the program we are writing to
  Function? _function = null;

  public struct Context {
    // Expected value type resulting from an expression
    // Used for type checking 
    // null means any value type allowed
    public ValueType? expectedType;
  }

  public bool Compile(string name, IInputStream input, IOutput output, Program program) {
    // Store reference to program to write to
    _program = program;
    _function = null;

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
      Consume(TokenType.IDENTIFIER, "Expected a valid name");
      string name = _prev.Str ?? "";
      if (name.Length == 0) {
        ErrorAt(_curr, "Couldn't parse name of top level identifier");
      }
      WriteDebugLine($"Top level identifier: {name}");

      if (Match(TokenType.LEFT_PAREN)) {
        // its a function
        ParseFunction(name, t);
      }
      else if (Match(TokenType.EQUAL)) {
        // its a global variable definition
        ParseGlobal(name, t);
      }
      else {
        ErrorAt(_curr, "Expected either ( or =");
      }
    }
  }

  void ParseFunction(string name, ValueType vtype) {
    // Create a new function object to write to
    _function = _program.NewFunction(name);

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

  void ParseGlobal(string name, ValueType vtype) {
    // Initial value:
    ParseExpr(vtype);
    // Add the global, note that we do this in the right sequence so we can look up by index later
    EmitInstr(OpCode.DEFINE_GLOBAL_VAR, (byte) vtype);
    Consume(TokenType.SEMICOLON, "Expected ';' after variable definition.");
  }

  void ParseParam() {
    // ValueType vtype = ConsumeType();
    // byte nameIdx = ParseGlobalName();
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
    // 1 argument. Don't specify return type, accepts any.
    ParseExpr();
    Consume(TokenType.RIGHT_PAREN, "Expected ')' after print argument");
    Consume(TokenType.SEMICOLON, "Expected ';' after statement.");
    EmitInstr(OpCode.PRINT);
  }

  void ParseExpr(ValueType? retType = null) {
    // Parse expressions with precedence >= ASSIGNMENT
    Parse(Precedence.ASSIGNMENT, retType);
  }

  short GetIndexOfGlobal(string name) {
    if (_declarations.TryGetValue(name, out Declaration declaration)) {
      return declaration.Id;
    } else {
      ErrorAt(_prev, $"The name {name} is not defined");
      return 0;  // unreachable
    }
  }

  void ParseVarReference(bool canAssign, ValueType? retType) {
    short globalIdx = GetIndexOfGlobal(_prev.Str!);

    if (canAssign && Match(TokenType.EQUAL)) {
      ParseExpr(retType);
      EmitInstr(OpCode.SET_GLOBAL, (byte)globalIdx);
    } else {
      EmitInstr(OpCode.GET_GLOBAL, (byte)globalIdx);
    }
  }

  void Parse(Precedence prec, ValueType? retType) {
    WriteDebugLine($"Parsing - expect an {retType}");
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
    WriteDebugLine($"Prefix op {type} - expect an {retType}");
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

        // Literals
        case TokenType.STR_VALUE:     MakeStrValue(retType);   return true;
        case TokenType.INT_VALUE:     MakeIntValue(retType);   return true;
        case TokenType.FLT_VALUE:     MakeFloatValue(retType); return true;
        case TokenType.TRUE:          EmitTrue(retType);       return true;
        case TokenType.FALSE:         EmitFalse(retType);      return true;

        // Variables
        case TokenType.IDENTIFIER:    ParseVarReference(canAssign, retType); return true;

        // Built-in functions
        case TokenType.PRINT:         ParsePrint(); return true;
        default: return false;
    }
  }

  bool InfixOperation(TokenType type, ValueType? retType) {
    WriteDebugLine($"Infix op {type} - expect an {retType}");
    switch( type ){
        // case Token::LEFT_PAREN:      call_();                           return true;
        // case Token::LEFT_BRACKET:    index_();                          return true;

        case TokenType.STAR:            BinaryOperation(OpCode.MULTIPLY, retType);         return true;
        case TokenType.SLASH:           BinaryOperation(OpCode.DIVIDE, retType);           return true;

        case TokenType.PLUS:            BinaryOperation(OpCode.ADD, retType);              return true;
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
    // Type checking: We want result to be `retType`
    // first operand is gone... did it get checked?

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

  void MakeIntValue(ValueType? retType) {
    // Type checking (i.e. can an integer literal be implicitly cast to anything): yes, a float.
    if (retType == ValueType.FLT) {
      // We actually want a float, so make that instead:
      MakeFloatValue(retType);
      return;
    }
    if (retType != null && retType != ValueType.INT) {
      ErrorAt(_prev, $"No implicit cast from integer to a {Value.ValueTypeToStr(retType)}");
    }

    // Parse value as int
    if (int.TryParse(_prev.Str, out int num)) {
      byte literalIdx = AddLiteral(new Value.Int(num));
      EmitInstr(OpCode.LITERAL, literalIdx);
    } else {
      ErrorAt(_prev, "Invalid floating point value.");
    }
  }

  void MakeFloatValue(ValueType? retType) {
    // Type checking (i.e. can a float literal be implicitly cast to anything): no.
    if (retType != null && retType != ValueType.FLT ) {
      ErrorAt(_prev, $"No implicit cast from floating point to a {Value.ValueTypeToStr(retType)}");
    }

    // Parse value as float
    if (double.TryParse(_prev.Str, out double num)) {
      byte literalIdx = AddLiteral(new Value.Flt(num));
      EmitInstr(OpCode.LITERAL, literalIdx);
    } else {
      ErrorAt(_prev, "Invalid floating point value.");
    }
  }

  void MakeStrValue(ValueType? retType) {
    // Type checking (i.e. can a string literal be implicitly cast to anything): no.
    if (retType != null && retType != ValueType.STR ) {
      ErrorAt(_prev, $"No implicit cast from string to a {Value.ValueTypeToStr(retType)}");
    }

    byte literalIdx = AddLiteral(new Value.Str(_prev.Str!));
    EmitInstr(OpCode.LITERAL, literalIdx);
  }

  byte AddLiteral(Value v) {
    int index = _program.Literals.Count;
    if (index >= 254) {
      ErrorAt(_prev, "Too many literals in this chunk.");
    }
    _program.Literals.Add(v);
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
    _function?.WriteByteCode((byte)opCode);
  }

  void EmitInstr(OpCode opCode, byte b) {
    EmitInstr(opCode);
    WriteDebugLine($"  opnd: {b}");
    _function?.WriteByteCode(b);
  }

  void EmitInstr(OpCode opCode, byte a, byte b) {
    EmitInstr(opCode, a);
    WriteDebugLine($"  opnd: {b}");
    _function?.WriteByteCode(b);
  }

  void EmitTrue(ValueType? retType) {
    // Type-checking: Can true be implicitly cast to anything but a bool? no.
    if (retType != null && retType != ValueType.BOOL) {
      ErrorAt(_prev, $"No implicit cast from bool to a {Value.ValueTypeToStr(retType)}");
    }
    EmitInstr(OpCode.TRUE);
  }

  void EmitFalse(ValueType? retType) {
    // Type-checking: Can true be implicitly cast to anything but a bool? no.
    if (retType != null && retType != ValueType.BOOL) {
      ErrorAt(_prev, $"No implicit cast from bool to a {Value.ValueTypeToStr(retType)}");
    }
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

