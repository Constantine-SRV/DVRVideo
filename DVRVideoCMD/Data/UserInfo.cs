/// <summary>
/// Пользователь Telegram-бота / Web-API.
/// </summary>
public class UserInfo
{
    public long Id { get; set; }
    public string Name { get; set; }
    // При необходимости позже можно добавить список прав:
    // public List<string> Rights { get; set; } = new();
}
