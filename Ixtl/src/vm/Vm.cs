#nullable disable

namespace Ixtl;

public class Vm {
  // Redirectable Output
  IOutput _output;

  // Stack - should be Value[]
  Stack<double> _stack = new();

  // Code
  Chunk _chunk;
  int _ip; // instruction pointer aka index into _chunk

  public bool Interpret(Chunk chunk, IOutput output) {
    _chunk = chunk;
    _ip = 0;
    _output = output;
    _stack.Clear();

    return Run();
  }

  byte ReadByte() {
    return _chunk.Code[_ip++];
  }

  // TODO make this incrementally driven from outside (tickable)
  bool Run() {
    while (true) {
      _output.Write("Stack: ");
      foreach (float v in _stack) {
        _output.Write($"{v} |");
      }
      _output.WriteLine("");

      OpCode instruction = (OpCode)ReadByte();
      switch (instruction) {
        case OpCode.LITERAL:
          var val = _chunk.Literals[ReadByte()];
          _output.WriteDebugLine($"Literal: {val}");
          _stack.Push(val);
          break;

        case OpCode.ADD:
          var a = _stack.Pop();
          var b = _stack.Pop();
          _output.WriteDebugLine($"Adding {a} and {b}");
          _stack.Push(a + b);
          break;

        case OpCode.PRINT:
          _output.WriteLine($"{_stack.Pop()}");
          break;

        case OpCode.RETURN:
          _output.WriteDebugLine($"Return");
          return true;
      }
    }
  }
}
