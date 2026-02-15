namespace Ixtl;

public class ConsoleOutput : IOutput {
  public void Write(string str) {
    Console.Write(str);
  }

  public void WriteLine(string str) {
    Console.WriteLine(str);
  }

  public void WriteDebug(string str) {
    Console.Write(str);
  }

  public void WriteDebugLine(string str) {
    Console.WriteLine(str);
  }
}
