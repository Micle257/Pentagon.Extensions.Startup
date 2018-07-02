namespace Pentagon.Extensions.Startup.Cli {
    using System.Threading.Tasks;

    public interface ICliCommand<in TOptions>
    {
        Task<int> RunAsync(TOptions options);
    }
}