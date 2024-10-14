
//namespace AppSysoHelp.Service
//{
//    public class ServiceGoogle : Google.Apis.Auth.OAuth2.Mvc.FlowMetadata
//    {
//        private static readonly Google.Apis.Auth.OAuth2.Flows.IAuthorizationCodeFlow flow =
//            new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow(new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow.Initializer
//            {
//                ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
//                {
//                    ClientId = "183697080595-6c6ufjdmr5qutgcjapj1nktggoj11imh.apps.googleusercontent.com",
//                    ClientSecret = "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"
//                },
//                Scopes = new[] { Google.Apis.Drive.v3.DriveService.Scope.Drive },
//                DataStore = new Google.Apis.Util.Store.FileDataStore("Drive.Api.Auth.Store")
//            });

//        public override string GetUserId(System.Web.Mvc.Controller controller)
//        {
//            // In this sample we use the session to store the user identifiers.
//            // That's not the best practice, because you should have a logic to identify
//            // a user. You might want to use "OpenID Connect".
//            // You can read more about the protocol in the following link:
//            // https://developers.google.com/accounts/docs/OAuth2Login.
//            var user = controller.Session["user"];
//            if (user == null)
//            {
//                user = Guid.NewGuid();
//                controller.Session["user"] = user;
//            }
//            return user.ToString();

//        }

//        public override Google.Apis.Auth.OAuth2.Flows.IAuthorizationCodeFlow Flow
//        {
//            get { return flow; }
//        }
//    }
//}
