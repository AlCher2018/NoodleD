//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class StringValue
    {
        public int Id { get; set; }
        public System.Guid RowGUID { get; set; }
        public int FieldTypeId { get; set; }
        public string Lang { get; set; }
        public string Value { get; set; }
    
        public virtual FieldType FieldType { get; set; }
    }
}