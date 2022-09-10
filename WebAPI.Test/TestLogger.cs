using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace WebAPI.Test;

class TestLogger<T> : ILogger<T>
{
    ITestOutputHelper _testOutputHelper;

    public TestLogger(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
    }

    public IDisposable BeginScope<TState>(TState state) {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) {
        throw new NotImplementedException();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        _testOutputHelper.WriteLine($"{logLevel}: {state}");
    }
}