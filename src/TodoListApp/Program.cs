using System.IO;
using System.Linq;
using System.Text.Json;

namespace TodoListApp;

internal class TodoItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public void MarkComplete()
    {
        IsComplete = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkIncomplete()
    {
        IsComplete = false;
        CompletedAt = null;
    }
}

internal class TodoList
{
    private const string StorageFileName = "todos.json";
    private readonly List<TodoItem> _items = new();

    public TodoList()
    {
        Load();
    }

    public IReadOnlyList<TodoItem> Items => _items;

    public void Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A task must have a description.", nameof(title));
        }

        _items.Add(new TodoItem
        {
            Title = title.Trim()
        });

        Save();
    }

    public bool ToggleCompletion(int index)
    {
        if (!TryGetItem(index, out var item))
        {
            return false;
        }

        if (item.IsComplete)
        {
            item.MarkIncomplete();
        }
        else
        {
            item.MarkComplete();
        }

        Save();
        return true;
    }

    public bool Remove(int index)
    {
        if (!TryGetItem(index, out var item))
        {
            return false;
        }

        _items.Remove(item);
        Save();
        return true;
    }

    private bool TryGetItem(int index, out TodoItem item)
    {
        var adjustedIndex = index - 1;
        if (adjustedIndex < 0 || adjustedIndex >= _items.Count)
        {
            item = default!;
            return false;
        }

        item = _items[adjustedIndex];
        return true;
    }

    private void Load()
    {
        if (!File.Exists(StorageFileName))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(StorageFileName);
            var items = JsonSerializer.Deserialize<List<TodoItem>>(stream);
            if (items is not null)
            {
                _items.Clear();
                _items.AddRange(items);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load saved todos: {ex.Message}");
        }
    }

    private void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(StorageFileName));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(StorageFileName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save todos: {ex.Message}");
        }
    }
}

public static class Program
{
    private static readonly TodoList Todos = new();
    private static string? _statusMessage;

    public static void Main()
    {
        var exitRequested = false;
        while (!exitRequested)
        {
            DisplayDashboard();
            DisplayMenu();
            Console.Write("Select an option: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    DisplayTasks();
                    break;
                case "2":
                    AddTask();
                    break;
                case "3":
                    ToggleTaskCompletion();
                    break;
                case "4":
                    RemoveTask();
                    break;
                case "5":
                    exitRequested = true;
                    break;
                default:
                    _statusMessage = "Unknown option. Please choose between 1 and 5.";
                    break;
            }
        }

        Console.WriteLine("Goodbye!");
    }

    private static void DisplayMenu()
    {
        Console.WriteLine("===========================");
        Console.WriteLine("1. View tasks");
        Console.WriteLine("2. Add a task");
        Console.WriteLine("3. Toggle task completion");
        Console.WriteLine("4. Remove a task");
        Console.WriteLine("5. Exit");
        Console.WriteLine("===========================");
    }

    private static void DisplayDashboard()
    {
        ClearConsoleSafely();
        Console.WriteLine("Simple .NET Todo List\n");

        var total = Todos.Items.Count;
        var completed = Todos.Items.Count(todo => todo.IsComplete);
        var pending = total - completed;
        var percent = total == 0
            ? 0
            : (int)Math.Round(completed / (double)total * 100, MidpointRounding.AwayFromZero);

        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Task Overview");
        Console.WriteLine("-------------");
        Console.ForegroundColor = originalColor;

        Console.WriteLine($"Total: {total,3}   Completed: {completed,3}   Pending: {pending,3}");
        Console.WriteLine(RenderProgressBar(percent));
        if (!string.IsNullOrWhiteSpace(_statusMessage))
        {
            Console.WriteLine();
            Console.WriteLine(_statusMessage);
            _statusMessage = null;
        }
        Console.WriteLine();
    }

    private static string RenderProgressBar(int percent)
    {
        const int barWidth = 24;
        var filledWidth = (int)Math.Round(percent / 100d * barWidth, MidpointRounding.AwayFromZero);
        filledWidth = Math.Clamp(filledWidth, 0, barWidth);

        var filledSection = new string('█', filledWidth);
        var emptySection = new string('░', barWidth - filledWidth);
        return $"[{filledSection}{emptySection}] {percent,3}% complete";
    }

    private static void DisplayTasks()
    {
        if (Todos.Items.Count == 0)
        {
            _statusMessage = "No tasks found. Add your first task!";
            return;
        }

        for (var i = 0; i < Todos.Items.Count; i++)
        {
            var todo = Todos.Items[i];
            var status = todo.IsComplete ? "[x]" : "[ ]";
            Console.WriteLine($"{i + 1}. {status} {todo.Title}");
        }

        Console.WriteLine();
        PauseForUser();
    }

    private static void AddTask()
    {
        Console.Write("Enter the task description: ");
        var title = Console.ReadLine();

        try
        {
            Todos.Add(title ?? string.Empty);
            _statusMessage = "Task added successfully!";
        }
        catch (ArgumentException ex)
        {
            _statusMessage = $"Could not add task: {ex.Message}";
        }
    }

    private static void ToggleTaskCompletion()
    {
        if (!TryReadTaskIndex(out var index))
        {
            return;
        }

        if (Todos.ToggleCompletion(index))
        {
            _statusMessage = "Updated task completion status.";
        }
        else
        {
            _statusMessage = "Task not found.";
        }
    }

    private static void RemoveTask()
    {
        if (!TryReadTaskIndex(out var index))
        {
            return;
        }

        if (Todos.Remove(index))
        {
            _statusMessage = "Task removed.";
        }
        else
        {
            _statusMessage = "Task not found.";
        }
    }

    private static bool TryReadTaskIndex(out int index)
    {
        if (Todos.Items.Count == 0)
        {
            _statusMessage = "No tasks to select.";
            index = -1;
            return false;
        }

        Console.Write("Enter the task number: ");
        var input = Console.ReadLine();

        if (!int.TryParse(input, out index))
        {
            _statusMessage = "Please enter a valid number.";
            return false;
        }

        return true;
    }

    private static void PauseForUser()
    {
        Console.WriteLine("Press Enter to return to the menu...");
        Console.ReadLine();
    }

    private static void ClearConsoleSafely()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Ignore consoles that do not support clearing (e.g., redirected output).
        }
        catch (PlatformNotSupportedException)
        {
            // Some environments (like certain Linux terminals) may not support clearing.
        }
    }
}
