
namespace Ixtl;

public class Vm {
  // Redirectable Output
  IOutput _output = null!;

  // Stack - should be Value[]
  readonly Stack<Value> _stack = new();

  // Global variables and functions
  readonly List<Value> _globals = [];

  // Code
  Chunk _chunk = null!;
  int _ip; // instruction pointer aka index into _chunk

  public bool Interpret(Chunk chunk, IOutput output) {
    _chunk = chunk;
    _ip = 0;
    _output = output;
    _stack.Clear();

    return Run();
  }

  byte ReadByte() {
    if (_ip >= _chunk.Code.Count) {
      throw new InvalidOperationException("instruction pointer reached end of bytecode");
    }
    return _chunk.Code[_ip++];
  }

  // TODO make this incrementally driven from outside (tickable)
  bool Run() {
    while (true) {
      WriteDebugLine($"------- Stack: [{string.Join(" | ", _stack)}]");
      WriteDebugLine($"------- Globals: [{string.Join(" | ", _globals)}]");

      OpCode instr = (OpCode)ReadByte();
      WriteDebugLine($":{instr}");
      switch (instr) {
        case OpCode.LITERAL: {
            var val = _chunk.Literals[ReadByte()];
            Push(val);
            break;
          }
        case OpCode.POP: {
            Pop();
            break;
          }
        case OpCode.DEFINE_GLOBAL_VAR: {
            ValueType vtype = (ValueType)ReadByte();
            Value v = Pop();
            WriteDebugLine($"New global type:{vtype}, init:{v}");
            _globals.Add(v);
            break;
          }
        case OpCode.GET_GLOBAL: {
            byte globalIdx = ReadByte();
            Value v = _globals[globalIdx];
            WriteDebugLine($"Lookup global #{globalIdx}: {v}");
            Push(v);
            break;
          }
        case OpCode.SET_GLOBAL: {
            byte globalIdx = ReadByte();
            Value v = Peek();
            WriteDebugLine($"Set global #{globalIdx}: {v}");
            _globals[globalIdx] = v;
            break;
          }
        case OpCode.ADD: {
          double b = Value.AsFlt(_stack.Pop()).Data;
          double a = Value.AsFlt(_stack.Pop()).Data;
          WriteDebugLine($"{a} + {b}");
          _stack.Push(new Value.Flt(a + b));
          break;
        }
        case OpCode.SUBTRACT: {
          double b = Value.AsFlt(_stack.Pop()).Data;
          double a = Value.AsFlt(_stack.Pop()).Data;
          WriteDebugLine($"{a} - {b}");
          _stack.Push(new Value.Flt(a - b));
          break;
        }
        case OpCode.MULTIPLY: {
          double b = Value.AsFlt(_stack.Pop()).Data;
          double a = Value.AsFlt(_stack.Pop()).Data;
          WriteDebugLine($"{a} * {b}");
          _stack.Push(new Value.Flt(a * b));
          break;
        }
        case OpCode.DIVIDE: {
          double b = Value.AsFlt(_stack.Pop()).Data;
          double a = Value.AsFlt(_stack.Pop()).Data;
          WriteDebugLine($"{a} / {b}");
          _stack.Push(new Value.Flt(a / b));
          break;
        }
        case OpCode.PRINT:
          _output.WriteLine($"{_stack.Pop()}");
          break;

        case OpCode.RETURN:
          WriteDebugLine($"Return");
          return true;

        default:
          _output.WriteLine($"Unsupported opcode: {instr}");
          return false;
      }
    }
  }

  void Push(Value val) {
    _stack.Push(val);
    WriteDebugLine($"Push {val}");
  }

  Value Pop() {
    Value val = _stack.Pop();
    WriteDebugLine($"Pop {val}");
    return val;
  }

  Value Peek() {
    Value val = _stack.Peek();
    WriteDebugLine($"Peek {val}");
    return val;
  }

  string GetLiteralStr(int i) {
    return Value.AsStr(_chunk.Literals[i]).Data;
  }

  void WriteDebugLine(string msg) {
    _output.WriteDebugLine("vm", msg);
  }
}
