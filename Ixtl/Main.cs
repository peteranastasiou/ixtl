
using Ixtl;
using Port.Native;

string code = @"
  flt f1 = 0;
  flt f2 = 1 / 2;
  int i2 = 0;
  int i3 = i2 + i2;

  void main() {
    i2 = 1.0;
  }

  # Fail:
  #int aaa = 1.2;
  #int b1 = 1 / ""hello"";
";
// string code = @"
//   flt ab = 5.0;
//   flt cd = 3.0;
//   int ef = 1.0;  # Should fail type check
//   str string = ""hello world"";
//   void main() {
//     cd = 2.0;
//     print(cd);
//     print(ef + ab / cd - 1.1 * 3.0);
//   }
//   str end = ""The end"";
// ";

// Parse debug options
List<string> debugKeys = [];
foreach (var arg in args) {
  if(arg.StartsWith("--debug=")) {
    debugKeys.Add(arg[8..]);
  }
}

Console.WriteLine(code);

var input = new StringInputStream(code);
var output = new ConsoleOutput(debugKeys);

Compiler compiler = new();
Ixtl.Program program = new();
bool compiledOk = compiler.Compile("<>", input, output, program);
if (!compiledOk) {
  Environment.Exit(1);
}
output.WriteDebugLine("chunk", "------ Literals -------");
for (int i = 0; i < program.Literals.Count; i++) {
  output.WriteDebugLine("chunk", $"Literal[{i}]: {program.Literals[i]}");
}

// output.WriteDebugLine("chunk", "------ ByteCode -------");
// foreach (byte c in chunk.Code) {
//   output.WriteDebugLine("chunk", $" {c}");
// }

Vm vm = new();
bool ranOk = vm.Interpret(program, output);
Environment.Exit(ranOk ? 0 : 2);

