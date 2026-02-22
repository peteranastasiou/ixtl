
using Ixtl;
using Port.Native;

// string code = "1.0435 63 + P R";
string code = @"
  i32 ab = 5.0;
  i32 cd = 3.0;
  i32 ef = 1.0;
  str string = ""hello world"";
  void main(i32 c, str msg) {
    cd = 2.0;
    print(cd);
    print(ef + ab / cd - 1.1 * 3.0);
  }
  str end = ""The end"";
";

Console.WriteLine(code);

var input = new StringInputStream(code);
var output = new ConsoleOutput(debugKeys: ["compiler", "vm"]);

Compiler compiler = new();
if (compiler.Compile("<>", input, output, out Chunk chunk)) {
  output.WriteDebugLine("chunk", "------ Literals -------");
  for (int i = 0; i < chunk.Literals.Count; i++) {
    output.WriteDebugLine("chunk", $"Literal[{i}]: {chunk.Literals[i]}");
  }

  output.WriteDebugLine("chunk", "------ ByteCode -------");
  foreach (byte c in chunk.Code) {
    output.WriteDebugLine("chunk", $" {c}");
  }

  Vm vm = new();
  vm.Interpret(chunk, output);
}
else {
  Environment.Exit(1);
}
