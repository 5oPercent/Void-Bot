using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public class SerilogSetting : IConfigurationSetting
{
    public class SeqData
    {
        public string ServerUrl { get; set; } = string.Empty;
        
        public string ApiKey { get; set; } = string.Empty;
    }
    
    public string Application { get; set; }
    
    public SeqData Seq { get; set; }

    public SerilogSetting(IConfiguration configuration)
    {
        Application = configuration["Serilog:Application"] ?? "Void.Bot.Core";
        Seq = new SeqData
        {
            ServerUrl = configuration["Serilog:Seq:ServerUrl"] ?? string.Empty,
            ApiKey = configuration["Serilog:Seq:ApiKey"] ?? string.Empty
        };
    }
}