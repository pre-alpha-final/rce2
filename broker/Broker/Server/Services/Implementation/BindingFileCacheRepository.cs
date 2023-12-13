using Broker.Shared.Model;
using Newtonsoft.Json;

namespace Broker.Server.Services.Implementation;

// TODO make better persistence
public class BindingFileCacheRepository : IBindingRepository
{
    private const string FileName = "bindings.txt";
    private readonly object _lock = new object();

    public BindingFileCacheRepository()
    {
        LoadFromFile();
    }

    public HashSet<Binding> Bindings { get; set; } = new();

    public List<Binding> GetAll()
    {
        return Bindings.ToList();
    }

    public List<Binding> GetBindingsFrom(Guid id, string contact)
    {
        return Bindings
            .Where(e => e.OutId == id)
            .Where(e => e.OutContact == contact)
            .ToList();
    }

    public bool AddBinding(Binding binding)
    {
        lock (_lock)
        {
            var result = Bindings.Add(binding);
            SaveToFile();

            return result;
        }
    }

    public bool UpdateBinding(Binding binding)
    {
        lock (_lock)
        {
            Bindings.Remove(binding);
            var result = Bindings.Add(binding);
            SaveToFile();

            return result;
        }
    }

    public bool DeleteBinding(Binding binding)
    {
        lock (_lock)
        {
            var result = Bindings.Remove(binding);
            SaveToFile();

            return result;
        }
    }

    private void LoadFromFile()
    {
        try
        {
            var content = File.ReadAllText(FileName);
            Bindings = JsonConvert.DeserializeObject<HashSet<Binding>>(content);
        }
        catch (Exception e)
        {
            // ignore
        }
    }

    private void SaveToFile()
    {
        try
        {
            File.WriteAllText(FileName, JsonConvert.SerializeObject(Bindings));
        }
        catch (Exception e)
        {
            // ignore
        }
    }
}
