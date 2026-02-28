using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
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
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public string Message { get; set; } = string.Empty;

    private string FormattedMessage => FormatJson(Message);

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

    private async Task CopyFormattedAsync()
    {
        await JsRuntime.InvokeVoidAsync("clipboardInterop.writeText", FormattedMessage);
        Snackbar.Add("Copied to clipboard", Severity.Success);
    }

    private void Cancel() => MudDialog.Close(DialogResult.Cancel());
}
