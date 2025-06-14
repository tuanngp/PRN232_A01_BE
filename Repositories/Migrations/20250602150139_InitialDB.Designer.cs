﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Repositories;

#nullable disable

namespace Repositories.Migrations
{
    [DbContext(typeof(FUNewsDbContext))]
    [Migration("20250602150139_InitialDB")]
    partial class InitialDB
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BusinessObject.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("CategoryDescription")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DeletedById")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ParentCategoryId")
                        .HasColumnType("int");

                    b.HasKey("CategoryId");

                    b.HasIndex("DeletedById");

                    b.HasIndex("ParentCategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("BusinessObject.NewsArticle", b =>
                {
                    b.Property<int>("NewsArticleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NewsArticleId"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DeletedById")
                        .HasColumnType("int");

                    b.Property<string>("Headline")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("NewsContent")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewsSource")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("NewsStatus")
                        .HasColumnType("int");

                    b.Property<string>("NewsTitle")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int?>("UpdatedById")
                        .HasColumnType("int");

                    b.HasKey("NewsArticleId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DeletedById");

                    b.HasIndex("UpdatedById");

                    b.ToTable("NewsArticles");
                });

            modelBuilder.Entity("BusinessObject.NewsArticleTag", b =>
                {
                    b.Property<int>("NewsArticleId")
                        .HasColumnType("int");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.HasKey("NewsArticleId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("NewsArticleTags");
                });

            modelBuilder.Entity("BusinessObject.SystemAccount", b =>
                {
                    b.Property<int>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AccountId"));

                    b.Property<string>("AccountEmail")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("AccountName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("AccountPassword")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("AccountRole")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("AccountId");

                    b.ToTable("SystemAccounts");
                });

            modelBuilder.Entity("BusinessObject.Tag", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TagId"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DeletedById")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Note")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("TagName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("TagId");

                    b.HasIndex("DeletedById");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("BusinessObject.Category", b =>
                {
                    b.HasOne("BusinessObject.SystemAccount", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.HasOne("BusinessObject.Category", "ParentCategory")
                        .WithMany("SubCategories")
                        .HasForeignKey("ParentCategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("DeletedBy");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("BusinessObject.NewsArticle", b =>
                {
                    b.HasOne("BusinessObject.Category", "Category")
                        .WithMany("NewsArticles")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.SystemAccount", "CreatedBy")
                        .WithMany("CreatedNewsArticles")
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BusinessObject.SystemAccount", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.HasOne("BusinessObject.SystemAccount", "UpdatedBy")
                        .WithMany("UpdatedNewsArticles")
                        .HasForeignKey("UpdatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Category");

                    b.Navigation("CreatedBy");

                    b.Navigation("DeletedBy");

                    b.Navigation("UpdatedBy");
                });

            modelBuilder.Entity("BusinessObject.NewsArticleTag", b =>
                {
                    b.HasOne("BusinessObject.NewsArticle", "NewsArticle")
                        .WithMany("NewsArticleTags")
                        .HasForeignKey("NewsArticleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.Tag", "Tag")
                        .WithMany("NewsArticleTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NewsArticle");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("BusinessObject.Tag", b =>
                {
                    b.HasOne("BusinessObject.SystemAccount", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.Navigation("DeletedBy");
                });

            modelBuilder.Entity("BusinessObject.Category", b =>
                {
                    b.Navigation("NewsArticles");

                    b.Navigation("SubCategories");
                });

            modelBuilder.Entity("BusinessObject.NewsArticle", b =>
                {
                    b.Navigation("NewsArticleTags");
                });

            modelBuilder.Entity("BusinessObject.SystemAccount", b =>
                {
                    b.Navigation("CreatedNewsArticles");

                    b.Navigation("UpdatedNewsArticles");
                });

            modelBuilder.Entity("BusinessObject.Tag", b =>
                {
                    b.Navigation("NewsArticleTags");
                });
#pragma warning restore 612, 618
        }
    }
}
