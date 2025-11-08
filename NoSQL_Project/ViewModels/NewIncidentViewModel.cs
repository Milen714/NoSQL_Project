using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.ViewModels
{
    public class NewIncidentViewModel
    {

        public string Subject { get; set; }
        public IncidentType IncidentType { get; set; }
        public Priority Priority { get; set; }
        public int Deadline { get; set; } // 7|14|28
        public string LocationBranchName { get; set; }
        public string Description { get; set; }

        public ReporterSnapshot Reporter { get; set; }

    }
}


