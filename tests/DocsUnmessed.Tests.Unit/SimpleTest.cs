using NUnit.Framework;

namespace DocsUnmessed.Tests.Unit;

public class SimpleTest
{
    [Test]
    public void SimpleTest_ShouldPass()
    {
        Assert.That(true, Is.True);
    }
}
