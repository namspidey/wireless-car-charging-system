using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTO
{
    public class MyCarsDTO
    {
        public int CarId { get; set; }
        public string? CarName { get; set; }

        public string? LicensePlate { get; set; }

        public bool? IsDeleted { get; set; }
        public string? Type { get; set; }

        public string? Color { get; set; }
        public string? Brand { get; set; }
        public string? Img { get; set; }
    }
}
