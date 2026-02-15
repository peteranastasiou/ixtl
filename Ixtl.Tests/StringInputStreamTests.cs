namespace Ixtl.Tests;

using Xunit;

public class StringInputStreamTests
{
    [Fact]
    public void PeekNextTestString()
    {
        IInputStream ss = new StringInputStream("Hi\nBye");
        Assert.Equal('H', ss.Peek());
        Assert.Equal('H', ss.Peek());
        Assert.Equal('H', ss.Next());
        Assert.Equal('i', ss.Peek());
        Assert.Equal('i', ss.Next());
        Assert.Equal('\n', ss.Peek());
        Assert.Equal('\n', ss.Next());
        Assert.Equal('B', ss.Next());
        Assert.Equal('y', ss.Next());
        Assert.Equal('e', ss.Peek());
        Assert.Equal('e', ss.Next());
        Assert.Equal('\0', ss.Peek());
        Assert.Equal('\0', ss.Next());
        Assert.Equal('\0', ss.Peek());
        Assert.Equal('\0', ss.Next());
    }

    
    [Fact]
    public void PeekNextTestEmptyString()
    {
        IInputStream ss = new StringInputStream("");
        Assert.Equal('\0', ss.Peek());
        Assert.Equal('\0', ss.Next());
        Assert.Equal('\0', ss.Peek());
        Assert.Equal('\0', ss.Next());
    }
}
