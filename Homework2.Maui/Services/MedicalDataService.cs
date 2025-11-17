using Homework2.Maui.Models;
using System.Collections.Generic;
using System.Linq;

namespace Homework2.Maui.Services
{
    public class MedicalDataService
    {
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
                var appointmentsToDelete = _appointments
                    .Where(a => a?.patients?.Id == patientId)
                    .ToList();
                
                foreach (var appointment in appointmentsToDelete)
                {
                    if (appointment != null) DeleteAppointment(appointment.Id);
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
                var appointmentsToDelete = _appointments
                    .Where(a => a?.physicians?.Id == physicianId)
                    .ToList();
                
                foreach (var appointment in appointmentsToDelete)
                {
                    if (appointment != null) DeleteAppointment(appointment.Id);
                }
                
                _physicians.Remove(physician);
            }
        }

        // --- Appointment CRUD ---
        
        public List<Appointment?> GetAppointments() => _appointments;
        
        public Appointment? GetAppointment(int id) => _appointments.FirstOrDefault(a => a?.Id == id);

        // Helper to refresh the cache (kept for compatibility, though we use robust checks now)
        private void RefreshAllAvailability()
        {
            foreach (var p in _patients) p?.unavailable_hours.Clear();
            foreach (var p in _physicians) p?.unavailable_hours.Clear();

            foreach (var appt in _appointments)
            {
                if (appt != null)
                {
                    appt.patients?.unavailable_hours.Add(appt.hour);
                    appt.physicians?.unavailable_hours.Add(appt.hour);
                }
            }
        }

        // ROBUST: Check availability by scanning the actual appointment list for conflicts
        public List<DateTime> GetAvailableSlots(DateTime date, Patient patient, int? excludeAppointmentId = null)
        {
            var availableSlots = new List<DateTime>();
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return availableSlots;

            for (int hour = 8; hour <= 17; hour++)
            {
                var timeSlot = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                
                // 1. Check if Patient is busy in ANY OTHER appointment
                bool patientBusy = _appointments.Any(a => 
                    a.Id != excludeAppointmentId && // Ignore the appointment we are currently editing
                    a.patients?.Id == patient.Id && // Check for THIS patient
                    a.hour == timeSlot              // Check for THIS time
                );

                // 2. Check if ANY Physician is free
                bool anyPhysicianFree = _physicians.Any(ph => {
                    if (ph == null) return false;
                    
                    // Check if THIS physician is busy in ANY OTHER appointment
                    bool physicianBusy = _appointments.Any(a =>
                        a.Id != excludeAppointmentId &&
                        a.physicians?.Id == ph.Id &&
                        a.hour == timeSlot
                    );
                    
                    return !physicianBusy;
                });
                
                // Only valid if Patient is free AND at least one Physician is free
                if (!patientBusy && anyPhysicianFree)
                {
                    availableSlots.Add(timeSlot);
                }
            }
            
            return availableSlots;
        }

        public List<Physician?> GetAvailablePhysicians(DateTime timeSlot, int? excludeAppointmentId = null)
        {
            return _physicians.Where(ph => 
            {
                if (ph == null) return false;
                
                // Check if this physician is busy in ANY OTHER appointment
                bool isBusy = _appointments.Any(a => 
                    a.Id != excludeAppointmentId && // Ignore the appointment we are currently editing
                    a.physicians?.Id == ph.Id &&
                    a.hour == timeSlot
                );
                
                return !isBusy;
            }).ToList();
        }

        public bool IsPhysicianAvailable(Physician? physician, DateTime time)
        {
            if (physician == null || time == default) return false;

            // Robust check
            return !_appointments.Any(a => a.physicians?.Id == physician.Id && a.hour == time);
        }

        public Appointment CreateAppointment(Patient patient, Physician physician, DateTime time)
        {
            var normalizedTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
            
            var newAppointment = new Appointment
            {
                Id = _nextAppointmentId++,
                patients = patient,
                physicians = physician,
                hour = normalizedTime
            };

            _appointments.Add(newAppointment);
            RefreshAllAvailability(); // Update caches

            return newAppointment;
        }

        public void UpdateAppointment(Appointment updatedAppointment)
        {
            var appointment = GetAppointment(updatedAppointment.Id);
            if (appointment != null)
            {
                var normalizedTime = new DateTime(
                    updatedAppointment.hour.Year, 
                    updatedAppointment.hour.Month, 
                    updatedAppointment.hour.Day, 
                    updatedAppointment.hour.Hour, 
                    0, 0);
                
                appointment.patients = updatedAppointment.patients;
                appointment.physicians = updatedAppointment.physicians;
                appointment.hour = normalizedTime;
                
                RefreshAllAvailability(); // Update caches
            }
        }

        public void DeleteAppointment(int appointmentId)
        {
            var appointment = GetAppointment(appointmentId);
            if (appointment != null)
            {
                _appointments.Remove(appointment);
                RefreshAllAvailability(); // Update caches
            }
        }
    }
}