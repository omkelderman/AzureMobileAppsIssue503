using Microsoft.Datasync.Client;

namespace Client;

public class TodoItem : DatasyncClientData
{
    public string Title { get; set; } = "";

    public bool IsComplete { get; set; }
}