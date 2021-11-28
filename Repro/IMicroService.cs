using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Repro;

public interface IMicroService
{  
    string Name { get; }
    Task RunAsync(IConfigurationRoot configuration = null, params string[] args);
}