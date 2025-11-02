
public static class MedicalDataService
{
  public static Patient createPatient(List<Patient?> patients)
  {
    var patient = new Patient();

    var maxId = -1;

    if (patients.Any())
    {
      maxId = patients.Select(reader => reader?.Id ?? -1).Max();
    }
    else
    {
      maxId = 0;
    }
    patient.Id = ++maxId;

    Console.WriteLine("What is the patients name?");
    string? name = Console.ReadLine();
    patient.name = name;


    Console.WriteLine("What is the patients address?");
    string? address = Console.ReadLine();
    patient.address = address;

    Console.WriteLine("What is the patients birthdate? (MM/DD/YYYY)");
    bool isValidDate = false;
    do
    {
      try
      {
        DateTime birthdate = DateTime.Parse(Console.ReadLine());
        patient.birthdate = birthdate;
        isValidDate = true;
      }
      catch
      {
        Console.WriteLine("Invalid format. Please use MM/DD/YYYY.");
      }
    } while (isValidDate == false);


    Console.WriteLine("What is the patients race?");
    string? race = Console.ReadLine();
    patient.race = race;

    Console.WriteLine("What is the patients gender? (M/F)");
    bool isValidGender = false;
    do
    {
      string? gender = Console.ReadLine();
      if (gender == "M" || gender == "m" || gender == "F" || gender == "f")
      {
        patient.gender = gender;
        isValidGender = true;
      }
      else
      {
        Console.WriteLine("Invalid Gender.");
      }
    } while (isValidGender == false);

    return patient;
  }

  public static void addMedicalNote(List<Patient?> patients)
  {
    AddPatientDetail(patients, "medical note", (patient, note) => patient.medical_notes.Add(note));
  }

  public static void addDiagnosis(List<Patient?> patients)
  {
    AddPatientDetail(patients, "diagnosis", (patient, diagnosis) => patient.diagnoses.Add(diagnosis));
  }

  public static void addPrescription(List<Patient?> patients)
  {
    AddPatientDetail(patients, "prescription", (patient, prescription) => patient.prescriptions.Add(prescription));
  }

  private static void AddPatientDetail(List<Patient?> patients, string detailType, Action<Patient, string> addAction)
  {
    Console.WriteLine("Please Choose the patients ID:");

    foreach (var patient in patients)
    {
      Console.WriteLine($"({patient?.Id}) {patient?.name}");
    }

    bool patientFound = false;
    do
    {
      try
      {
        Console.WriteLine("Please enter a patient ID:");
        int userId = int.Parse(Console.ReadLine());
        var chosenPatient = patients.FirstOrDefault(p => p?.Id == userId);
        if (chosenPatient == null) throw new ArgumentOutOfRangeException();

        Console.WriteLine($"Type in the patient's {detailType}:");
        string? input = Console.ReadLine();
        addAction(chosenPatient, input);
        Console.WriteLine($"{char.ToUpper(detailType[0]) + detailType.Substring(1)} added.");
        patientFound = true;
      }
      catch (Exception ex)
      {
        Console.WriteLine("Invalid ID. Please choose a valid ID.");
        // Optional: Log the exception for debugging
        // Console.WriteLine(ex.Message);
      }
    } while (patientFound == false);
  }

  public static void UpdatePatient(List<Patient?> patients)
  {
    Console.WriteLine("Please choose the patient ID to update:");
    foreach (var patient in patients)
    {
      Console.WriteLine($"({patient?.Id}) {patient?.name}");
    }

    int patientId;
    while (true)
    {
      Console.WriteLine("Enter the ID of the patient you want to update:");
      if (int.TryParse(Console.ReadLine(), out patientId))
      {
        var patientToUpdate = patients.FirstOrDefault(p => p?.Id == patientId);
        if (patientToUpdate != null)
        {
          Console.WriteLine($"Updating patient: {patientToUpdate.name}");
          Console.WriteLine("What is the patient's new name?");
          patientToUpdate.name = Console.ReadLine();
          Console.WriteLine("What is the patient's new address?");
          patientToUpdate.address = Console.ReadLine();
          Console.WriteLine("Patient information updated successfully.");
          break;
        }
        else
        {
          Console.WriteLine("Patient not found. Please enter a valid ID.");
        }
      }
      else
      {
        Console.WriteLine("Invalid input. Please enter a number.");
      }
    }
  }

