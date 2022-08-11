using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        /** Se puede asignar un mobre a una ruta en este caso "ObtenerLibro" */
        [HttpGet("{id:int}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await context.Libros
                .Include(libroDB => libroDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Autor)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (libro == null)
                return NotFound();
            libro.AutoresLibros = libro.AutoresLibros.OrderBy(s => s.Orden).ToList();
            return mapper.Map<LibroDTOConAutores>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }

            var autoresIds = await context.Autores
                .Where(autorDB => libroCreacionDTO.AutoresIds.Contains(autorDB.Id)).Select(s => s.Id).ToListAsync();

            if (autoresIds.Count != libroCreacionDTO.AutoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }
            
            var libro = mapper.Map<Libro>(libroCreacionDTO);

            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();
            var libroDTO = mapper.Map<LibroDTO>(libro);
            
            /** Crea una ruta que será devuelta en location del header de la respuesta, además devuelve el recurso creado en este caso el "libroDTO" */
            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO) 
        {
            var libroDB = await context.Libros
                .Include(s => s.AutoresLibros)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (libroDB == null)
            {
                return NotFound();
            }
            /** Lo que aca se hace es pasar los valores de libroCreacionDTO a libroDB que es una instancia creada por EFCore */
            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            AsignarOrdenAutores(libroDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro) 
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }

        /** Con HttpPatch podemos modificar parcialmente nuestros recursos sin estar enviando toda la data y solo mandar la data o propiedades que solo deseamos reemplazar */
        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument) 
        {
            if (patchDocument == null)
            {
                /** Si entra significa que hay un error con el formato que ha enviado el cliente */
                return BadRequest();
            }
            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);
            if (libroDB == null)
            {
                return NotFound();
            }
            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB);
            patchDocument.ApplyTo(libroDTO, ModelState);
            var esValido = TryValidateModel(libroDTO);
            if (!esValido)
            {
                return BadRequest(ModelState);
            }
            mapper.Map(libroDTO, libroDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(s => s.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Libro { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
