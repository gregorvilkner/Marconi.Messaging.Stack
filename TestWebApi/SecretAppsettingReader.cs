using Microsoft.Extensions.Configuration;
using System;

public class ConfigurationRoot
{
    public static IConfigurationRoot Get()
    {
        // https://programmingwithwolfgang.com/use-net-secrets-in-console-application/
        var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<ConfigurationRoot>()
            .AddEnvironmentVariables();
        var configurationRoot = builder.Build();

        return configurationRoot;
    }
}