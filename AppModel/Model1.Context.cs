﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppModel
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class NoodleDContext : DbContext
    {
        public NoodleDContext()
            : base("name=NoodleDContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Dish> Dish { get; set; }
        public virtual DbSet<DishGarnish> DishGarnish { get; set; }
        public virtual DbSet<DishIngredient> DishIngredient { get; set; }
        public virtual DbSet<DishMark> DishMark { get; set; }
        public virtual DbSet<DishMarks> DishMarks { get; set; }
        public virtual DbSet<DishUnit> DishUnit { get; set; }
        public virtual DbSet<FieldType> FieldType { get; set; }
        public virtual DbSet<MenuFolder> MenuFolder { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderDish> OrderDish { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<StringValue> StringValue { get; set; }
        public virtual DbSet<Terminal> Terminal { get; set; }
        public virtual DbSet<OrderDishGarnish> OrderDishGarnish { get; set; }
        public virtual DbSet<OrderDishIngredient> OrderDishIngredient { get; set; }
        public virtual DbSet<DishRecommends> DishRecommends { get; set; }
    }
}
