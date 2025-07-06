using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UMT88.Models;

namespace UMT88.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bet> Bets { get; set; }

    public virtual DbSet<Bet_Item> Bet_Items { get; set; }

    public virtual DbSet<Competition> Competitions { get; set; }

    public virtual DbSet<Deposit_Request> Deposit_Requests { get; set; }

    public virtual DbSet<League_Standing> League_Standings { get; set; }

    public virtual DbSet<Market> Markets { get; set; }

    public virtual DbSet<Market_Type> Market_Types { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<MatchEvent> MatchEvents { get; set; }

    public virtual DbSet<Match_Result> Match_Results { get; set; }

    public virtual DbSet<Odd> Odds { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Season> Seasons { get; set; }

    public virtual DbSet<Selection> Selections { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Team_Season> Team_Seasons { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<User_Promotion> User_Promotions { get; set; }

    public virtual DbSet<Withdraw_Request> Withdraw_Requests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=GROOTTHEDEVELOP\\MSSQLSERVER01;Database=UMT88;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bet>(entity =>
        {
            entity.HasKey(e => e.bet_id).HasName("PK__Bet__551113D84ACA4B57");

            entity.ToTable("Bet");

            entity.Property(e => e.placed_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.potential_payout).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.stake_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.user).WithMany(p => p.Bets)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bet_User");
        });

        modelBuilder.Entity<Bet_Item>(entity =>
        {
            entity.HasKey(e => e.bet_item_id).HasName("PK__Bet_Item__71A020A10F114B2D");

            entity.ToTable("Bet_Item");

            entity.Property(e => e.odds_at_placement).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.result)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.bet).WithMany(p => p.Bet_Items)
                .HasForeignKey(d => d.bet_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BetItem_Bet");

            entity.HasOne(d => d.selection).WithMany(p => p.Bet_Items)
                .HasForeignKey(d => d.selection_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BetItem_Selection");
        });

        modelBuilder.Entity<Competition>(entity =>
        {
            entity.HasKey(e => e.competition_id).HasName("PK__Competit__BB383B58DC10674C");

            entity.ToTable("Competition");

            entity.Property(e => e.country)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Deposit_Request>(entity =>
        {
            entity.HasKey(e => e.deposit_id).HasName("PK__Deposit___4450A62A38A071C1");

            entity.ToTable("Deposit_Request");

            entity.Property(e => e.amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.payment_method)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.requested_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.user).WithMany(p => p.Deposit_Requests)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposit_User");
        });

        modelBuilder.Entity<League_Standing>(entity =>
        {
            entity.HasKey(e => e.league_standing_id).HasName("PK__League_S__9BB04619DDC0D8A3");

            entity.ToTable("League_Standing");

            entity.HasIndex(e => new { e.season_id, e.team_id }, "UQ_Standing").IsUnique();

            entity.Property(e => e.last_updated).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.season).WithMany(p => p.League_Standings)
                .HasForeignKey(d => d.season_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Standing_Season");

            entity.HasOne(d => d.team).WithMany(p => p.League_Standings)
                .HasForeignKey(d => d.team_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Standing_Team");
        });

        modelBuilder.Entity<Market>(entity =>
        {
            entity.HasKey(e => e.market_id).HasName("PK__Market__C108146D08C997FB");

            entity.ToTable("Market");

            entity.HasIndex(e => new { e.match_id, e.market_type_id }, "UQ_Market").IsUnique();

            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.market_type).WithMany(p => p.Markets)
                .HasForeignKey(d => d.market_type_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Market_MarketType");

            entity.HasOne(d => d.match).WithMany(p => p.Markets)
                .HasForeignKey(d => d.match_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Market_Match");
        });

        modelBuilder.Entity<Market_Type>(entity =>
        {
            entity.HasKey(e => e.market_type_id).HasName("PK__Market_T__00CBC51838D13187");

            entity.ToTable("Market_Type");

            entity.HasIndex(e => e.code, "UQ__Market_T__357D4CF995FE3FE9").IsUnique();

            entity.Property(e => e.code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.match_id).HasName("PK__Match__9D7FCBA3DD82A4AB");

            entity.ToTable("Match");

            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.away_team).WithMany(p => p.Matchaway_teams)
                .HasForeignKey(d => d.away_team_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_AwayTeam");

            entity.HasOne(d => d.home_team).WithMany(p => p.Matchhome_teams)
                .HasForeignKey(d => d.home_team_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_HomeTeam");

            entity.HasOne(d => d.season).WithMany(p => p.Matches)
                .HasForeignKey(d => d.season_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_Season");
        });

        modelBuilder.Entity<MatchEvent>(entity =>
        {
            entity.HasKey(e => e.event_id).HasName("PK__MatchEve__2370F7277AF1FE12");

            entity.ToTable("MatchEvent");

            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.description).HasMaxLength(255);
            entity.Property(e => e.event_type)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.match).WithMany(p => p.MatchEvents)
                .HasForeignKey(d => d.match_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MatchEvent_Match");

            entity.HasOne(d => d.team).WithMany(p => p.MatchEvents)
                .HasForeignKey(d => d.team_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MatchEvent_Team");
        });

        modelBuilder.Entity<Match_Result>(entity =>
        {
            entity.HasKey(e => e.match_result_id).HasName("PK__Match_Re__92A127F1F125A9CE");

            entity.ToTable("Match_Result");

            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.full_time_result)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.match).WithMany(p => p.Match_Results)
                .HasForeignKey(d => d.match_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MatchResult_Match");
        });

        modelBuilder.Entity<Odd>(entity =>
        {
            entity.HasKey(e => e.odds_id).HasName("PK__Odds__88E3B83645E0143B");

            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.odds_value).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.selection).WithMany(p => p.Odds)
                .HasForeignKey(d => d.selection_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Odds_Selection");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.promotion_id).HasName("PK__Promotio__2CB9556BBC89DF60");

            entity.ToTable("Promotion");

            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.type)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.value).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.role_id).HasName("PK__Role__760965CC6E4358F3");

            entity.ToTable("Role");

            entity.HasIndex(e => e.role_name, "UQ__Role__783254B1F71A87F2").IsUnique();

            entity.Property(e => e.role_name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasKey(e => e.season_id).HasName("PK__Season__0A99E3315F91C957");

            entity.ToTable("Season");

            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.competition).WithMany(p => p.Seasons)
                .HasForeignKey(d => d.competition_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Season_Competition");
        });

        modelBuilder.Entity<Selection>(entity =>
        {
            entity.HasKey(e => e.selection_id).HasName("PK__Selectio__010BE5391C8B524E");

            entity.ToTable("Selection");

            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.market).WithMany(p => p.Selections)
                .HasForeignKey(d => d.market_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Selection_Market");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.team_id).HasName("PK__Team__F82DEDBC1E1C4515");

            entity.ToTable("Team");

            entity.Property(e => e.country)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.image_url)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.short_name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Team_Season>(entity =>
        {
            entity.HasKey(e => e.team_season_id).HasName("PK__Team_Sea__F499146C5DE72A4C");

            entity.ToTable("Team_Season");

            entity.HasIndex(e => new { e.team_id, e.season_id }, "UQ_TeamSeason").IsUnique();

            entity.HasOne(d => d.season).WithMany(p => p.Team_Seasons)
                .HasForeignKey(d => d.season_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeamSeason_Season");

            entity.HasOne(d => d.team).WithMany(p => p.Team_Seasons)
                .HasForeignKey(d => d.team_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeamSeason_Team");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.transaction_id).HasName("PK__Transact__85C600AFC61C4420");

            entity.ToTable("Transaction");

            entity.Property(e => e.amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.balance_after).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.transaction_type)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.user).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaction_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PK__User__B9BE370FD5D807DF");

            entity.ToTable("User");

            entity.HasIndex(e => e.email, "UQ__User__AB6E616440F246C0").IsUnique();

            entity.Property(e => e.balance).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.password_hash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.role_id).HasDefaultValue(2);
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<User_Promotion>(entity =>
        {
            entity.HasKey(e => e.user_promotion_id).HasName("PK__User_Pro__43C43122B491582D");

            entity.ToTable("User_Promotion");

            entity.HasIndex(e => new { e.user_id, e.promotion_id }, "UQ_UserPromotion").IsUnique();

            entity.Property(e => e.assigned_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.promotion).WithMany(p => p.User_Promotions)
                .HasForeignKey(d => d.promotion_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserPromotion_Promotion");

            entity.HasOne(d => d.user).WithMany(p => p.User_Promotions)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserPromotion_User");
        });

        modelBuilder.Entity<Withdraw_Request>(entity =>
        {
            entity.HasKey(e => e.withdraw_id).HasName("PK__Withdraw__2F1C7929401D566F");

            entity.ToTable("Withdraw_Request");

            entity.Property(e => e.amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.bank_account)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.requested_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.user).WithMany(p => p.Withdraw_Requests)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Withdraw_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
