using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.ViewModels
{
    public class NewIncidentViewModel
    {
        // [Required]
        public string Subject { get; set; }
        public IncidentType IncidentType { get; set; }
        public Priority Priority { get; set; }
        public int Deadline { get; set; } // 7|14|28
        public FranchiseLocation LocationBranchName { get; set; } 
        public string Description { get; set; } 
       
                                                         
        public ReporterSnapshot Reporter { get; set; }	
    }
}


    

    // Dropdowns
    /*
    public IEnumerable<SelectListItem> IncidentTypeOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> PriorityOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> DeadlineOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> LocationOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    */
