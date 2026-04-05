using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Accounting;

namespace Maba.Infrastructure.Data.Configurations;

public class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
{
    public void Configure(EntityTypeBuilder<AccountType> builder)
    {
        builder.ToTable("AccountTypes");
        builder.ConfigureBaseEntity<AccountType>();

        builder.Property(x => x.Key).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NormalBalance).IsRequired().HasMaxLength(10);
        builder.HasIndex(x => x.Key).IsUnique();
    }
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.ConfigureBaseEntity<Account>();

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("ILS");
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.AccountType)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.AccountTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
{
    public void Configure(EntityTypeBuilder<FiscalYear> builder)
    {
        builder.ToTable("FiscalYears");
        builder.ConfigureBaseEntity<FiscalYear>();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
    }
}

public class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("FiscalPeriods");
        builder.ConfigureBaseEntity<FiscalPeriod>();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);

        builder.HasOne(x => x.FiscalYear)
            .WithMany(x => x.Periods)
            .HasForeignKey(x => x.FiscalYearId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.ConfigureBaseEntity<JournalEntry>();

        builder.Property(x => x.EntryNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(100);
        builder.Property(x => x.SourceDocumentNumber).HasMaxLength(100);
        builder.HasIndex(x => x.EntryNumber).IsUnique();

        builder.HasOne(x => x.FiscalPeriod)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.JournalEntryType)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.ConfigureBaseEntity<JournalEntryLine>();

        builder.HasOne(x => x.JournalEntry)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.JournalEntryLines)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
