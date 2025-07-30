using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGetStudio.Contracts.Services;

namespace WinGetStudio.Services;

public class ConfigurationGenerationService : IConfigurationGenerationService
{
    public async Task<string> GenerateConfigurationAsync(string input)
    {
        return "\"Generated Configuration\"";
    }
}
