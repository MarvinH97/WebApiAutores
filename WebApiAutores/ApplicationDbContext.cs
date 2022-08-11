
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        /** Función para especificar o configurar propiedades de las entidades */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            /** Configuramos que la Primary Key de AutoresLibros estará compuesta por AutorId y LibroId */
            modelBuilder.Entity<AutorLibro>()
                .HasKey(al => new { al.AutorId, al.LibroId });
        }
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        /** Tabla de doble relación para Autor y Libro ya que un Autor puede escribir muchos libros y un Libro puede estar escrito por muchos autores. Relación M:M(Muchos a Muchos)  */
        public DbSet<AutorLibro> AutoresLibros { get; set; }
    }
}
