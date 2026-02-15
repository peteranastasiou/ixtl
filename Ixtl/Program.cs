
using Ixtl;

var input = new StringInputStream("+14.0435-63/3");

Lexer lexer = new Lexer(input);

while (true) {
  var tok = lexer.ScanToken();
  Console.WriteLine($"Token {tok.Type}: {tok.Str}");
  if (tok.Type == TokenType.END) {
    return;
  }
}
