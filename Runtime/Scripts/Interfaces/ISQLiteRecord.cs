namespace ParkMinPackages.SQLiteToolkit
{
	public interface ISQLiteRecord<TKey>
	{
		TKey Id { get; }
	}
}
