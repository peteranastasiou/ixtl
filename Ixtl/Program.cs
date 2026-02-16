
using Ixtl;
using Port.Native;

var input = new StringInputStream("1.0435 63 + P R");
var output = new ConsoleOutput(debug: false);

Compiler compiler = new();
if ( compiler.Compile("<>", input, output, out Chunk chunk) ) {
  output.WriteDebugLine("Chunk: ");
  foreach(byte c in chunk.Code) {
    output.WriteDebugLine($"{c}");
  }

  Vm vm = new();
  vm.Interpret(chunk, output);
}
