using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

// Enable test parallelization for better performance
// Tests within a class run sequentially, but different classes can run in parallel
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]

namespace Rapr.Tests
{
    [STATestClass]
    public sealed class TestAssembly
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Application.EnableVisualStyles();
        }
    }
}
