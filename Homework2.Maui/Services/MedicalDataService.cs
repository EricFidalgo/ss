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
        
        public List<DateTime> GetAvailableSlots(DateTime date, Patient patient, List<Physician?> physicians, int? excludeAppointmentId = null)
        {
            var availableSlots = new List<DateTime>();
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return availableSlots;

            for (int hour = 8; hour <= 17; hour++)
            {
                var timeSlot = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                
                // Check if this is the appointment being edited (should show as available)
                bool isCurrentAppointment = false;
                if (excludeAppointmentId.HasValue)
                {
                    var editingAppointment = GetAppointment(excludeAppointmentId.Value);
                    if (editingAppointment != null && editingAppointment.hour == timeSlot)
                    {
                        isCurrentAppointment = true;
                    }
                }
                
                // Patient must be free at this exact time
                bool patientAvailable = isCurrentAppointment || 
                    !patient.unavailable_hours.Any(dt => 
                        dt.Year == timeSlot.Year && 
                        dt.Month == timeSlot.Month && 
                        dt.Day == timeSlot.Day && 
                        dt.Hour == timeSlot.Hour);
                
                // At least one physician must be free at this exact time
                bool physicianAvailable = physicians.Any(ph => 
                {
                    if (ph == null) return false;
                    
                    bool isPhysicianBusy = ph.unavailable_hours.Any(dt => 
                        dt.Year == timeSlot.Year && 
                        dt.Month == timeSlot.Month && 
                        dt.Day == timeSlot.Day && 
                        dt.Hour == timeSlot.Hour);
                    
                    return !isPhysicianBusy || isCurrentAppointment;
                });
                
                if (patientAvailable && physicianAvailable)
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
                
                // Check if this is the appointment being edited
                bool isCurrentAppointment = false;
                if (excludeAppointmentId.HasValue)
                {
                    var editingAppointment = GetAppointment(excludeAppointmentId.Value);
                    if (editingAppointment != null && 
                        editingAppointment.physicians?.Id == ph.Id && 
                        editingAppointment.hour == timeSlot)
                    {
                        isCurrentAppointment = true;
                    }
                }
                
                // Check if physician is busy at this exact time
                bool isBusy = ph.unavailable_hours.Any(dt => 
                    dt.Year == timeSlot.Year && 
                    dt.Month == timeSlot.Month && 
                    dt.Day == timeSlot.Day && 
                    dt.Hour == timeSlot.Hour);
                
                return !isBusy || isCurrentAppointment;
            }).ToList();
        }

        public bool IsPhysicianAvailable(Physician? physician, DateTime time)
        {
            if (physician == null || time == default)
            {
                return false;
            }

            // A physician is available if they don't have an appointment at the exact given time.
            // This uses the unavailable_hours list which is managed when appointments are created/updated/deleted.
            return !physician.unavailable_hours.Any(dt => dt == time);
        }

        public Appointment CreateAppointment(Patient patient, Physician physician, DateTime time)
        {
            // Normalize the time to remove seconds and milliseconds
            var normalizedTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
            
            var newAppointment = new Appointment
            {
                Id = _nextAppointmentId++,
                patients = patient,
                physicians = physician,
                hour = normalizedTime
            };

            patient.unavailable_hours.Add(normalizedTime);
            physician.unavailable_hours.Add(normalizedTime);
            _appointments.Add(newAppointment);

            return newAppointment;
        }

        public void UpdateAppointment(Appointment updatedAppointment)
        {
            var appointment = GetAppointment(updatedAppointment.Id);
            if (appointment != null)
            {
                // Normalize the new time
                var normalizedTime = new DateTime(
                    updatedAppointment.hour.Year, 
                    updatedAppointment.hour.Month, 
                    updatedAppointment.hour.Day, 
                    updatedAppointment.hour.Hour, 
                    0, 0);
                
                // Remove old unavailable hours from OLD patient and physician
                appointment.patients?.unavailable_hours.Remove(appointment.hour);
                appointment.physicians?.unavailable_hours.Remove(appointment.hour);
                
                // Update the appointment with new data
                appointment.patients = updatedAppointment.patients;
                appointment.physicians = updatedAppointment.physicians;
                appointment.hour = normalizedTime;
                
                // Add new unavailable hours to NEW patient and physician
                appointment.patients?.unavailable_hours.Add(normalizedTime);
                appointment.physicians?.unavailable_hours.Add(normalizedTime);
            }
        }

        public void DeleteAppointment(int appointmentId)
        {
            var appointment = GetAppointment(appointmentId);
            if (appointment != null)
            {
                appointment.patients?.unavailable_hours.Remove(appointment.hour);
                appointment.physicians?.unavailable_hours.Remove(appointment.hour);
                
                _appointments.Remove(appointment);
            }
        }
    }
}