namespace Ixtl;

public interface IOutput {
  void Write(string str);
  void WriteLine(string str);
  void WriteDebug(string str);
  void WriteDebugLine(string str);
}
