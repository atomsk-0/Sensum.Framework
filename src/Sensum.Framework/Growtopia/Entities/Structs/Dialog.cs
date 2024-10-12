using Cysharp.Text;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public class Dialog : IResourceLifecycle
{
    public readonly List<IDialogEntity> Entities = [];
    public readonly List<string> EmbedData = [];

    public string? Name;
    public string? Raw;

    public void Parse(string raw)
    {
        Reset();
        Raw = raw;
        foreach (string line in raw.Split('\n'))
        {
            string[] tokens = line.Split('|');
            string type = tokens[0];
            switch (type)
            {
                case "add_label_with_icon":
                    Entities.Add(new DialogText(tokens[2]));
                    break;
                case "add_textbox":
                    Entities.Add(new DialogText(tokens[1]));
                    break;
                case "add_text_input":
                    Entities.Add(new DialogTextInput(tokens[1], tokens[2], uint.Parse(tokens[4])));
                    break;
                case "add_button":
                    Entities.Add(new DialogButton(tokens[1], tokens[2], false));
                    break;
                case "add_searchable_item_list":
                    string[] data = tokens[1].Split(',');
                    int index = 0;
                    for (int i = 0; i < data.Length; i += 2)
                    {
                        ushort itemId = ushort.Parse(data[i]);
                        byte amount = byte.Parse(data[i + 1]);
                        Entities.Add(new SearchableItem(itemId, amount, (ushort)index));
                        index++;
                    }
                    break;
                case "end_dialog":
                    Name = tokens[1];
                    int c = 0;
                    foreach (string token in tokens)
                    {
                        if (c > 1 && string.IsNullOrEmpty(token) == false)
                        {
                            Entities.Add(new DialogButton(token, token, true));
                        }
                        c++;
                    }
                    break;
                case "embed_data":
                    EmbedData.Add(line.Replace("embed_data|", ""));
                    break;
            }
        }
    }

    public void Reset()
    {
        Entities.Clear();
        EmbedData.Clear();
        Raw = "";
        Name = "";
    }

    public void Destroy()
    {
        Reset();
    }
}

public class DialogTextInput(string id, string hint, uint maxLength) : IDialogEntity
{
    public EntityType EntityType => EntityType.TextInput;
    public string Id => id;
    public string Hint => hint;
    public string Text = "";
    public uint MaxLength => maxLength;
}

public readonly struct SearchableItem(ushort itemId, byte amount, ushort index) : IDialogEntity
{
    public EntityType EntityType => EntityType.SearchableItem;
    public ushort ItemId => itemId;
    public byte Amount => amount;
    public ushort Index => index;
    public string ButtonId => $"searchableItemListButton_{itemId}_{amount}_{index}";
}

public readonly struct DialogText(string text) : IDialogEntity
{
    public EntityType EntityType => EntityType.Text;
    public string Text => text;
}

public readonly struct DialogButton(string id, string text, bool endDialog) : IDialogEntity
{
    public EntityType EntityType => EntityType.Button;
    public string Id => id;
    public string Text => text;
    public bool EndDialog => endDialog;

    public void OnClick(ENetClient client, Dialog dialog)
    {
        var stringBuilder = ZString.CreateStringBuilder();
        stringBuilder.Append($"action|dialog_return\ndialog_name|{dialog.Name}\n");
        foreach (string embed in dialog.EmbedData)
        {
            stringBuilder.AppendLine(embed);
        }
        foreach (IDialogEntity entity in dialog.Entities)
        {
            if (entity.EntityType != EntityType.TextInput) continue;
            DialogTextInput textInput = (DialogTextInput)entity;
            stringBuilder.AppendLine($"{textInput.Id}|{textInput.Text}");
        }
        if (EndDialog)
        {
            client.Dialog.Reset();
            client.SendGenericText(stringBuilder.ToString());
        }
        stringBuilder.AppendLine($"buttonClicked|{Id}");
        client.Dialog.Reset();
        client.SendGenericText(stringBuilder.ToString());
    }
}

public interface IDialogEntity
{
    public EntityType EntityType { get; }
}

public enum EntityType
{
    Text,
    TextInput,
    Button,
    SearchableItem
}