namespace Port.Native;

using Ixtl;

public class ConsoleOutput : IOutput {
  bool _debug;

  public ConsoleOutput(bool debug) {
    _debug = debug;
  }

  public void Write(string str) {
    Console.Write(str);
  }

  public void WriteLine(string str) {
    Console.WriteLine(str);
  }

  public void WriteDebug(string str) {
    if (_debug) {
      Console.Write(str);
    }
  }

  public void WriteDebugLine(string str) {
    if (_debug) {
      Console.WriteLine(str);
    }
  }
}
