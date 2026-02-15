
Ixtl.IInputStream ss = new Ixtl.StringInputStream("hello world");

while (ss.Peek() != '\0')
{
  Console.WriteLine(ss.Next());
}
