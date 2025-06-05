using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Parses incoming Telegram messages and dispatches them to the appropriate command.
/// </summary>
public class TelegramCommandRouter
{
    private readonly List<TelegramCommand> _commands;

    public TelegramCommandRouter(TelegramCommandProcessor handlers)
    {
        // Заполняем список команд (можно расширять)
        _commands = new List<TelegramCommand>
        {
            new TelegramCommand {
                Order = 1,
                Keywords = new List<string> { "sw", "swimming" },
                MinAccessLevel = 0,
                Handler = handlers.HandleSwim
            },
            new TelegramCommand {
                Order = 2,
                Keywords = new List<string> { "home", "temp" },
                MinAccessLevel = 0,
                Handler = handlers.HandleHomeOrTemp
            },
            new TelegramCommand {
                Order = 100,
                Keywords = new List<string> { "help", "/help" },
                MinAccessLevel = 0,
                Handler = handlers.HandleHelp
            },
            new TelegramCommand {
                Order = 3,
                Keywords = new List<string> { "ch", "cam" },
                MinAccessLevel = 3,
                Handler = handlers.HandleChannel
            }
            // Добавишь новые команды — просто добавь сюда.
        }.OrderBy(c => c.Order).ToList();
    }

    // Главный обработчик
    public async Task HandleAnyCommand(long chatId, string text, int accessLevel)
    {
        _ = MongoLogService.LogAsync(chatId, "UserMessage", "text: " + text);

        foreach (var cmd in _commands)
        {
            if (cmd.Keywords.Any(k => text == k || text.Contains(k)))
            {
                if (accessLevel < cmd.MinAccessLevel)
                {
                    await TelegramMessageSender.SendMessageAsync(
                        AppSettingsService.TelegramToken,
                        chatId,
                        "Access denied. Your access level is insufficient for this command."
                    );
                    return;
                }
                await cmd.Handler(chatId, text);
                return;
            }
        }

        // Если не совпало ни с одной командой:
        await TelegramMessageSender.SendMessageAsync(
            AppSettingsService.TelegramToken,
            chatId,
            "Unknown command. Type 'help' for available commands."
        );
    }
}


public class TelegramCommand
{
    public int Order { get; set; }
    public List<string> Keywords { get; set; }
    public int MinAccessLevel { get; set; }
    public Func<long, string, Task> Handler { get; set; }
}

