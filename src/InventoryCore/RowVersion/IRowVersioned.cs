namespace InventoryCore.RowVersion;

public interface IRowVersioned
{
    long RowVersion { get; set; }
}
