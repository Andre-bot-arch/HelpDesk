using AppSysoHelp.Models;

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
    }
}
