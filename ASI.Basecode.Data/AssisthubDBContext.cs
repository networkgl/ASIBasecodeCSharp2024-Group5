﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    public partial class AssisthubDBContext : DbContext
    {
        public AssisthubDBContext()
        {
        }

        public AssisthubDBContext(DbContextOptions<AssisthubDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<AssignedTicket> AssignedTickets { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Expertise> Expertises { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Priority> Priorities { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAgent> UserAgents { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserTicket> UserTickets { get; set; }
        public virtual DbSet<VwAdminCount> VwAdminCounts { get; set; }
        public virtual DbSet<VwAdminUsersView> VwAdminUsersViews { get; set; }
        public virtual DbSet<VwAgentCount> VwAgentCounts { get; set; }
        public virtual DbSet<VwAgentFeedbackRatingView> VwAgentFeedbackRatingViews { get; set; }
        public virtual DbSet<VwAssignedTicketView> VwAssignedTicketViews { get; set; }
        public virtual DbSet<VwCustomerSatisfactionRating> VwCustomerSatisfactionRatings { get; set; }
        public virtual DbSet<VwFeedbackView> VwFeedbackViews { get; set; }
        public virtual DbSet<VwNotificationView> VwNotificationViews { get; set; }
        public virtual DbSet<VwResolvedTicketByAgent> VwResolvedTicketByAgents { get; set; }
        public virtual DbSet<VwTicketAssignedToMeAgent> VwTicketAssignedToMeAgents { get; set; }
        public virtual DbSet<VwTicketCountForAgent> VwTicketCountForAgents { get; set; }
        public virtual DbSet<VwTicketDetailsView> VwTicketDetailsViews { get; set; }
        public virtual DbSet<VwTicketsByCategory> VwTicketsByCategories { get; set; }
        public virtual DbSet<VwTicketsByPriority> VwTicketsByPriorities { get; set; }
        public virtual DbSet<VwTicketsByStatus> VwTicketsByStatuses { get; set; }
        public virtual DbSet<VwTotalTicketSummaryWithCategory> VwTotalTicketSummaryWithCategories { get; set; }
        public virtual DbSet<VwTotalTicketSummaryWithPriority> VwTotalTicketSummaryWithPriorities { get; set; }
        public virtual DbSet<VwTotalTicketSummaryWithStatus> VwTotalTicketSummaryWithStatuses { get; set; }
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


                optionsBuilder.UseSqlServer("workstation id=AssisthubDB.mssql.somee.com;packet size=4096;user id=XAssistHubX_SQLLogin_1;pwd=tpu83eivqf;data source=AssisthubDB.mssql.somee.com;persist security info=False;initial catalog=AssisthubDB;TrustServerCertificate=True");

                optionsBuilder.UseSqlServer("Data Source=.\\sqlexpress;Initial Catalog=AssisthubDB;Integrated Security=True;Trust Server Certificate=True");

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("Article");

                entity.Property(e => e.Content).IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Articles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Article__UserId__4E88ABD4");
            });

            modelBuilder.Entity<AssignedTicket>(entity =>
            {
                entity.ToTable("AssignedTicket");

                entity.Property(e => e.DateAssigned)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateResolved).HasColumnType("datetime");

                entity.Property(e => e.LastModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Agent)
                    .WithMany(p => p.AssignedTicketAgents)
                    .HasForeignKey(d => d.AgentId)
                    .HasConstraintName("FK__AssignedT__Agent__5070F446");

                entity.HasOne(d => d.Assigner)
                    .WithMany(p => p.AssignedTicketAssigners)
                    .HasForeignKey(d => d.AssignerId)
                    .HasConstraintName("FK__AssignedT__Assig__4F7CD00D");

                entity.HasOne(d => d.UserTicket)
                    .WithMany(p => p.AssignedTickets)
                    .HasForeignKey(d => d.UserTicketId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__AssignedT__UserT__5165187F");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Expertise>(entity =>
            {
                entity.ToTable("Expertise");

                entity.Property(e => e.ExpertiseName)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.FeedbackRating).HasColumnType("decimal(3, 2)");

                entity.Property(e => e.FeedbackText).IsUnicode(false);

                entity.Property(e => e.TicketCategory).IsUnicode(false);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Content).IsUnicode(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsRead).HasColumnName("isRead");

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

                entity.HasIndex(e => e.PriorityName, "UQ__Priority__346EBED69A6FB293")
                    .IsUnique();

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B61600B938209")
                    .IsUnique();

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Status");

                entity.HasIndex(e => e.StatusName, "UQ__Status__05E7698A818E6FCB")
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

                entity.HasIndex(e => e.Name, "UQ__User__737584F638687D46")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "UQ__User__A9D10534CF54E8FD")
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

                entity.Property(e => e.ProfilePicturePath).IsUnicode(false);
            });

            modelBuilder.Entity<UserAgent>(entity =>
            {
                entity.ToTable("UserAgent");

                entity.Property(e => e.Expertise)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Agent)
                    .WithMany(p => p.UserAgents)
                    .HasForeignKey(d => d.AgentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__UserAgent__Agent__59904A2C");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRole");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__UserRole__RoleId__5812160E");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__UserRole__UserId__59063A47");
            });

            modelBuilder.Entity<UserTicket>(entity =>
            {
                entity.ToTable("UserTicket");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.UserTickets)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__UserTicke__Ticke__59FA5E80");

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

            modelBuilder.Entity<VwAgentFeedbackRatingView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_AgentFeedbackRatingView");

                entity.Property(e => e.AgentName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AverageRating).HasColumnType("decimal(38, 6)");

                entity.Property(e => e.ProfilePicture).IsUnicode(false);
            });

            modelBuilder.Entity<VwAssignedTicketView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_AssignedTicketView");

                entity.Property(e => e.AssignedTicketLastModified).HasColumnType("datetime");

                entity.Property(e => e.AttachmentPath).IsUnicode(false);

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.DateAssigned).HasColumnType("datetime");

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

            modelBuilder.Entity<VwCustomerSatisfactionRating>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_CustomerSatisfactionRatings");

                entity.Property(e => e.AgentName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AvgFeedbackRating).HasColumnType("decimal(38, 6)");

                entity.Property(e => e.FeedbackAt).HasColumnType("date");
            });

            modelBuilder.Entity<VwFeedbackView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_FeedbackView");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.FeedbackRating).HasColumnType("decimal(3, 2)");

                entity.Property(e => e.FeedbackText).IsUnicode(false);

                entity.Property(e => e.IssueDescription).IsUnicode(false);

                entity.Property(e => e.TicketCategory).IsUnicode(false);
            });

            modelBuilder.Entity<VwNotificationView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_NotificationView");

                entity.Property(e => e.AgentName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("Agent Name");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DateAssigned).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.IssueDescription).IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StatusName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwResolvedTicketByAgent>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_ResolvedTicketByAgent");

                entity.Property(e => e.AgentName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ResolvedAt).HasColumnType("date");
            });

            modelBuilder.Entity<VwTicketAssignedToMeAgent>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TicketAssignedToMeAgent");

                entity.Property(e => e.TicketCount).HasColumnName("Ticket Count");
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

            modelBuilder.Entity<VwTicketDetailsView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TicketDetailsView");

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

            modelBuilder.Entity<VwTicketsByCategory>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TicketsByCategory");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("date")
                    .HasColumnName("Created At");
            });

            modelBuilder.Entity<VwTicketsByPriority>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TicketsByPriority");

                entity.Property(e => e.CreatedAt).HasColumnType("date");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("userId");
            });

            modelBuilder.Entity<VwTicketsByStatus>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TicketsByStatus");

                entity.Property(e => e.CreatedAt).HasColumnType("date");

                entity.Property(e => e.StatusName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwTotalTicketSummaryWithCategory>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TotalTicketSummaryWithCategory");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("date");
            });

            modelBuilder.Entity<VwTotalTicketSummaryWithPriority>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TotalTicketSummaryWithPriority");

                entity.Property(e => e.CreatedAt).HasColumnType("date");

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwTotalTicketSummaryWithStatus>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_TotalTicketSummaryWithStatus");

                entity.Property(e => e.CreatedAt).HasColumnType("date");

                entity.Property(e => e.StatusName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
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

                entity.Property(e => e.ProfilePicturePath).IsUnicode(false);

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

                entity.Property(e => e.PriorityName)
                    .HasMaxLength(100)
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

                entity.Property(e => e.Expertise)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProfilePicturePath).IsUnicode(false);

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
