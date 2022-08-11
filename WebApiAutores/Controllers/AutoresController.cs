using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/Autores")]
    /** Al dejar Authorize en el controlador, estamos declarando que para que el cliente pueda acceder a
     * cada uno de los endpoint, debe de haber un usuario autenticado. Este restrincción aplicaría para todos los
     * endpoint, excepto los que tengan el atributo [AllowAnonymous]*/
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public AutoresController(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        /** Puedo definir más de una ruta para un endpoint o action */

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name = "obtenerAutor")]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id) 
        {
            var autor = await context.Autores
                .Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                return NotFound();
            }

            return mapper.Map<AutorDTOConLibros>(autor);
        }

        [HttpGet("{nombre}")] /** Aca no se le puede poner una restricción a la variable o parámtro nombre ya que no existe uno que sea "string" solo existe "int" */
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
            
            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(s => s.Nombre == autorCreacionDTO.Nombre);
            if (existeAutorConElMismoNombre)
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("obtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var existe = await context.Autores.AnyAsync(s => s.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            /** Se puede devolver 204 cuando no hay registro que devolver */
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(s => s.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Autor { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }       
    }
}
