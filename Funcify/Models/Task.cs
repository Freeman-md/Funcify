using System;
using System.Text.Json;

public class QueueTask
{
    public required string TaskId { get; set; }
    public required string ActionType { get; set; } // e.g., "Resize-Image"
    public required Product Product { get; set; }
    public DateTime CreatedAt { get; set; }

    public QueueTask()
    {
        TaskId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static QueueTask FromJson(string json)
    {
        return JsonSerializer.Deserialize<QueueTask>(json)!;
    }

    public override string ToString()
    {
        return $"QueueTask(TaskId: {TaskId}, ActionType: {ActionType}, Product: {Product}, CreatedAt: {CreatedAt})";
    }
}
