using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Utilidades
{
    public class HATEOASFiltroAttribute : ResultFilterAttribute
    {
        protected bool DebeIncluirHATEOS(ResultExecutingContext context) 
        {
            var resultado = context.Result as ObjectResult;
            if (!EsRespuestaExitosa(resultado))
                return false;
            var cabecera = context.HttpContext.Request.Headers["incluirHATEOAS"];
            if (cabecera.Count == 0)
                return false;
            var valor = cabecera[0];
            if (!valor.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                return false;
            return true;
        }

        private bool EsRespuestaExitosa(ObjectResult result) 
        {
            if (result == null || result.Value == null)
                return false;
            if (result.StatusCode.HasValue && !result.StatusCode.Value.ToString().StartsWith("2"))
                return false;
            return true;
        }
    }
}
