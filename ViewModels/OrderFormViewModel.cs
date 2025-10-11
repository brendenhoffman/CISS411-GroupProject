/*
Course #: CISS 411
Course Name: Software Architecture with ASP.NET with MVC

Group 3: Ashley Steward, Linda Daniel,Allan Lopesandovall,
Brenden Hoffman, Jason Farr, Jerome Whitaker,
Jason Farr and Justin Kim.

Date Completed: 10-2-2025
Story Assigne: Ashley Steward 
Story: User Story 2
*/

using CISS411_GroupProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CISS411_GroupProject.Models.ViewModels
{
    public class OrderFormViewModel
    {
        [ValidateNever]
        public Order? OrderInput { get; set; } = new();

        [Required]
        public List<OrderItem> Items { get; set; } = new();

		// For dropdown
	public List<SelectListItem> AvailableItems { get; set; } = new();
	}
}