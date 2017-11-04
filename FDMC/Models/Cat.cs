namespace FDMC.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Cat
    {
        public const int Max = 50;

  
        public string Id { get; set; }

        [Required]
        [MaxLength(Max)]
        public string Name { get; set; }

        [Required]
        [MaxLength(Max)]
        public int Age { get; set; }

        [Required]
        public Breed Breed { get; set; }

        [Required]
        public string ImageUrl { get; set; }
    }
}
