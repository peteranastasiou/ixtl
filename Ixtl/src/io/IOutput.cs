namespace Ixtl;

public interface IOutput {
  void Write(string str);
  void WriteLine(string str);
  void WriteDebugLine(string key, string str);
}
