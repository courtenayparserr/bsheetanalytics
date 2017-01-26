namespace BackgroundTracking.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class url_activities
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string started { get; set; }

        [StringLength(2147483647)]
        public string ended { get; set; }

        public int? url_id { get; set; }

        public virtual urls urls { get; set; }
    }
}
