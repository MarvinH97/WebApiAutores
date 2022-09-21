using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using WebApiAutores.DTOs;

namespace WebApiAutores.Servicios
{
    public class HashService
    {
        public ResultadoHash Hash(string textoPlano) 
        {
            /** En este caso generamos una "sal" aleatoria, esta nos ayudara a hashear nuestro texto plano.
             * Si queremos generar el mimo hash a un mismo texto plano, deberemos usar la misma sal,
             caso contrario el has que se generaría fuera diferente.
             Cabe decir que los textos planos hasheados no se pueden descifrar, solo se comparan con otro hash delmismo texto plano*/
            var sal = new byte[16];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(sal);
            }
            return Hash(textoPlano, sal);
        }

        public ResultadoHash Hash(string textoPlano, byte[] sal) 
        {
            var llaveDerivada = KeyDerivation.Pbkdf2(password: textoPlano,
                salt: sal, prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 32
                );
            var hash = Convert.ToBase64String(llaveDerivada);
            return new ResultadoHash { 
                Hash = hash,
                Sal = sal
            };
        }
    }
}
