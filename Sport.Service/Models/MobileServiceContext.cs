using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using System.Configuration;
using System.Collections.Generic;

namespace Sport.Service.Models
{
    public class MobileServiceContext : DbContext
    {
        private const string connectionStringName = "Name=MS_TableConnectionString";

        public MobileServiceContext() : base(connectionStringName)
		{
        }

        public DbSet<Athlete> Athletes
        {
            get;
            set;
        }

        public DbSet<Membership> Memberships
        {
            get;
            set;
        }

        public DbSet<League> Leagues
        {
            get;
            set;
        }

        public DbSet<Challenge> Challenges
        {
            get;
            set;
        }

        public DbSet<GameResult> GameResults
        {
            get;
            set;
        }

        static object _syncObject;
        public static object SyncObject
        {
            get
            {
                return _syncObject ?? (_syncObject = new object());
            }
        }

        static MobileServiceContext _instance;
        public static MobileServiceContext Instance
        {
            get
            {
                return _instance ?? (_instance = new MobileServiceContext());
            }
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;

            var schema = ConfigurationManager.AppSettings.Get("MS_MobileServiceName");
            if (!string.IsNullOrEmpty(schema))
            {
                modelBuilder.HasDefaultSchema(schema);
            }

            modelBuilder.Entity<Athlete>().ToTable("Athlete");
            modelBuilder.Entity<League>().ToTable("League");
            modelBuilder.Entity<Membership>().ToTable("Membership");
            modelBuilder.Entity<Challenge>().ToTable("Challenge");
            modelBuilder.Entity<GameResult>().ToTable("GameResult");

            modelBuilder.Entity<Challenge>().HasRequired(a => a.ChallengeeAthlete)
                .WithMany().HasForeignKey(a => a.ChallengeeAthleteId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Challenge>().HasRequired(a => a.ChallengerAthlete)
                .WithMany().HasForeignKey(a => a.ChallengerAthleteId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Challenge>().HasRequired(a => a.League)
                .WithMany().HasForeignKey(a => a.LeagueId);

            modelBuilder.Entity<GameResult>().HasRequired(a => a.Challenge)
                .WithMany().HasForeignKey(a => a.ChallengeId);

            modelBuilder.Entity<GameResult>().HasRequired(g => g.Challenge)
                .WithMany(c => c.MatchResult)
                .HasForeignKey(g => g.ChallengeId);

            modelBuilder.Entity<League>().HasOptional(a => a.CreatedByAthlete)
                .WithMany().HasForeignKey(a => a.CreatedByAthleteId);

            modelBuilder.Entity<Membership>().HasRequired(m => m.League)
                .WithMany(l => l.Memberships)
                .HasForeignKey(m => m.LeagueId);

            modelBuilder.Entity<Challenge>().HasRequired(c => c.League)
                .WithMany(l => l.Challenges)
                .HasForeignKey(m => m.LeagueId);

            modelBuilder.Entity<Membership>().HasRequired(m => m.Athlete)
                .WithMany(a => a.Memberships)
                .HasForeignKey(m => m.AthleteId);

            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }
    }
}