  public static void DeletePatient(List<Patient?> patients)
  {
    Console.WriteLine("Please choose the patient ID to delete:");
    foreach (var patient in patients)
    {
      Console.WriteLine($"({patient?.Id}) {patient?.name}");
    }

    int patientId;
    while (true)
    {
      Console.WriteLine("Enter the ID of the patient you want to delete:");
      if (int.TryParse(Console.ReadLine(), out patientId))
      {
        var patientToDelete = patients.FirstOrDefault(p => p?.Id == patientId);
        if (patientToDelete != null)
        {
          patients.Remove(patientToDelete);
          Console.WriteLine("Patient deleted successfully.");
          break;
        }
        else
        {
          Console.WriteLine("Patient not found. Please enter a valid ID.");
        }
      }
      else
      {
        Console.WriteLine("Invalid input. Please enter a number.");
      }
    }
  }

  public static Physician createPhysician(List<Physician?> physicians)
  {
    var physician = new Physician();

    var maxId = -1;

    if (physicians.Any())
    {
      maxId = physicians.Select(reader => reader?.Id ?? -1).Max();
    }
    else
    {
      maxId = 0;
    }
    physician.Id = ++maxId;

    Console.WriteLine("What is the physician's name?");
    string? name = Console.ReadLine();
    physician.name = name;

    Console.WriteLine("What is the physicians graduation date? (MM/DD/YYYY)");
    bool isValidDate = false;
    do
    {
      try
      {
        DateTime graduation = DateTime.Parse(Console.ReadLine());
        physician.graduation = graduation;
        isValidDate = true;
      }
      catch
      {
        Console.WriteLine("Invalid format. Please use MM/DD/YYYY.");
      }
    } while (isValidDate == false);

    Console.WriteLine("What is the physicians license number?");
    string? license_number = Console.ReadLine();
    physician.license_number = license_number;

    return physician;
  }

  public static void addSpecialization(List<Physician?> physicians)
  {
    Console.WriteLine("Please Choose the physicians ID:");
    foreach (var physician in physicians) // Changed variable name from 'specialization' to 'physician' for clarity
    {
      Console.WriteLine($"({physician?.Id}) {physician?.name}");
    }

    bool physician_found = false;
    do
    {
      try
      {
        Console.WriteLine("Please enter a physician ID:");
        int user_id = int.Parse(Console.ReadLine());
        // Using FirstOrDefault for safer ID lookup
        var chosen_physician = physicians.FirstOrDefault(p => p?.Id == user_id);
        if (chosen_physician != null)
        {
          Console.WriteLine("Type in the physician's specialization:");
          string? specialization_input = Console.ReadLine();
          chosen_physician.specializations.Add(specialization_input);
          Console.WriteLine("Specialization added.");
          physician_found = true;
        }
        else
        {
          Console.WriteLine("Invalid ID. Please choose a valid ID.");
        }
      }
      catch (FormatException) // Catch specific exception for parsing errors
      {
        Console.WriteLine("Invalid input. Please enter a number.");
      }
      catch (Exception ex) // Catch other potential exceptions
      {
        Console.WriteLine($"An error occurred: {ex.Message}");
      }
    } while (physician_found == false);
  }

