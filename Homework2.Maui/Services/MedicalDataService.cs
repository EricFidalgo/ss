using Homework2.Maui.Models;
using System.Collections.Generic;
using System.Linq;

namespace Homework2.Maui.Services
{
    public class MedicalDataService
    {
        // Data lists are now private. Access is controlled by public methods.
        private List<Patient?> _patients = new List<Patient?>();
        private List<Physician?> _physicians = new List<Physician?>();
        private List<Appointment?> _appointments = new List<Appointment?>();
        private int _nextPatientId = 1;
        private int _nextPhysicianId = 1;
        private int _nextAppointmentId = 1;


        // --- Patient CRUD ---

        public List<Patient?> GetPatients() => _patients;

        public Patient? GetPatient(int id) => _patients.FirstOrDefault(p => p?.Id == id);

        public Patient AddPatient(Patient patient)
        {
            patient.Id = _nextPatientId++;
            _patients.Add(patient);
            return patient;
        }

        public void UpdatePatient(Patient updatedPatient)
        {
            var patient = GetPatient(updatedPatient.Id ?? -1);
            if (patient != null)
            {
                patient.name = updatedPatient.name;
                patient.address = updatedPatient.address;
                patient.birthdate = updatedPatient.birthdate;
                patient.race = updatedPatient.race;
                patient.gender = updatedPatient.gender;
                patient.medical_notes = updatedPatient.medical_notes;
                patient.diagnoses = updatedPatient.diagnoses;
                patient.prescriptions = updatedPatient.prescriptions;
            }
        }

        public void DeletePatient(int patientId)
        {
            var patient = GetPatient(patientId);
            if (patient != null)
            {
                _patients.Remove(patient);
            }
        }

        // --- Physician CRUD (You would add methods here) ---
        public List<Physician?> GetPhysicians() => _physicians;
        // ... AddPhysician, UpdatePhysician, DeletePhysician, etc.


        // --- Appointment CRUD (You would add methods here) ---
        public List<Appointment?> GetAppointments() => _appointments;
        
        // This logic is now UI-friendly. The UI will call these methods.
        public List<DateTime> GetAvailableSlots(DateTime date, Patient patient, List<Physician?> physicians)
        {
            var availableSlots = new List<DateTime>();
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return availableSlots; // Empty list

            for (int hour = 8; hour <= 17; hour++)
            {
                var time = date.Date.AddHours(hour);
                // Check if patient is free AND at least one physician is free
                if (!patient.unavailable_hours.Contains(time) && 
                    physicians.Any(ph => ph != null && !ph.unavailable_hours.Contains(time)))
                {
                    availableSlots.Add(time);
                }
            }
            return availableSlots;
        }

        public List<Physician?> GetAvailablePhysicians(DateTime time)
        {
            return _physicians.Where(ph => ph != null && !ph.unavailable_hours.Contains(time)).ToList();
        }

        public Appointment CreateAppointment(Patient patient, Physician physician, DateTime time)
        {
            var newAppointment = new Appointment
            {
                Id = _nextAppointmentId++,
                patients = patient,
                physicians = physician,
                hour = time
            };

            patient.unavailable_hours.Add(time);
            physician.unavailable_hours.Add(time);
            _appointments.Add(newAppointment);

            return newAppointment;
        }

        // ... UpdateAppointment, DeleteAppointment, etc.
    }
}