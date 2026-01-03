using Microsoft.VisualStudio.TestTools.UnitTesting;

// Enable test parallelization for better performance
// Tests within a class run sequentially, but different classes can run in parallel
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]
