﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VkApi
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class VKApiEntities : DbContext
    {
        public VKApiEntities()
            : base("name=VKApiEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<GroupActionLimit> GroupActionLimits { get; set; }
        public virtual DbSet<GroupActions> GroupActions { get; set; }
        public virtual DbSet<GroupActionType> GroupActionTypes { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<UserActionLimit> UserActionLimits { get; set; }
        public virtual DbSet<UserAction> UserActions { get; set; }
        public virtual DbSet<UserActionType> UserActionTypes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
    }
}
