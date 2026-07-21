using Discord;

namespace GrillBot.Contracts.Bot.Events.Messages;

public class DiscordMessageFile
{
    public string Filename { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsSpoiler { get; set; }
    public byte[] Data { get; set; } = null!;

    public DiscordMessageFile()
    {
    }

    public DiscordMessageFile(string filename, bool isSpoiler, byte[] data, string? description = null)
    {
        Filename = filename;
        Description = description;
        IsSpoiler = isSpoiler;
        Data = data;
    }

    public FileAttachment ToFileAttachment()
        => new(new MemoryStream(Data), Filename, Description, IsSpoiler);

    public static DiscordMessageFile FromAttachment(IAttachment attachment, byte[] data)
        => new(attachment.Filename, attachment.IsSpoiler(), data, attachment.Description);

    public static DiscordMessageFile FromAttachment(FileAttachment attachment)
    {
        using var ms = new MemoryStream();
        attachment.Stream.CopyTo(ms);

        return new(attachment.FileName, attachment.IsSpoiler, ms.ToArray(), attachment.Description);
    }
}
