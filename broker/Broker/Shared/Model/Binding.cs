namespace Broker.Shared.Model;

public class Binding
{
    public Guid OutId { get; set; }
    public string OutName { get; set; }
    public string OutContact { get; set; }
    public Guid InId { get; set; }
    public string InName { get; set; }
    public string InContact { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, this)) return true;
        if (ReferenceEquals(obj, null)) return false;
        if (ReferenceEquals(this, null)) return false;
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
