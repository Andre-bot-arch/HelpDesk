using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace AppSysoHelp.Service
{
    public class ServiceLicenca
    {
        private readonly HelpdesksysoContext _context;
        public ServiceLicenca(HelpdesksysoContext context)
        {
            _context = context;
        }

        internal Licencas BuscarLicencaPorId(long id)
        {
            return _context.Licencas.FirstOrDefault(a => a.LicencaId == id);
        }

        internal string GerarHash(long length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            // Insere os hífens após cada 4 caracteres
            for (int i = 4; i < result.Length; i += 5)
            {
                if (i < result.Length)
                {
                    result = InsertChar(result, i, '-');
                }
            }

            return new string(result);
        }

        static char[] InsertChar(char[] array, int index, char charToInsert)
        {
            var newArray = new char[array.Length + 1];
            Array.Copy(array, 0, newArray, 0, index);
            newArray[index] = charToInsert;
            Array.Copy(array, index, newArray, index + 1, array.Length - index);
            return newArray;
        }

        internal ViewModelApiLicenca VerificarChave(string licenca, string apelido, string serial)
        {
            var contrato = _context.Contratos
                .Include(c => c.FkCliente)
                .Include(c => c.Licencas)
                .ThenInclude(c => c.Dispositivos)
                .FirstOrDefault(c => c.Licencas.Any(l => l.Hash == licenca));

            if (contrato == null)
            {
                var retorno1 = new ViewModelApiLicenca
                {
                    Status = false,
                    Chave = licenca,
                    Mensagem = "Chave inválida",
                    Cnpj = "",
                    Empresa = "",
                    Url = "",
                };
                return retorno1;
            }

            var retorno = new ViewModelApiLicenca
            {
                Status = false,
                Chave = licenca,
                Mensagem = "",
                Cnpj = contrato.FkCliente.Documento,
                Empresa = contrato.FkCliente.NomeCliente,
                Url = contrato.Licencas.FirstOrDefault(a=> a.Hash == licenca).Urlacesso,
            };

            //verificar chave
            if(contrato.SituacaoContrato.Trim() == "Inativo")
            {
                retorno.Mensagem = "Contrato Cancelado";
                return retorno;
            }

            //verificar se a licença esta ativa
            else if (contrato.Licencas.FirstOrDefault(a => a.Hash == licenca).Ativo == false)
            {
                retorno.Mensagem = "Chave Revogada";
                return retorno;
            }

            //verificar se existe pontos disponiveis
            int totalDispositivos = contrato.Licencas.Sum(l => l.Dispositivos?.Count() ?? 0);

            //verificar se o dispositivo ja foi adicionado anteriormente e se foi realizar o update
            var validador = contrato.Licencas.FirstOrDefault(c => c.Dispositivos.Any(l => l.Equipamento == serial) && c.Hash == licenca);

            if (totalDispositivos >= contrato.PontosContratados && validador == null)
            {
                retorno.Mensagem = "Limite de pontos Excedido";
                return retorno;
            }

            
            else if (validador != null && UpdateDispositivo(validador, apelido, serial))
            {
                retorno.Mensagem = "1 - Ativa";
                retorno.Status = true;
                return retorno;
            }
            else if (GravarDispositivos(contrato.Licencas.FirstOrDefault(a => a.Hash == licenca), apelido, serial))
            {
                retorno.Mensagem = "1 - Ativa";
                retorno.Status = true;
                return retorno;
            }

            retorno.Mensagem = "Erro na Gravação";
            retorno.Status = false;
            return retorno;
        }

        internal bool UpdateDispositivo(Licencas? licencas, string apelido = "", string serial = "")
        {
            var dispositivo = licencas.Dispositivos.FirstOrDefault(a => a.Equipamento == serial);
            dispositivo.Apelido = apelido.ToUpper();
            dispositivo.UltimoAcesso = DateTime.Now;
            _context.Update(dispositivo);
            _context.SaveChanges();
            return true;            
        }

        internal bool GravarDispositivos(Licencas? licencas, string apelido = "", string serial = "")
        {
            var dispositivo = new Dispositivos
            {
                Apelido = apelido.ToUpper(),
                Chave = licencas.Hash,
                Equipamento = serial,
                DataCriacao = DateTime.Now,
                FkLicenca = licencas.LicencaId,
                UltimoAcesso = DateTime.Now,
            };
            _context.Add(dispositivo);
            _context.SaveChanges();
            return true;
        }

        internal ViewModelApiLicenca VerificarLicencaLog(string licenca)
        {
            var contrato = _context.Contratos
                .Include(c => c.FkCliente)
                .Include(c => c.Licencas)
                .ThenInclude(c => c.Dispositivos)
                .FirstOrDefault(c => c.Licencas.Any(l => l.Hash == licenca));

            if (contrato == null)
            {
                var retorno1 = new ViewModelApiLicenca
                {
                    Status = false,
                    Chave = licenca,
                    Mensagem = "Chave inválida",
                    Cnpj = "",
                    Empresa = "",
                    Url = "",
                };
                return retorno1;
            }

            var retorno = new ViewModelApiLicenca
            {
                Status = false,
                Chave = licenca,
                Mensagem = "",
                Cnpj = contrato.FkCliente.Documento,
                Empresa = contrato.FkCliente.NomeCliente,
                Url = contrato.Licencas.FirstOrDefault(a => a.Hash == licenca).Urlacesso,
            };

            //verificar chave
            if (contrato.SituacaoContrato.Trim() == "Inativo")
            {
                retorno.Mensagem = "Contrato Inativo";
                return retorno;
            }

            //verificar se a licença esta ativa
            else if (contrato.Licencas.FirstOrDefault(a => a.Hash == licenca).Ativo == false)
            {
                retorno.Mensagem = "Chave Suspensa";
                return retorno;
            }
            else if (contrato.Licencas.FirstOrDefault(a => a.Hash == licenca).Ativo == true && contrato.SituacaoContrato.Trim() == "PENDENTE")
            {
                retorno.Status = true;
                retorno.Mensagem = "1 - Ativa";
                retorno.Mensagem2 = "Olá\r\n\r\nEsperamos que você esteja tendo uma ótima experiência com o nosso app. \r\n\r\nPara garantir que tudo continue funcionando bem, se faz necessário entrar em contato com a empresa Syso Tecnologia (69) 3222-0609. \r\n\r\nAguardo seu contato!";
                return retorno;
            }

            retorno.Mensagem = "1 - Ativa";
            retorno.Status = true;
            return retorno;

        }
    }
}
