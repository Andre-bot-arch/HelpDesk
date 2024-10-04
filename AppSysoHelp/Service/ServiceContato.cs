using AppSysoHelp.Models.ViewModels;
using System.Net.Mail;

namespace AppSysoHelp.Service
{
    public class ServiceContato
    {
        private readonly IConfiguration _configuration;
        private readonly string email;
        private readonly string host;
        private readonly string port;
        private readonly string senha;
        private readonly string rota;
        private readonly string imagem;
        public ServiceContato(IConfiguration configuration)
        {
            _configuration = configuration;
            email = _configuration.GetValue<string>("Email:email");
            port = _configuration.GetValue<string>("Email:porta");
            host = _configuration.GetValue<string>("Email:host");
            senha = _configuration.GetValue<string>("Email:senha");
            rota = _configuration.GetValue<string>("Email:rota");
            imagem = _configuration.GetValue<string>("Email:imagem");
        }

        public bool EnviarEmail(string name, string emailForm, string phone, string message)
        {
            try
            {
                var fromAddress = new MailAddress(email, "Contato SysoTecnologia");
                var toAddress = new MailAddress("sysotecnologia@gmail.com", "Cliente SysoTecnologia");
                const string subject = "Novo contato pelo formulário";

                var mail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject
                };
                if (String.IsNullOrEmpty(name))
                    name = "Anônimo";
                if (String.IsNullOrEmpty(emailForm))
                    emailForm = "Anônimo";
                if (String.IsNullOrEmpty(phone))
                    phone = "(00) 00000-0000";
                if (String.IsNullOrEmpty(message))
                    message = "Sem mensagem!";


                //"https://i.imgur.com/HljFU6v.png  pague menos"
                string data = DateTime.Now.AddHours(4).ToString("dd/MM/yyyy HH:mm");
                //hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(data+"||"+ email));
                // Criando o corpo do e-mail em HTML com formatação CSS
                var htmlBody = "<html><head><style>body { font-family: Arial, sans-serif; color: #333; }</style></head><body>"
                             + "<h3>Novo contato pelo formulário, no site sysotecnologia:</h3>"
                             + $"<p>Nome: {name} <br /> Email: {emailForm} <br /> Telefone para contato: {phone}</p>"
                             + $"<p>Mensagem: <br /> {message}</p>"
                             + "</body></html>";

                // Criando uma visualização alternativa em HTML
                var alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                mail.AlternateViews.Add(alternateView);

                var smtp = new SmtpClient
                {
                    Host = host,
                    Port = Convert.ToInt32(port),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(fromAddress.Address, senha)
                };

                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                var erro = ex.Message;
                return false;
            }
        }
    }
}
