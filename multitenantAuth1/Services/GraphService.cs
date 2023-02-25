using Azure.Identity;
using Microsoft.Graph;
using multitenantAuth1.IServices;
using multitenantAuth1.Model;

namespace multitenantAuth1.Services
{
    public class GraphService: IGraphService
    {
        private readonly IConfiguration Configuration;

        private string clientId;
        private string clientSecret;
        private string tenantId;
        private static Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
        private static Timer? subscriptionTimer = null;

        public GraphService(IConfiguration configuration)
        {
            Configuration = configuration;
            tenantId= Configuration["AzureAd:TenantId"];
            clientId = Configuration["AzureAd:ClientId"];
            clientSecret = Configuration["AzureAd:ClientSecret"];


        }
        public async Task<dynamic> GetUserData()
        {
            HttpClient client = new HttpClient();
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
                CommonMethods.GetSession().tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
            var users = await graphClient.Users
                        .Request()
                        .GetAsync();

            return users;
         
        }
        public async Task<dynamic> CreateSubscription()
        {
            HttpClient client = new HttpClient();
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
                CommonMethods.GetSession().tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);



            var subscription = new Subscription
            {
                ChangeType = "created,updated",
                NotificationUrl = "https://multitenantauthbackend.azurewebsites.net/Notification/postSubscription",
                Resource = "/users",
                ExpirationDateTime = DateTime.Now.AddDays(3),
                ClientState = "SecretClientState"
            };

            var newSubscription = await graphClient.Subscriptions
                .Request()
                .AddAsync(subscription);

            Subscriptions[newSubscription.Id] = newSubscription;

            if (subscriptionTimer == null)
            {
                subscriptionTimer = new Timer(CheckSubscriptions, null, 5000, 15000);
            }


            return $"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}";

        }
        private void CheckSubscriptions(Object? stateInfo)
        {
            AutoResetEvent? autoEvent = stateInfo as AutoResetEvent;

            Console.WriteLine($"Checking subscriptions {DateTime.Now.ToString("h:mm:ss.fff")}");
            Console.WriteLine($"Current subscription count {Subscriptions.Count()}");

            foreach (var subscription in Subscriptions)
            {
                // if the subscription expires in the next 2 min, renew it
                if (subscription.Value.ExpirationDateTime < DateTime.UtcNow.AddMinutes(2))
                {
                    RenewSubscription(subscription.Value);
                }
            }
        }

        private async void RenewSubscription(Subscription subscription)
        {
            Console.WriteLine($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

            //var graphServiceClient = GetGraphClient();
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
                CommonMethods.GetSession().tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);


            var newSubscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddMinutes(5)
            };

            await graphClient
              .Subscriptions[subscription.Id]
              .Request()
              .UpdateAsync(newSubscription);

            subscription.ExpirationDateTime = newSubscription.ExpirationDateTime;
            Console.WriteLine($"Renewed subscription: {subscription.Id}, New Expiration: {subscription.ExpirationDateTime}");
        }

        public async Task<dynamic> GetAllSubscription()
        {
            HttpClient client = new HttpClient();
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
              CommonMethods.GetSession().tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);




            var Subscriptions = await graphClient.Subscriptions
                                .Request()
                                .GetAsync();


            return Subscriptions;

        }
        public async Task<dynamic> DeleteSubscription(string id)
        {
            HttpClient client = new HttpClient();
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
            CommonMethods.GetSession().tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);


            await graphClient.Subscriptions[id]
                     .Request()
                     .DeleteAsync();


            return "Ok";

        }





    }
}
