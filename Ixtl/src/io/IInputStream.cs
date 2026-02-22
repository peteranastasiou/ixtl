
namespace Ixtl;

public interface IInputStream {
  /**
   * return the next input character
   * null value (0) indicates end of stream
   */
  char Next();

  /**
   * return the next input character, without advancing
   * null value (0) indicates end of stream
   */
  char Peek();

  void Reset();

  string GetLine(int lineNum);

  /**
   * Close the stream
   */
  void Close();
}
