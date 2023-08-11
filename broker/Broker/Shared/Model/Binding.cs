namespace Broker.Shared.Model;

public class Binding
{
    public Guid OutId { get; set; }
    public string OutName { get; set; }
    public string OutContact { get; set; }
    public Guid InId { get; set; }
    public string InName { get; set; }
    public string InContact { get; set; }
    public bool IsActive { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj.GetType() != GetType()) return false;

        return
            ((Binding)obj).OutId.Equals(OutId) &&
            ((Binding)obj).OutContact == OutContact &&
            ((Binding)obj).InId.Equals(InId) &&
            ((Binding)obj).InContact == InContact;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(OutId, OutContact, InId, InContact);
    }
}
