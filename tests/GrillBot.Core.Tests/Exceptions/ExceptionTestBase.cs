namespace GrillBot.Core.Tests.Exceptions;

public class ExceptionTestBase<TException> where TException : Exception, new()
{
    [TestMethod]
    public void EmptyConstructor()
    {
        try
        {
            throw new TException();
        }
        catch (TException ex)
        {
            Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
            Assert.Contains("Exception of type", ex.Message);
            Assert.IsNull(ex.InnerException);
        }
    }

    [TestMethod]
    public void WithMessage()
    {
        try
        {
            throw (TException)Activator.CreateInstance(typeof(TException), "Test")!;
        }
        catch (TException ex)
        {
            Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
            Assert.AreEqual("Test", ex.Message);
            Assert.IsNull(ex.InnerException);
        }
    }

    [TestMethod]
    public void WithInnerException()
    {
        try
        {
            throw (TException)Activator.CreateInstance(typeof(TException), "Test", new Exception("TestInner"))!;
        }
        catch (TException ex)
        {
            Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
            Assert.AreEqual("Test", ex.Message);
            Assert.IsNotNull(ex.InnerException);
            Assert.IsFalse(string.IsNullOrEmpty(ex.InnerException.Message));
            Assert.AreEqual("TestInner", ex.InnerException.Message);
        }
    }
}
