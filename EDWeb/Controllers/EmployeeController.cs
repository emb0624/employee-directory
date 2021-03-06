﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using EDWeb.ModelFactory;
using EDWeb.Models;
using EDWeb.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EDWeb.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/employees")]
    public class EmployeeController : ApiController
    {
        private readonly EmployeeRepository _employeeRepo;
        private readonly EmployeeModelFactory _employeeModelFactory;

        public EmployeeController()
        {
            _employeeRepo = new EmployeeRepository();
            _employeeModelFactory = new EmployeeModelFactory();
        }

        // GET api/employees
        [Authorize]
        [Route("")]
        public PageResult<EmployeeModel> GetEmployees(ODataQueryOptions<ApplicationUser> options)
        {
            var query = _employeeRepo.Get();
            long? count = 0;

            // set total quantity
            count = options.Filter != null
                ? options.Filter.ApplyTo(query, new ODataQuerySettings()).Count() 
                : query.Count();

            // create query
            var filteredQuery = options.ApplyTo(query, new ODataQuerySettings()) as IQueryable<ApplicationUser>;

            // convert query to list to allow manipulation
            var employees = filteredQuery != null ? filteredQuery.ToList() : new List<ApplicationUser>();

            // create a collection of EmployeeModel instead of ApplicationUser
            var result = employees.Select(s => new EmployeeModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                UserName = s.UserName,
                MiddleInitial = s.MiddleInitial,
                LastName = s.LastName,
                SecondLastName = s.SecondLastName,
                JobTitle = s.JobTitle,
                Location = s.Location,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Role = _employeeModelFactory.GetRoleName(s.Id)
            });

            return new PageResult<EmployeeModel>(result, null, count);
        }

        [Authorize]
        [Route("{id}")]
        public IHttpActionResult GetEmployee(string id)
        {
            // find employee
            var employee = _employeeRepo.GetEmployee(id);

            // check if employee was found
            if (employee == null) return NotFound();

            // get employee roles
            var userRole = employee.Roles.FirstOrDefault();

            if (userRole != null)
            {
                // create employee model
                var employeeModel = _employeeModelFactory.Create(employee);

                return Ok(employeeModel);
            }

            return NotFound();
        }

        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteEmployee(string id)
        {
            // remove employeee
            bool deleted = _employeeRepo.DeleteEmployee(id);

            if (deleted)
            {
                _employeeRepo.SaveAll();
                return Ok();
            }

            return NotFound();
        }

        [Authorize]
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult UpdateEmployee([FromUri]string id, [FromBody]EmployeeModel updatedEmployee)
        {
            // find employee
            var employee = _employeeRepo.GetEmployee(id);
            var updated = _employeeRepo.UpdateEmployee(employee, updatedEmployee);

            // update role
            // update role if changed
            var originalEmployeeRole = _employeeModelFactory.GetRoleName(id);

            // compare roles
            if (originalEmployeeRole != updatedEmployee.Role)
            {
                // add role to user
                _employeeModelFactory.SetUserCurrentRole(id, updatedEmployee.Role);
            }

            if (updated)
            {
                _employeeRepo.SaveAll();
                return Ok();
            }

            return BadRequest();
        }
    }
}
