
using Ixtl;

var input = new StringInputStream("+14.0435-63/3");

Scanner scanner = new Scanner(input);

while(true)
{
  var tok = scanner.ScanToken();
  Console.WriteLine($"Token {tok.Type}: {tok.Str}");
  if (tok.Type == TokenType.END)
  {
    return;
  }
}
