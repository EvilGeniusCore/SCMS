namespace SCMS.Tests
{
    // Tests that modify process-global state (CWD) or use WebApplicationFactory
    // must run sequentially to avoid conflicts.
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class SequentialCollection;
}
