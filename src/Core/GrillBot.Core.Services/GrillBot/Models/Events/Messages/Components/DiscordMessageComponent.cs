namespace GrillBot.Models.Events.Messages.Components;

public class DiscordMessageComponent
{
    public List<ButtonComponent> Buttons { get; set; } = [];
    public List<string> ComponentOrder { get; set; } = [];

    public void AddButton(ButtonComponent component)
        => AddComponent(() => Buttons, component, "Button");

    private void AddComponent(Func<System.Collections.IList> listSelector, object item, string componentType)
    {
        if (ComponentOrder.Count > Discord.ComponentBuilder.MaxActionRowCount * Discord.ActionRowBuilder.MaxChildCount)
            throw new ArgumentException("Unable to add next component. List is full.");

        var list = listSelector();
        var index = list.Count;

        list.Add(item);
        ComponentOrder.Add($"{componentType}/{index}");
    }

    public IEnumerable<Discord.IMessageComponent> BuildComponents()
    {
        return ComponentOrder
            .Select(o => o.Split('/', StringSplitOptions.TrimEntries))
            .Select(o => o[0] switch
            {
                "Button" => Buttons[Convert.ToInt32(o[1])].ToDiscordComponent(),
                _ => null
            })
            .Where(o => o is not null)!;
    }

    public static DiscordMessageComponent? FromComponents(IEnumerable<Discord.IMessageComponent> components)
    {
        var result = new DiscordMessageComponent();

        var buttonsQuery = components
            .Where(o => o.Type == Discord.ComponentType.Button)
            .OfType<Discord.ButtonComponent>()
            .Select(ButtonComponent.FromDiscordComponent);

        foreach (var button in buttonsQuery)
            result.AddButton(button);

        return result.ComponentOrder.Count > 0 ? result : null;
    }
}
