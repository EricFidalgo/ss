using Homework2.Maui.Models;
using Library.eCommerce.Utilities; // Namespace from your WebRequestHandler
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homework2.Maui.Services
{
    public class MedicalDataService
    {
        private readonly WebRequestHandler _webRequestHandler;

        // Patient list is now managed by the API, so we don't need a class-level _patients list for them.
        // We keep these lists for entities not yet on the API:
        private List<Physician?> _physicians = new List<Physician?>();
        private List<Appointment?> _appointments = new List<Appointment?>();
        
        private int _nextPhysicianId = 1;
        private int _nextAppointmentId = 1;

        public MedicalDataService()
        {
            _webRequestHandler = new WebRequestHandler();
        }

        // --- Patient CRUD (API Integration) ---

        public async Task<List<Patient>> GetPatients()
        {
            var response = await _webRequestHandler.Get("/Patient");
            if (response != null)
            {
                return JsonConvert.DeserializeObject<List<Patient>>(response) ?? new List<Patient>();
            }
            return new List<Patient>();
        }

        public async Task<Patient?> GetPatient(int id)
        {
            var response = await _webRequestHandler.Get($"/Patient/{id}");
            if (response != null)
            {
                return JsonConvert.DeserializeObject<Patient>(response);
            }
            return null;
        }

        public async Task<Patient> AddPatient(Patient patient)
        {
            var response = await _webRequestHandler.Post("/Patient", patient);
            if (response != null && response != "ERROR")
            {
                return JsonConvert.DeserializeObject<Patient>(response);
            }
            return null;
        }

        public async Task UpdatePatient(Patient updatedPatient)
        {
            // NOTE: Ensure your WebRequestHandler has a 'Put' method similar to 'Post'
            await _webRequestHandler.Put("/Patient", updatedPatient);
        }

        public async Task DeletePatient(int patientId)
        {
            await _webRequestHandler.Delete($"/Patient/{patientId}");
        }

        public async Task<List<Patient>> SearchPatients(string query)
        {
            var response = await _webRequestHandler.Get($"/Patient/Search/{query}");
            if (response != null)
            {
                return JsonConvert.DeserializeObject<List<Patient>>(response) ?? new List<Patient>();
            }
            return new List<Patient>();
        }

        // --- Physician CRUD (In-Memory) ---
        
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

        // --- Appointment CRUD (In-Memory) ---
        
        public List<Appointment?> GetAppointments() => _appointments;
        
        public Appointment? GetAppointment(int id) => _appointments.FirstOrDefault(a => a?.Id == id);

        private void RefreshAllAvailability()
        {
            // Note: This logic assumes patients are in memory, which they aren't anymore.
            // For a full real-world app, availability checking would move to the backend.
            // For this exercise, we will skip clearing patient hours to avoid errors, 
            // or you would need to fetch the patient first.
            
            foreach (var p in _physicians) p?.unavailable_hours.Clear();

            foreach (var appt in _appointments)
            {
                if (appt != null)
                {
                    // We can't easily update the patient object's unavailable_hours since 
                    // the patient object is no longer held in a persistent list here.
                    // appt.patients?.unavailable_hours.Add(appt.hour); 
                    
                    appt.physicians?.unavailable_hours.Add(appt.hour);
                }
            }
        }

        public List<DateTime> GetAvailableSlots(DateTime date, Patient patient, int? excludeAppointmentId = null)
        {
            var availableSlots = new List<DateTime>();
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return availableSlots;

            for (int hour = 8; hour <= 17; hour++)
            {
                var timeSlot = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                
                bool patientBusy = _appointments.Any(a => 
                    a.Id != excludeAppointmentId && 
                    a.patients?.Id == patient.Id && 
                    a.hour == timeSlot              
                );

                bool anyPhysicianFree = _physicians.Any(ph => {
                    if (ph == null) return false;
                    
                    bool physicianBusy = _appointments.Any(a =>
                        a.Id != excludeAppointmentId &&
                        a.physicians?.Id == ph.Id &&
                        a.hour == timeSlot
                    );
                    
                    return !physicianBusy;
                });
                
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
                
                bool isBusy = _appointments.Any(a => 
                    a.Id != excludeAppointmentId && 
                    a.physicians?.Id == ph.Id &&
                    a.hour == timeSlot
                );
                
                return !isBusy;
            }).ToList();
        }

        public bool IsPhysicianAvailable(Physician? physician, DateTime time, int? excludeAppointmentId = null)
        {
            if (physician == null || time == default) return false;

            return !_appointments.Any(a => 
                a.Id != excludeAppointmentId && 
                a.physicians?.Id == physician.Id && 
                a.hour == time);
        }

        public bool IsRoomAvailable(string room, DateTime time, int? excludeAppointmentId = null)
        {
            if (string.IsNullOrWhiteSpace(room)) return true;

            return !_appointments.Any(a => 
                a.Id != excludeAppointmentId && 
                string.Equals(a.Room, room, System.StringComparison.OrdinalIgnoreCase) && 
                a.hour == time);
        }

        public Appointment CreateAppointment(Patient patient, Physician physician, DateTime time, string room)
        {
            var normalizedTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
            
            var newAppointment = new Appointment
            {
                Id = _nextAppointmentId++,
                patients = patient,
                physicians = physician,
                hour = normalizedTime,
                Room = room 
            };

            _appointments.Add(newAppointment);
            RefreshAllAvailability(); 

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
                appointment.Room = updatedAppointment.Room; 
                appointment.Treatments = updatedAppointment.Treatments;
                
                RefreshAllAvailability(); 
            }
        }

        public void DeleteAppointment(int appointmentId)
        {
            var appointment = GetAppointment(appointmentId);
            if (appointment != null)
            {
                _appointments.Remove(appointment);
                RefreshAllAvailability(); 
            }
        }
    }
}