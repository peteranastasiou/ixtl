
Itxl.IInputStream ss = new Itxl.StringInputStream("hello world");

while (ss.Peek() != '\0')
{
  Console.WriteLine(ss.Next());
}
