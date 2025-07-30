using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinGetStudio.Models;

public class ChatMessage(string message, bool isUser)
{
    public string Message { get; set; } = message;
    public bool IsUser { get; set; } = isUser;
}
