namespace HeadBet.Core.Domain.Interfaces;

public interface ISettingsProvider
{
    Task<string?> GetJsonAsync(string name, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string name, CancellationToken ct = default) where T : class;
}
