namespace WebApiAutores.DTOs
{
    /** Decimos que T va a heredar de la clase Recurso */
    public class ColeccionDeRecursos<T> : Recurso where T : Recurso 
    {
        public List<T> Valores { get; set; }
    }
}
