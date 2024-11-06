//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Auth.OAuth2.Flows;
//using Google.Apis.Drive.v3;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Mvc;

//namespace AppSysoHelp.Controllers
//{
//    public class DriveController : Controller
//    {
//        private readonly DriveService _driveService;

//        public DriveController(DriveService driveService)
//        {
//            _driveService = driveService;
//        }

//        public async Task<IActionResult> Index(CancellationToken cancellationToken)
//        {
//            // Verifica se o usuário está autenticado
//            var result = await HttpContext.AuthenticateAsync();
//            if (!result.Succeeded)
//            {
//                return Challenge(); // Redireciona para o login se não estiver autenticado
//            }

//            // O DriveService já foi configurado com as credenciais do usuário
//            ViewBag.Arquivos = ListarArquivos(_driveService);

//            return View();
//        }

//        private string ListarArquivos(DriveService service)
//        {
//            var request = service.Files.List();
//            request.Fields = "files(id, name)";
//            var resultado = request.Execute();
//            var arquivos = resultado.Files;

//            if (arquivos != null && arquivos.Any())
//            {
//                foreach (var arquivo in arquivos)
//                {
//                    Console.WriteLine(arquivo.Name);
//                }
//            }
//            return "";
//        }
//    }
//}
