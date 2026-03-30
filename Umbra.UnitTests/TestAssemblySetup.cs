using Umbra.Logging;

namespace Umbra.UnitTests;

[TestClass]
public static class TestAssemblySetup
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _) => Logger.DisableAll();
}
