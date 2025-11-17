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
                // Cascade delete: Remove all appointments for this patient
                var appointmentsToDelete = _appointments
                    .Where(a => a?.patients?.Id == patientId)
                    .ToList();
                
                foreach (var appointment in appointmentsToDelete)
                {
                    if (appointment != null)
                    {
                        DeleteAppointment(appointment.Id);
                    }
                }
                
                _patients.Remove(patient);
            }
        }

        // --- Physician CRUD ---
        
        public List<Physician?> GetPhysicians() => _physicians;
        
        public Physician? GetPhysician(int id) => _physicians.FirstOrDefault(p => p?.Id == id);

        public Physician AddPhysician(Physician physician)
        {
            physician.Id = _nextPhysicianId++;
            _physicians.Add(physician);
            return physician;
        }

        public void UpdatePhysician(Physician updatedPhysician)
        {
            var physician = GetPhysician(updatedPhysician.Id ?? -1);
            if (physician != null)
            {
                physician.name = updatedPhysician.name;
                physician.license_number = updatedPhysician.license_number;
                physician.graduation = updatedPhysician.graduation;
                physician.specializations = updatedPhysician.specializations;
            }
        }

        public void DeletePhysician(int physicianId)
        {
            var physician = GetPhysician(physicianId);
            if (physician != null)
            {
                // Cascade delete: Remove all appointments for this physician
                var appointmentsToDelete = _appointments
                    .Where(a => a?.physicians?.Id == physicianId)
                    .ToList();
                
                foreach (var appointment in appointmentsToDelete)
                {
                    if (appointment != null)
                    {
                        DeleteAppointment(appointment.Id);
                    }
                }
                
                _physicians.Remove(physician);
            }
        }


        // --- Appointment CRUD ---
        
        public List<Appointment?> GetAppointments() => _appointments;
        
        public Appointment? GetAppointment(int id) => _appointments.FirstOrDefault(a => a?.Id == id);
        
        public List<DateTime> GetAvailableSlots(DateTime date, Patient patient, List<Physician?> physicians)
        {
            var availableSlots = new List<DateTime>();
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return availableSlots;

            for (int hour = 8; hour <= 17; hour++)
            {
                var time = date.Date.AddHours(hour);
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

        public void UpdateAppointment(Appointment updatedAppointment)
        {
            var appointment = GetAppointment(updatedAppointment.Id);
            if (appointment != null)
            {
                // Remove old unavailable hours
                appointment.patients?.unavailable_hours.Remove(appointment.hour);
                appointment.physicians?.unavailable_hours.Remove(appointment.hour);
                
                // Update the appointment
                appointment.patients = updatedAppointment.patients;
                appointment.physicians = updatedAppointment.physicians;
                appointment.hour = updatedAppointment.hour;
                
                // Add new unavailable hours
                appointment.patients?.unavailable_hours.Add(appointment.hour);
                appointment.physicians?.unavailable_hours.Add(appointment.hour);
            }
        }

        public void DeleteAppointment(int appointmentId)
        {
            var appointment = GetAppointment(appointmentId);
            if (appointment != null)
            {
                // Free up the time slot for both patient and physician
                appointment.patients?.unavailable_hours.Remove(appointment.hour);
                appointment.physicians?.unavailable_hours.Remove(appointment.hour);
                
                _appointments.Remove(appointment);
            }
        }
    }
}