namespace WebAPISample.Services.Contracts
{
    public interface IMessageService<T>
    {
        Task DeleteMessageAsync(string id);

        Task<Dictionary<string, T?>> ReceiveMessageAsync(int maxMessages = 1);

        Task SendMessage(T message);
    }
}