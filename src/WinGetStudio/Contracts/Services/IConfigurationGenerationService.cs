using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinGetStudio.Contracts.Services;

public interface IConfigurationGenerationService
{
    public Task<string> GenerateConfigurationAsync(string input);
}
