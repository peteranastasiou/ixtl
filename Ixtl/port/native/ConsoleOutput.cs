namespace Port.Native;

using Ixtl;

public class ConsoleOutput : IOutput {
  readonly List<string>? _debugKeys;

  public ConsoleOutput(List<string>? debugKeys) {
    _debugKeys = debugKeys;
  }

  public void Write(string str) {
    Console.Write(str);
  }

  public void WriteLine(string str) {
    Console.WriteLine(str);
  }

  public void WriteDebugLine(string key, string str) {
    if (_debugKeys != null && _debugKeys.Contains(key)) {
      Console.WriteLine($"[{key}] {str}");
    }
  }
}
