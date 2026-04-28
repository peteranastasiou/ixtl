
using Ixtl;
using Port.Native;

string code = @"
  flt a1 = 0;
  int aaa = 1.2;
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
bool compiledOk = compiler.Compile("<>", input, output, out Chunk chunk);
if (!compiledOk) {
  Environment.Exit(1);
}
output.WriteDebugLine("chunk", "------ Literals -------");
for (int i = 0; i < chunk.Literals.Count; i++) {
  output.WriteDebugLine("chunk", $"Literal[{i}]: {chunk.Literals[i]}");
}

output.WriteDebugLine("chunk", "------ ByteCode -------");
foreach (byte c in chunk.Code) {
  output.WriteDebugLine("chunk", $" {c}");
}

Vm vm = new();
bool ranOk = vm.Interpret(chunk, output);
Environment.Exit(ranOk ? 0 : 2);

