//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NBDCostTrackingSystem
{
    using System;
    using System.Collections.Generic;
    
    public partial class LABOUR_SUMMARY
    {
        public int ID { get; set; }
        public int projectID { get; set; }
        public int workerTypeID { get; set; }
        public short lsHours { get; set; }
    
        public virtual PROJECT PROJECT { get; set; }
        public virtual WORKER_TYPE WORKER_TYPE { get; set; }
    }
}
