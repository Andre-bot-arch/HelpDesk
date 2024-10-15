using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class DriveController : Controller
    {
        private static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        private const string ApplicationName = "Drive API Sample";

        public async Task<IActionResult> Index()
        {
            var arquivos = await ListarArquivos();
            return View(arquivos);
        }

        private async Task<IList<Google.Apis.Drive.v3.Data.File>> ListarArquivos()
        {
            // Caminho para o arquivo JSON com as credenciais
            string credenciaisPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\client_secret_183697080595-6c6ufjdmr5qutgcjapj1nktggoj11imh.apps.googleusercontent.com.json");

            // Carregar credenciais e obter token de acesso
            UserCredential credential;
            using (var stream = new FileStream(credenciaisPath, FileMode.Open, FileAccess.Read))
            {
                var credPath = Path.Combine(Directory.GetCurrentDirectory(), "token.json");

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            // Inicializar o serviço Google Drive com as credenciais
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Solicitar a lista de arquivos do Google Drive
            var request = service.Files.List();
            request.Fields = "nextPageToken, files(id, name, mimeType)";
            var result = await request.ExecuteAsync();

            return result.Files;
        }
    }
}
