using System;
using System.ComponentModel.DataAnnotations;

namespace cima.Domain.Shared.Dtos
{
    public class CreatePropertyDtoValidation
    {
        [Required(ErrorMessage = "El titulo es requerido")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "El titulo debe tener entre 5 y 200 caracteres")]
        public string Title { get; set; }

        [StringLength(5000, ErrorMessage = "La descripcion no puede exceder 5000 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "La ubicacion es requerida")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "La ubicacion debe tener entre 3 y 500 caracteres")]
        public string Location { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El area es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El area debe ser mayor a 0")]
        public decimal Area { get; set; }

        [Range(0, 20, ErrorMessage = "Las recamaras deben estar entre 0 y 20")]
        public int Bedrooms { get; set; }

        [Range(0, 20, ErrorMessage = "Los banos deben estar entre 0 y 20")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "El arquitecto es requerido")]
        public Guid ArchitectId { get; set; }
    }

    public class CreateContactRequestDtoValidation
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es valida")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "El telefono no es valido")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 5000 caracteres")]
        public string Message { get; set; }

        [Required(ErrorMessage = "La propiedad es requerida")]
        public Guid ListingId { get; set; }
    }

    public class CreateArchitectDtoValidation
    {
        [StringLength(2000, ErrorMessage = "La biografia no puede exceder 2000 caracteres")]
        public string Bio { get; set; }

        [Url(ErrorMessage = "La URL del portafolio no es valida")]
        [StringLength(500, ErrorMessage = "La URL no puede exceder 500 caracteres")]
        public string PortfolioUrl { get; set; }
    }
}
