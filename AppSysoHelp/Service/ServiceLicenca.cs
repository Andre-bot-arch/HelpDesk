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

            var retorno = new ViewModelApiLicenca
            {
                Autorizado = false,
                Cliente = new Clientes
                {
                    Bairro = contrato.FkCliente.Bairro,
                    Cep = contrato.FkCliente.Cep,
                    Cidade = contrato.FkCliente.Cidade,
                    Documento = contrato.FkCliente.Documento,
                    Email = contrato.FkCliente.Email,
                    Fantasia = contrato.FkCliente.Fantasia,
                    Logradouro = contrato.FkCliente.Logradouro,
                    TelefoneCliente = contrato.FkCliente.TelefoneCliente,
                    Uf = contrato.FkCliente.Uf,
                    WhatsApp = contrato.FkCliente.WhatsApp
                },
                Hash = licenca,
                Mensagem = ""
            };

            //verificar chave
            if (contrato == null)
            {
                retorno.Mensagem = "Chave inválida";
                return retorno;
            }else if(contrato.SituacaoContrato.Trim() == "Inativo")
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
                retorno.Mensagem = "Chave Autorizada e Atualizada";
                retorno.Autorizado = true;
                return retorno;
            }
            else if (GravarDispositivos(contrato.Licencas.FirstOrDefault(a => a.Hash == licenca), apelido, serial))
            {
                retorno.Mensagem = "Chave Autorizada";
                retorno.Autorizado = true;
                return retorno;
            }

            retorno.Mensagem = "Erro na Gravação";
            retorno.Autorizado = false;
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
    }
}