  public static void UpdatePhysician(List<Physician?> physicians)
  {
    Console.WriteLine("Please choose the physician ID to update:");
    foreach (var physician in physicians)
    {
      Console.WriteLine($"({physician?.Id}) {physician?.name}");
    }

    int physicianId;
    while (true)
    {
      Console.WriteLine("Enter the ID of the physician you want to update:");
      if (int.TryParse(Console.ReadLine(), out physicianId))
      {
        var physicianToUpdate = physicians.FirstOrDefault(p => p?.Id == physicianId);
        if (physicianToUpdate != null)
        {
          Console.WriteLine($"Updating physician: {physicianToUpdate.name}");

          Console.WriteLine($"What is the physician's new name? (current: {physicianToUpdate.name})");
          string? newName = Console.ReadLine();
          if (!string.IsNullOrWhiteSpace(newName))
          {
            physicianToUpdate.name = newName;
          }

          Console.WriteLine($"What is the physician's new license number? (current: {physicianToUpdate.license_number})");
          string? newLicense = Console.ReadLine();
          if (!string.IsNullOrWhiteSpace(newLicense))
          {
            physicianToUpdate.license_number = newLicense;
          }

          Console.WriteLine($"What is the physician's new graduation date? (current: {physicianToUpdate.graduation.ToShortDateString()}) (MM/DD/YYYY)");
          bool isValidDate = false;
          do {
            string? newDateInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newDateInput)) break; // Keep old date if input is empty
            if (DateTime.TryParse(newDateInput, out DateTime newDate)) {
              physicianToUpdate.graduation = newDate;
              isValidDate = true;
            } else {
              Console.WriteLine("Invalid format. Please use MM/DD/YYYY or press Enter to skip.");
            }
          } while (!isValidDate);

          Console.WriteLine("Physician information updated successfully.");
          break;
        }
        else
        {
          Console.WriteLine("Physician not found. Please enter a valid ID.");
        }
      }
      else
      {
        Console.WriteLine("Invalid input. Please enter a number.");
      }
    }
  }

  public static void DeletePhysician(List<Physician?> physicians)
  {
    Console.WriteLine("Please choose the physician ID to delete:");
    foreach (var physician in physicians)
    {
      Console.WriteLine($"({physician?.Id}) {physician?.name}");
    }

    int physicianId;
    while (true)
    {
      Console.WriteLine("Enter the ID of the physician you want to delete:");
      if (int.TryParse(Console.ReadLine(), out physicianId))
      {
        var physicianToDelete = physicians.FirstOrDefault(p => p?.Id == physicianId);
        if (physicianToDelete != null)
        {
          physicians.Remove(physicianToDelete);
          Console.WriteLine("Physician deleted successfully.");
          break;
        }
        else
        {
          Console.WriteLine("Physician not found. Please enter a valid ID.");
        }
      }
      else
      {
        Console.WriteLine("Invalid input. Please enter a number.");
      }
    }
  }

  public static void CreateAppointment(List<Patient?> patients, List<Physician?> physicians, List<Appointment?> appointments)
  {
    if (!patients.Any() || !physicians.Any())
    {
      Console.WriteLine("Please add at least one patient and one physician to create an appointment.");
      return;
    }

    // 1. Select Patient
    var chosenPatient = SelectPatient(patients, "schedule an appointment for");
    if (chosenPatient == null) return;

    // 2. Select Date & Time
    var finalAppointmentTime = SelectAppointmentDateTime(chosenPatient, physicians);
    if (finalAppointmentTime == DateTime.MinValue) return;

    // 3. Select Physician
    var chosenPhysician = SelectAvailablePhysician(physicians, finalAppointmentTime);
    if (chosenPhysician == null) return;

    // 4. Create and Add Appointment
    var newAppointment = new Appointment
    {
      Id = appointments.Any() ? appointments.Max(a => a.Id) + 1 : 1,
      patients = chosenPatient,
      physicians = chosenPhysician,
      hour = finalAppointmentTime
    };

    chosenPatient.unavailable_hours.Add(finalAppointmentTime);
    chosenPhysician.unavailable_hours.Add(finalAppointmentTime);
    appointments.Add(newAppointment);

    Console.WriteLine($"Appointment created successfully for {chosenPatient.name} with Dr. {chosenPhysician.name} at {finalAppointmentTime}.");
  }

  public static void UpdateAppointment(List<Patient?> patients, List<Physician?> physicians, List<Appointment?> appointments)
  {
    if (!appointments.Any())
    {
      Console.WriteLine("No appointments to update.");
      return;
    }

    Console.WriteLine("Please choose the appointment to update:");
    foreach (var appt in appointments)
    {
      Console.WriteLine($"({appt.Id}) {appt.hour} - Patient: {appt.patients.name}, Physician: {appt.physicians.name}");
    }

    Appointment appointmentToUpdate = null;
    while (appointmentToUpdate == null)
    {
      Console.WriteLine("Enter the ID of the appointment you want to update:");
      if (int.TryParse(Console.ReadLine(), out int apptId))
      {
        appointmentToUpdate = appointments.FirstOrDefault(a => a.Id == apptId);
        if (appointmentToUpdate == null)
        {
          Console.WriteLine("Appointment not found. Please enter a valid ID.");
        }
      }
      else
      {
        Console.WriteLine("Invalid input. Please enter a number.");
      }
    }

    // Free up the old slot
    appointmentToUpdate.patients.unavailable_hours.Remove(appointmentToUpdate.hour);
    appointmentToUpdate.physicians.unavailable_hours.Remove(appointmentToUpdate.hour);

    Console.WriteLine("Updating appointment. Please provide new details.");

    // Reuse selection logic
    var newPatient = SelectPatient(patients, "reschedule for");
    if (newPatient == null) return; // Canceled

    var newAppointmentTime = SelectAppointmentDateTime(newPatient, physicians);
    if (newAppointmentTime == DateTime.MinValue) return; // Canceled

    var newPhysician = SelectAvailablePhysician(physicians, newAppointmentTime);
    if (newPhysician == null) return; // Canceled

    // Update the appointment
    appointmentToUpdate.patients = newPatient;
    appointmentToUpdate.physicians = newPhysician;
    appointmentToUpdate.hour = newAppointmentTime;

    // Book the new slot
    newPatient.unavailable_hours.Add(newAppointmentTime);
    newPhysician.unavailable_hours.Add(newAppointmentTime);

    Console.WriteLine("Appointment updated successfully.");
  }

  public static void DeleteAppointment(List<Appointment?> appointments)
  {
    if (!appointments.Any())
    {
      Console.WriteLine("No appointments to delete.");
      return;
    }

    Console.WriteLine("Please choose the appointment to delete:");
    foreach (var appt in appointments)
    {
      Console.WriteLine($"({appt.Id}) {appt.hour} - Patient: {appt.patients.name}, Physician: {appt.physicians.name}");
    }

    Appointment appointmentToDelete = null;
    while (appointmentToDelete == null)
    {
      Console.WriteLine("Enter the ID of the appointment you want to delete:");
      if (int.TryParse(Console.ReadLine(), out int apptId))
      {
        appointmentToDelete = appointments.FirstOrDefault(a => a.Id == apptId);
        if (appointmentToDelete == null)
        {
          Console.WriteLine("Appointment not found. Please enter a valid ID.");
        }
      }
      else
      {
        Console.WriteLine("Invalid input. Please enter a number.");
      }
    }

    // Free up the time slot for both patient and physician
    appointmentToDelete.patients.unavailable_hours.Remove(appointmentToDelete.hour);
    appointmentToDelete.physicians.unavailable_hours.Remove(appointmentToDelete.hour);

    appointments.Remove(appointmentToDelete);
    Console.WriteLine("Appointment deleted successfully.");
  }

  private static Patient SelectPatient(List<Patient?> patients, string action)
  {
    Console.WriteLine($"Please choose the patient to {action}:");
    foreach (var p in patients) Console.WriteLine($"({p.Id}) {p.name}");

    Patient chosenPatient = null;
    while (chosenPatient == null)
    {
      Console.WriteLine("Enter patient ID:");
      if (int.TryParse(Console.ReadLine(), out int patientId))
      {
        chosenPatient = patients.FirstOrDefault(p => p?.Id == patientId);
        if (chosenPatient == null) Console.WriteLine("Patient not found.");
      }
      else
      {
        Console.WriteLine("Invalid ID format.");
      }
    }
    return chosenPatient;
  }

  private static DateTime SelectAppointmentDateTime(Patient patient, List<Physician?> physicians)
  {
    DateTime appointmentDate;
    while (true)
    {
      Console.WriteLine("Enter appointment date (MM/DD/YYYY):");
      if (DateTime.TryParse(Console.ReadLine(), out appointmentDate))
      {
        if (appointmentDate.DayOfWeek == DayOfWeek.Saturday || appointmentDate.DayOfWeek == DayOfWeek.Sunday)
        {
          Console.WriteLine("Appointments are only available Monday through Friday.");
          continue;
        }
        break;
      }
      Console.WriteLine("Invalid date format.");
    }

    Console.WriteLine("\nAvailable Times:");
    for (int hour = 8; hour <= 17; hour++)
    {
      var time = appointmentDate.Date.AddHours(hour);
      if (patient.unavailable_hours.Contains(time))
      {
        Console.WriteLine($"{time:hh:mm tt} - Unavailable (Patient booked)");
      }
      else
      {
        var availablePhysicians = physicians.Where(ph => !ph.unavailable_hours.Contains(time)).ToList();
        if (availablePhysicians.Any())
        {
          Console.WriteLine($"{time:hh:mm tt} - Available with {availablePhysicians.Count} physician(s)");
        }
        else
        {
          Console.WriteLine($"{time:hh:mm tt} - Unavailable (All physicians booked)");
        }
      }
    }

    while (true)
    {
      Console.WriteLine("\nEnter desired hour (8-17):");
      if (int.TryParse(Console.ReadLine(), out int chosenHour) && chosenHour >= 8 && chosenHour <= 17)
      {
        var finalTime = appointmentDate.Date.AddHours(chosenHour);
        if (patient.unavailable_hours.Contains(finalTime))
        {
          Console.WriteLine("Patient is already booked at this time.");
          continue;
        }
        if (!physicians.Any(ph => !ph.unavailable_hours.Contains(finalTime)))
        {
          Console.WriteLine("No physicians are available at this time.");
          continue;
        }
        return finalTime;
      }
      Console.WriteLine("Invalid hour.");
    }
  }

  private static Physician SelectAvailablePhysician(List<Physician?> physicians, DateTime time)
  {
    var availablePhysicians = physicians.Where(ph => !ph.unavailable_hours.Contains(time)).ToList();
    Console.WriteLine("\nAvailable Physicians at this time:");
    foreach (var p in availablePhysicians) Console.WriteLine($"({p.Id}) {p.name}");

    while (true)
    {
      Console.WriteLine("Enter physician ID:");
      if (int.TryParse(Console.ReadLine(), out int physicianId))
      {
        var chosenPhysician = availablePhysicians.FirstOrDefault(p => p.Id == physicianId);
        if (chosenPhysician != null) return chosenPhysician;
        Console.WriteLine("Invalid ID or physician is not available at this time.");
      }
      else
      {
        Console.WriteLine("Invalid ID format.");
      }
    }
  }
}
