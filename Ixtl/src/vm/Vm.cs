
namespace Ixtl;

public class Vm {
  // Redirectable Output
  IOutput _output = null!;

  // Stack - should be Value[]
  readonly Stack<Value> _stack = new();

  // Global variables
  readonly Dictionary<string, Value> _globals = new();

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
    return _chunk.Code[_ip++];
  }

  // TODO make this incrementally driven from outside (tickable)
  bool Run() {
    while (true) {
      WriteDebugLine($"Stack: [{string.Join(" | ", _stack)}]");

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
            string name = GetLiteralStr(ReadByte());
            ValueType vtype = (ValueType)ReadByte();
            Value v = Pop();
            WriteDebugLine($"New global {vtype} {name} = {v}");
            if (!_globals.TryAdd(name, v)) {
              _output.WriteLine($"Global variable {name} is already defined");
            }
            break;
          }
        case OpCode.GET_GLOBAL: {
            string name = GetLiteralStr(ReadByte());
            if (_globals.TryGetValue(name, out Value? v)) {
              WriteDebugLine($"Global lookup '{name}': {v}");
              Push(v);
            }
            else {
              _output.WriteLine($"'{name}' is not defined.");
              return false;
            }
            break;
          }
        case OpCode.SET_GLOBAL: {
            string name = GetLiteralStr(ReadByte());
            if (!_globals.ContainsKey(name)) {
              _output.WriteLine($"'{name}' is not defined.");
              return false;
            }
            _globals[name] = Peek();
            _output.WriteLine($"set '{name}' to {_globals[name]}");
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
