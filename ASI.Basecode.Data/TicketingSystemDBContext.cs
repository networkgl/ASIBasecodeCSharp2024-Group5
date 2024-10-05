using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    public partial class TicketingSystemDBContext : DbContext
    {
        public TicketingSystemDBContext()
        {
        }

        public TicketingSystemDBContext(DbContextOptions<TicketingSystemDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<AssignedTicket> AssignedTickets { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Priority> Priorities { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserTicket> UserTickets { get; set; }
        public virtual DbSet<VwAdminCount> VwAdminCounts { get; set; }
        public virtual DbSet<VwAdminUsersView> VwAdminUsersViews { get; set; }
        public virtual DbSet<VwAgentCount> VwAgentCounts { get; set; }
        public virtual DbSet<VwTicketAssignment> VwTicketAssignments { get; set; }
        public virtual DbSet<VwTicketCountForAgent> VwTicketCountForAgents { get; set; }
        public virtual DbSet<VwTotalTicketsResolved> VwTotalTicketsResolveds { get; set; }
        public virtual DbSet<VwUserCount> VwUserCounts { get; set; }
        public virtual DbSet<VwUserRoleView> VwUserRoleViews { get; set; }
        public virtual DbSet<VwUserTicketView> VwUserTicketViews { get; set; }
        public virtual DbSet<VwUserTicketViewForAdminsAndAgent> VwUserTicketViewForAdminsAndAgents { get; set; }
        public virtual DbSet<VwUsersAndAgentsView> VwUsersAndAgentsViews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=.\\sqlexpress;Initial Catalog=TicketingSystemDB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("Article");

                entity.Property(e => e.Content).IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Articles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Article__UserId__73BA3083");
            });

            modelBuilder.Entity<AssignedTicket>(entity =>
            {
                entity.ToTable("AssignedTicket");

                entity.Property(e => e.DateAssigned)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LastModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Agent)
                    .WithMany(p => p.AssignedTicketAgents)
                    .HasForeignKey(d => d.AgentId)
                    .HasConstraintName("FK__Resolve__AgentId__114A936A");

                entity.HasOne(d => d.Assigner)
                    .WithMany(p => p.AssignedTicketAssigners)
                    .HasForeignKey(d => d.AssignerId)
                    .HasConstraintName("FK__Resolve__AdminId__10566F31");

                entity.HasOne(d => d.UserTicket)
                    .WithMany(p => p.AssignedTickets)
                    .HasForeignKey(d => d.UserTicketId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Resolve__UserTic__0F624AF8");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback");

                entity.Property(e => e.FeedbackText).IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Feedback__UserId__76969D2E");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Content).IsUnicode(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LastModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.FromUserId)
                    .HasConstraintName("FK__Notificat__Creat__1BC821DD");

                entity.HasOne(d => d.UserTicket)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserTicketId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Notificat__UserT__1CBC4616");
            });

            modelBuilder.Entity<Priority>(entity =>
            {
                entity.ToTable("Priority");

                entity.HasIndex(e => e.PriorityName, "UQ__Priority__346EBED66C22B5A1")
                    .IsUnique();

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B61600F5C4C2D")
                    .IsUnique();

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Status");

                entity.HasIndex(e => e.StatusName, "UQ__Status__05E7698AA633D2C9")
                    .IsUnique();

                entity.Property(e => e.StatusName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket");

                entity.Property(e => e.AttachmentPath).IsUnicode(false);

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IssueDescription).IsUnicode(false);

                entity.Property(e => e.LastModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Ticket__Category__6EF57B66");

                entity.HasOne(d => d.Priority)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.PriorityId)
                    .HasConstraintName("FK__Ticket__Priority__6FE99F9F");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Ticket__StatusId__70DDC3D8");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Name, "UQ__User__737584F6BEDC2FA5")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "UQ__User__A9D10534540E4C7A")
                    .IsUnique();

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRole");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__UserRole__RoleId__693CA210");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__UserRole__UserId__68487DD7");
            });

            modelBuilder.Entity<UserTicket>(entity =>
            {
                entity.ToTable("UserTicket");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.UserTickets)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__UserTicke__Ticke__7A672E12");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserTickets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__UserTicke__UserI__797309D9");
            });

            modelBuilder.Entity<VwAdminCount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_AdminCount");

                entity.Property(e => e.TotalAdminCount).HasColumnName("Total Admin Count");
            });

            modelBuilder.Entity<VwAdminUsersView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_AdminUsersView");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwAgentCount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_AgentCount");

                entity.Property(e => e.TotalAgentCount).HasColumnName("Total Agent Count");
            });

            modelBuilder.Entity<VwTicketAssignment>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TicketAssignment");

                entity.Property(e => e.AgentEmail)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AgentName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AgentPassword)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AssignerEmail)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AssignerName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AssignerPassword)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AttachmentPath).IsUnicode(false);

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.DateAssigned).HasColumnType("datetime");

                entity.Property(e => e.IssueDescription).IsUnicode(false);

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Reporter)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ReporterEmail)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ReporterPassword)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ResolveLastModified).HasColumnType("datetime");

                entity.Property(e => e.StatusName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TicketLastModified).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwTicketCountForAgent>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TicketCountForAgent");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.NoOfTickets).HasColumnName("No. of tickets");
            });

            modelBuilder.Entity<VwTotalTicketsResolved>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TotalTicketsResolved");

                entity.Property(e => e.TotalTicketsResolved).HasColumnName("Total tickets resolved");
            });

            modelBuilder.Entity<VwUserCount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_UserCount");

                entity.Property(e => e.TotalUserCount).HasColumnName("Total user count");
            });

            modelBuilder.Entity<VwUserRoleView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_UserRoleView");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwUserTicketView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_UserTicketView");

                entity.Property(e => e.AttachmentPath).IsUnicode(false);

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.IssueDescription).IsUnicode(false);

                entity.Property(e => e.LastModified).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StatusName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwUserTicketViewForAdminsAndAgent>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_UserTicketViewForAdminsAndAgents");

                entity.Property(e => e.AttachmentPath).IsUnicode(false);

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.IssueDescription).IsUnicode(false);

                entity.Property(e => e.LastModified).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StatusName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwUsersAndAgentsView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_UsersAndAgentsView");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
