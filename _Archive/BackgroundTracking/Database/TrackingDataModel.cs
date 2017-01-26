namespace BackgroundTracking.Database
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TrackingDataModel : DbContext
    {
        public TrackingDataModel()
            : base("name=TrackingDataContext")
        {
        }
        public TrackingDataModel(string _ConnectionString)
            : base(new System.Data.SQLite.SQLiteConnection() { ConnectionString = _ConnectionString }, true)
        {
        }

        public virtual DbSet<process_activities> process_activities { get; set; }
        public virtual DbSet<processes> processes { get; set; }
        public virtual DbSet<sessions> sessions { get; set; }
        public virtual DbSet<url_activities> url_activities { get; set; }
        public virtual DbSet<urls> urls { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<processes>()
                .HasMany(e => e.process_activities)
                .WithOptional(e => e.processes)
                .HasForeignKey(e => e.process_id);

            modelBuilder.Entity<sessions>()
                .HasMany(e => e.processes)
                .WithOptional(e => e.sessions)
                .HasForeignKey(e => e.session_id);

            modelBuilder.Entity<sessions>()
                .HasMany(e => e.urls)
                .WithOptional(e => e.sessions)
                .HasForeignKey(e => e.session_id);

            modelBuilder.Entity<urls>()
                .HasMany(e => e.url_activities)
                .WithOptional(e => e.urls)
                .HasForeignKey(e => e.url_id);
        }
    }
}
