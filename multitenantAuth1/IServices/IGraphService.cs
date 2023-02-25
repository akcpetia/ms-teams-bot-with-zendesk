namespace multitenantAuth1.IServices
{
    public interface IGraphService
    {
        Task<dynamic> GetUserData();
        Task<dynamic> CreateSubscription();
        Task<dynamic> GetAllSubscription();
        Task<dynamic> DeleteSubscription(string id);
    }
}
