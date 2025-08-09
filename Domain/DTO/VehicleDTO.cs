using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Domain.DTO
{
    public record VehicleDTO
    {
        public string Name { get; set; } = default!;
        public string Brand { get; set; } = default!;
        public int Year { get; set; } = default!;
    }
}
