using CISS411_GroupProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
/*
Course #: CISS 411
Course Name: Software Architecture with ASP.NET with MVC
Group 2: Ashley Steward, Linda Daniel,Allan Lopesandovall,
Brenden Hoffman, Jason Farr, Jerome Whitaker,
Jason Farr and Justin Kim.
Date Completed: 10-2-2025
Story Assigne: Ashley Steward 
Story: User Story 2*/
namespace CISS411_GroupProject.Models.ViewModels
{
    public class OrderFormViewModel
    {
        public Order Order { get; set; } = new Order();

        [Required]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>
        {
            new OrderItem() 
        };
    }
}
