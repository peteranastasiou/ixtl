namespace Ixtl;

public class Function(string name) {
  public string Name = name;
  public Chunk Chunk = new();

  public void WriteByteCode(byte b) {
    Chunk.Code.Add(b);
  }
}
