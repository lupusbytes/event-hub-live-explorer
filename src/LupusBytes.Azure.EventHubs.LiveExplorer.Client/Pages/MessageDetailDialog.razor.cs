using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Client.Pages;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "Impossible")]
public sealed partial class MessageDetailDialog : ComponentBase
{
    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public EventHubMessage EventHubMessage { get; set; } = null!;

    private bool HasMetadata =>
        EventHubMessage.ContentType is not null ||
        EventHubMessage.CorrelationId is not null ||
        EventHubMessage.MessageId is not null ||
        EventHubMessage.Properties is { Count: > 0 };

    private string FormattedMessage => FormatJson(EventHubMessage.Message);

    private MarkupString HighlightedMessage => new(HighlightJson(EventHubMessage.Message));

    private static string FormatJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(doc, IndentedOptions);
        }
        catch (JsonException)
        {
            return raw;
        }
    }

    private static string HighlightJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var sb = new StringBuilder();
            HighlightElement(doc.RootElement, sb, 0);

            return sb.ToString();
        }
        catch (JsonException)
        {
            return WebUtility.HtmlEncode(raw);
        }
    }

    private static void HighlightElement(
        JsonElement element,
        StringBuilder sb,
        int indent)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                HighlightObject(element, sb, indent);
                break;
            case JsonValueKind.Array:
                HighlightArray(element, sb, indent);
                break;
            case JsonValueKind.String:
                sb.Append("<span class=\"json-string\">\"").Append(WebUtility.HtmlEncode(element.GetString())).Append("\"</span>");
                break;
            case JsonValueKind.Number:
                sb.Append("<span class=\"json-number\">").Append(element.GetRawText()).Append("</span>");
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                sb.Append("<span class=\"json-boolean\">").Append(element.GetRawText()).Append("</span>");
                break;
            case JsonValueKind.Null:
                sb.Append("<span class=\"json-null\">null</span>");
                break;
        }
    }

    private static void HighlightObject(
        JsonElement element,
        StringBuilder sb,
        int indent)
    {
        var pad = new string(' ', indent * 2);
        sb.Append("<span class=\"json-brace\">{</span>\n");
        var props = element.EnumerateObject().ToList();

        for (var i = 0; i < props.Count; i++)
        {
            var prop = props[i];

            sb.Append(pad).Append("  ");
            sb.Append("<span class=\"json-key\">\"").Append(WebUtility.HtmlEncode(prop.Name)).Append("\"</span>");
            sb.Append("<span class=\"json-colon\">: </span>");

            HighlightElement(prop.Value, sb, indent + 1);

            if (i < props.Count - 1)
            {
                sb.Append(',');
            }

            sb.Append('\n');
        }

        sb.Append(pad).Append("<span class=\"json-brace\">}</span>");
    }

    private static void HighlightArray(
        JsonElement element,
        StringBuilder sb,
        int indent)
    {
        var pad = new string(' ', indent * 2);

        sb.Append("<span class=\"json-bracket\">[</span>\n");

        var items = element.EnumerateArray().ToList();

        for (var i = 0; i < items.Count; i++)
        {
            sb.Append(pad).Append("  ");
            HighlightElement(items[i], sb, indent + 1);

            if (i < items.Count - 1)
            {
                sb.Append(',');
            }

            sb.Append('\n');
        }

        sb.Append(pad).Append("<span class=\"json-bracket\">]</span>");
    }

    private async Task CopyFormattedAsync()
    {
        await JS.InvokeVoidAsync("clipboardInterop.writeText", FormattedMessage);
        Snackbar.Add("Copied to clipboard", Severity.Success);
    }

    private void Cancel() => MudDialog.Close(DialogResult.Cancel());
}
