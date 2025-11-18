using Microsoft.AspNetCore.Mvc;
using Homework2.API.Models; // Using the local model we just created
using System.Collections.Generic;
using System.Linq;

namespace Homework2.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PatientController : ControllerBase
    {
        // A simple static list to act as our temporary database
        private static List<Patient> patients = new List<Patient>
        {
            new Patient { Id = 1, name = "John Doe", address = "123 Main St", birthdate = new DateTime(1980, 1, 1), race = "White", gender = "Male" },
            new Patient { Id = 2, name = "Jane Smith", address = "456 Oak Ave", birthdate = new DateTime(1990, 2, 2), race = "Black", gender = "Female" }
        };
        private static int nextId = 3;

        [HttpGet]
        public IEnumerable<Patient> Get()
        {
            return patients;
        }

        [HttpGet("{id}")]
        public ActionResult<Patient> Get(int id)
        {
            var patient = patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();
            return patient;
        }

        [HttpGet("Search/{query}")]
        public IEnumerable<Patient> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return patients;
            return patients.Where(p => p.name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        [HttpPost]
        public Patient Post([FromBody] Patient value)
        {
            value.Id = nextId++;
            patients.Add(value);
            return value;
        }

        [HttpPut]
        public ActionResult Put([FromBody] Patient value)
        {
            var existing = patients.FirstOrDefault(p => p.Id == value.Id);
            if (existing != null)
            {
                existing.name = value.name;
                existing.address = value.address;
                existing.birthdate = value.birthdate;
                existing.race = value.race;
                existing.gender = value.gender;
                return Ok(existing);
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var patient = patients.FirstOrDefault(p => p.Id == id);
            if (patient != null)
            {
                patients.Remove(patient);
                return Ok();
            }
            return NotFound();
        }
    }
}