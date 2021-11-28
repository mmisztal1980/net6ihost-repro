namespace Repro;

public static partial class Constants
{
    public static class EnvironmentVariables
    {
        public const string Hostname = "HOSTNAME";
        public const string Environment = "Environment";

        public static class DotNet
        {
            public const string Environment = "ASPNETCORE_ENVIRONMENT";
            public const string DotNetRunningInContainer = "DOTNET_RUNNING_IN_CONTAINER";
        }

        public static class Kubernetes
        {
            public const string KubernetesVariablePrefix = "KUBERNETES";
        }                        
    }
}
