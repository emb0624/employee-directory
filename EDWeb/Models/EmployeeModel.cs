﻿namespace EDWeb.Models
{
    public class EmployeeModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string SecondLastName { get; set; }
        public string JobTitle { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }

        public string FullName 
        {
            get 
            {
                return FirstName + (!string.IsNullOrEmpty(MiddleInitial) ? " " + MiddleInitial : "") + " " + LastName + (!string.IsNullOrEmpty(SecondLastName) ? " " + SecondLastName : ""); 
            }
        }
    }
}