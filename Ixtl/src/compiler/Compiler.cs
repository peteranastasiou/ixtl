#nullable disable

namespace Ixtl;

public class Compiler {
  Lexer _lexer;
  IOutput _output;
  Chunk _chunk;
  Token curr;
  Token prev;
  bool _hasErrors;

  public bool Compile(string name, IInputStream input, IOutput output, out Chunk chunk) {
    _lexer = new(input);
    _output = output;
    _chunk = new();
    chunk = _chunk;
    _hasErrors = false;

    while(true) {
      Advance();
      // Temporary Reverse Polish style language to kickstart us
      switch (curr.Type) {
        case TokenType.PLUS:
          EmitOpCode(OpCode.ADD);
          break;

        case TokenType.FLT_VALUE:
          MakeFloatValue();
          break;

        case TokenType.INT_VALUE:
          // TODO integers
          MakeFloatValue();
          break;

        case TokenType.PRINT:
          EmitOpCode(OpCode.PRINT);
          break;

        case TokenType.RETURN:
          EmitOpCode(OpCode.RETURN);
          break;
        
        case TokenType.EOF:
          return !_hasErrors;
      }
    }
  }

  void Advance() {
    prev = curr;
    // Skip over errors, reporting them
    while (true) {
      curr = _lexer.ScanToken();
      _output.WriteDebugLine($"Scanned {curr}");
      if (curr.Type == TokenType.ERROR) {
        ErrorAt(curr, "");
      }
      else {
        return;
      }
    }
  }

  void MakeFloatValue() {
    if (double.TryParse(curr.Str, out double num)) {
      int literalIdx = _chunk.Literals.Count;
      _chunk.Literals.Add(num);
      if (literalIdx > 255) {
        ErrorAt(curr, "Over 255 literals is not supported");
        return;
      }
      // Create the instruction
      EmitOpCode(OpCode.LITERAL);
      EmitByte((byte)literalIdx);
    }
  }

  void EmitOpCode(OpCode opCode) {
    _chunk.Code.Add((byte)opCode);
  }

  void EmitByte(byte b) {
    _chunk.Code.Add(b);
  }

  void ErrorAt(Token token, string msg) {
    // Suppress errors after the first
    if( _hasErrors ) return;
    _hasErrors = true;

    if (token.Type == TokenType.ERROR) {
      _output.WriteLine($"[{token.Line}] Error: {token.Str}");
    }
    else {
      _output.WriteLine($"[{token.Line}] Error at {token.Str}: {msg}");
    }
    // TODO better error messages
  }
}
