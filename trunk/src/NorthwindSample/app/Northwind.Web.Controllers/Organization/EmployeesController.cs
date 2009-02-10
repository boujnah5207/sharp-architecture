﻿using System.Web.Mvc;
using Northwind.Core.Organization;
using SharpArch.Core.PersistenceSupport;
using SharpArch.Core.DomainModel;
using System.Collections.Generic;
using System;
using SharpArch.Web.NHibernate;
using NHibernate.Validator.Engine;
using System.Text;
using SharpArch.Web.CommonValidator;
using SharpArch.Core;

namespace Northwind.Web.Controllers.Organization
{
    [HandleError]
    public class EmployeesController : Controller
    {
        public EmployeesController(IRepository<Employee> employeeRepository) {
            Check.Require(employeeRepository != null, "employeeRepository may not be null");

            this.employeeRepository = employeeRepository;
        }

        /// <summary>
        /// The transaction on this action is optional, but recommended for performance reasons
        /// </summary>
        [Transaction]
        public ActionResult Index() {
            List<Employee> employees = employeeRepository.GetAll();
            return View(employees);
        }

        /// <summary>
        /// The transaction on this action is optional, but recommended for performance reasons
        /// </summary>
        [Transaction]
        public ActionResult Show(int id) {
            Employee employee = employeeRepository.Get(id);
            return View(employee);
        }

        public ActionResult Create() {
            return View();
        }

        [ValidateAntiForgeryToken]      // Helps avoid CSRF attacks
        [Transaction]                   // Wraps a transaction around the action
        [AcceptVerbs(HttpVerbs.Post)]   // Limits the method to only accept post requests
        public ActionResult Create(Employee employee) {
            if (employee.IsValid()) {
                employeeRepository.SaveOrUpdate(employee);

                TempData["message"] = employee.FullName + " was successfully created.";
                return RedirectToAction("Index");
            }
            else {
                MvcValidationAdapter.TransferValidationMessagesTo(ViewData.ModelState, employee.ValidationResults());
                return View();
            }
        }

        /// <summary>
        /// The transaction on this action is optional, but recommended for performance reasons
        /// </summary>
        [Transaction]
        public ActionResult Edit(int id) {
            Employee employee = employeeRepository.Get(id);
            return View(employee);
        }

        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(int id, Employee employee) {
            Employee employeeToUpdate = employeeRepository.Get(id);
            employeeToUpdate.FirstName = employee.FirstName;
            employeeToUpdate.LastName = employee.LastName;
            employeeToUpdate.PhoneExtension = employee.PhoneExtension;

            if (employeeToUpdate.IsValid()) {
                TempData["message"] = employeeToUpdate.FullName + " was successfully updated.";
                return RedirectToAction("Index");
            }
            else {
                employeeRepository.DbContext.RollbackTransaction();
                MvcValidationAdapter.TransferValidationMessagesTo(ViewData.ModelState, employee.ValidationResults());
                return View(employee);
            }
        }

        [Transaction]
        public ActionResult Delete(int id) {
            string resultMessage = null;
            Employee employeeToDelete = employeeRepository.Get(id);

            if (employeeToDelete != null) {
                employeeRepository.Delete(employeeToDelete);

                try {
                    employeeRepository.DbContext.CommitChanges();
                }
                catch {
                    resultMessage = "The employee couldn't be deleted; likely due to a foreign key " +
                        "reference. You could cascade the deletion or you could inform " +
                        "the user better on where the foreign dependencies are and what needs to be " +
                        "done before the employee can be deleted.";
                    employeeRepository.DbContext.RollbackTransaction();
                }
            }
            else {
                resultMessage = "An employee with the ID of " + id + " could not be found for deletion.";
            }

            if (resultMessage == null) {
                resultMessage = "The employee was successfully deleted.";
            }

            TempData["Message"] = resultMessage;
            return RedirectToAction("Index");
        }

        private readonly IRepository<Employee> employeeRepository;
    }
}
