using System.Linq.Expressions;
using System.Net;

List<Patient?> patients = new List<Patient?>();
List<Physician?> physicians = new List<Physician?>();
List<Appointment?> appointments = new List<Appointment?>();

string? input;

void patientFunction(List<Patient?> patients)
{
	bool patient_loop = true;
	do
	{
		Console.WriteLine("\nC. Create a new Patient");
		Console.WriteLine("A. Add medical notes to a Patient");
		Console.WriteLine("N. Add a new diagnosis");
		Console.WriteLine("S. Add a new prescription");
		Console.WriteLine("R. Read Patients");
		Console.WriteLine("U. Update a Patient");
		Console.WriteLine("D. Delete a Patient");
		Console.WriteLine("B. Back");
		input = Console.ReadLine();
		switch (input)
		{
		case "C":
		case "c":
			Patient newPatient = MedicalDataService.createPatient(patients);
			patients.Add(newPatient);
			break;
		case "A":
		case "a":
			if (patients.Any())
				MedicalDataService.addMedicalNote(patients);
			else
				Console.WriteLine("No patients have been created.");
			break;
		case "N":
		case "n":
			if (patients.Any())
				MedicalDataService.addDiagnosis(patients);
			else
				Console.WriteLine("No patients have been created.");
			break;
		case "S":
		case "s":
			if (patients.Any())
				MedicalDataService.addPrescription(patients);
			else
				Console.WriteLine("No patients have been created.");
			break;
		case "R":
		case "r":
			string print_line = "PATIENT INFORMATION\n";
			print_line += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
				foreach (var patient in patients)
				{
					print_line += $"Name: ({patient?.Id}) {patient?.name}";
					print_line += (patient?.gender?.ToLower() == "m") ? " 🧔\n" : " 👩\n";
					print_line += $"Address: {patient?.address}\n";
					print_line += $"Birthdate: {patient?.birthdate.ToString("dd/MM/yyyy")}\n";
					print_line += $"Race: {patient?.race}\n";

					if (patient?.medical_notes.Any() == true)
					{
						print_line += "Medical Notes:\n";
					}
					foreach (var medical_note in patient?.medical_notes)
					{
						print_line += $"	- {medical_note}\n";
					}

					if (patient?.diagnoses.Any() == true)
					{
						print_line += "Diagnoses:\n";
					}
					foreach (var diagnosis in patient?.diagnoses)
					{
						print_line += $"	- {diagnosis}\n";
					}

					if (patient?.prescriptions.Any() == true)
					{
						print_line += "Prescriptions:\n";
					}
					foreach (var prescription in patient?.prescriptions)
					{
						print_line += $"	- {prescription}\n";
					}

					if (patient?.unavailable_hours.Any() == true)
					{
						print_line += "Scheduled Appointments:\n";
					}
					foreach (var unavailable_hour in patient?.unavailable_hours)
					{
						print_line += $"	- {unavailable_hour}\n";
					}
					print_line += "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n"; 
				}
				Console.WriteLine(print_line);
			break;
		case "U":
		case "u":
			if (patients.Any())
				MedicalDataService.UpdatePatient(patients);
			else
				Console.WriteLine("No patients have been created.");
			break;
		case "D":
		case "d":
			if (patients.Any())
				MedicalDataService.DeletePatient(patients);
			else
				Console.WriteLine("No patients have been created.");
			break;
		case "B":
		case "b":
			patient_loop = false;
			break;
		}
	} while (patient_loop == true);
}

void physicianFunction(List<Physician?> physicians)
{
	bool physician_loop = true;
	do
	{
		Console.WriteLine("\nC. Create a new Physician");
		Console.WriteLine("S. Add a Specialization");
		Console.WriteLine("R. Read Physicians");
		Console.WriteLine("U. Update a Physician"); // New option
		Console.WriteLine("D. Delete a Physician"); // New option
		Console.WriteLine("B. Back");
		input = Console.ReadLine();
		// Ensure physicians list is not empty for operations that require it
		bool hasPhysicians = physicians.Any();
		switch (input)
		{
		case "C":
		case "c":
			var newPhysician = MedicalDataService.createPhysician(physicians);
			physicians.Add(newPhysician); // Add the newly created physician to the list
			Console.WriteLine("Physician created.");
			break;
		case "S": // Changed from 'A' to 'S' to avoid conflict with 'A' for Appointments in main menu
		case "s":
			if (hasPhysicians)
				MedicalDataService.addSpecialization(physicians);
			else
				Console.WriteLine("No physicians have been created.");
			break;
		case "R":
		case "r":
		
			string print_line = "PHYSICIAN INFORMATION\n";
			print_line += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
				foreach (var physician in physicians)
				{
					print_line += $"Name: ({physician?.Id}) {physician?.name}\n";
					print_line += $"Graduation Date: {physician?.graduation.ToString("dd/MM/yyyy")}\n";
					print_line += $"License Number: {physician?.license_number}\n";

					if (physician?.specializations.Any() == true)
					{
						print_line += "Specializations:\n";
					}
					foreach (var specialization in physician?.specializations)
					{
						print_line += $"    - {specialization}\n";
					}

					if (physician?.unavailable_hours.Any() == true)
					{
						print_line += "Scheduled Appointments:\n";
					}
					foreach(var unavailable_hour in physician?.unavailable_hours)
					{
						print_line += $"    - {unavailable_hour}\n";
					}
					
					print_line += "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n"; 
				}
			Console.WriteLine(print_line);
			break;
		case "U": // New case for Update
		case "u":
			if (hasPhysicians)
				MedicalDataService.UpdatePhysician(physicians);
			else
				Console.WriteLine("No physicians have been created.");
			break;
		case "D": // New case for Delete
		case "d":
			if (hasPhysicians)
				MedicalDataService.DeletePhysician(physicians);
			else
				Console.WriteLine("No physicians have been created.");
			break;
		case "B":
		case "b":
			physician_loop = false;
			break;
		}
	} while (physician_loop == true);
}

void appointmentFunction(List<Patient?> patients, List<Physician?> physicians, List<Appointment?> appointments)
{
	bool appointment_loop = true;
	do
	{
		Console.WriteLine("\nC. Create an Appointment");
		Console.WriteLine("R. Read Appointments");
		Console.WriteLine("B. Back");
		input = Console.ReadLine();
		switch (input)
		{
			case "C":
			case "c":
				createAppointment(patients, physicians, appointments);
				break;
			case "R":
			case "r":
				readAppointments(appointments);
				break;
			case "B":
			case "b":
				appointment_loop = false;
				break;
		}
	} while (appointment_loop == true);
}

void createAppointment(List <Patient?> patients, List<Physician?> physicians, List<Appointment?> appointments)
{
	if (!(patients.Any() && physicians.Any()))
	{
		Console.WriteLine("Please add at least one patient and one physician to create an appointment.");
		return;
	}
	Patient? chosen_patient = null; // Initialize to null
	int patientId = 0;
	bool patientSelected = false;

	_printPatients(patients);

	// Logic to choose the user id
	do
	{
		Console.WriteLine("Please enter the ID of the patient you would like to schedule an appointment with:");
		string? inputId = Console.ReadLine();
		if (int.TryParse(inputId, out patientId))
		{
			chosen_patient = patients.FirstOrDefault(p => p?.Id == patientId);
			if (chosen_patient != null)
			{
				patientSelected = true;
			}
			else
			{
				Console.WriteLine("Invalid ID. Patient not found. Please choose a valid ID.");
			}
		}
		else
		{
			Console.WriteLine("Invalid input. Please enter a number.");
		}
	} while (!patientSelected);


	// Logic to chose the date
	Console.WriteLine("What is the appointment date? (MM/DD/YYYY)");
	bool isValidDate = false;
	DateTime appointmentStartDate = DateTime.MinValue;
	DateTime finalAppointmentTime = DateTime.MinValue;

	do
	{
		try
		{
			Console.WriteLine("Please enter a date (MM/DD/YYYY):");
			appointmentStartDate = DateTime.Parse(Console.ReadLine());

				// Check if the parsed date is a weekend day
		if (appointmentStartDate.DayOfWeek == DayOfWeek.Saturday || appointmentStartDate.DayOfWeek == DayOfWeek.Sunday)
		{
			Console.WriteLine("Invalid date. Please choose a date from Monday to Friday.");
			isValidDate = false;
		}
		else
		{
			// The date is a valid weekday
			isValidDate = true;
			Console.WriteLine($"You have selected a valid date: {appointmentStartDate.ToShortDateString()}");
		}
		}
		catch
		{
				Console.WriteLine("Invalid date format. Please use MM/DD/YYYY.");
				isValidDate = false;
		}
	} while (isValidDate == false);

	_printAvailableHours(patients, physicians, appointmentStartDate, chosen_patient);

	int chosenHour = _selectPatientTime(chosen_patient, appointmentStartDate); 

	// Combine the user's date choice and their chosen hour
	finalAppointmentTime = appointmentStartDate.Date.AddHours(chosenHour);
	Console.WriteLine(finalAppointmentTime);


	// Prints the physicians
	string print_line = "";
	foreach (var physician in physicians)
	{
		bool isAvailable = true;
		foreach (var unavailable_hour in physician?.unavailable_hours)
		{
			if (unavailable_hour.Date == finalAppointmentTime.Date && unavailable_hour.Hour == finalAppointmentTime.Hour)
			{
				isAvailable = false;
			}
		}
		if (isAvailable == true)
		{
			print_line += $"({physician?.Id}) {physician?.name}  | ";
		}
	}

	if (print_line == "")
	{
		Console.WriteLine($"Error the time slot that you've chosen ({finalAppointmentTime}) is already full with physicians.");
	}

	else
	{
		Console.WriteLine("Which physician would you like to schedule the appointment with?");
		Console.WriteLine(print_line);
	}

	_choosePhysicianId(physicians, appointments, chosen_patient, finalAppointmentTime);
}

void _printPatients(List<Patient?> patients)
{
	// Prints out all of the patients + their medical notes
	Console.WriteLine("Which patient would you like to schedule an appointment with?");
	string print_line = "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
	foreach (var patient in patients)
	{
		print_line += $"Patient: ({patient?.Id}) {patient?.name}";
		print_line += (patient?.gender?.ToLower() == "m") ? " 🧔\n" : " 👩\n";

		if (patient.medical_notes.Any())
		{
			print_line += "Medical Notes:\n";
			foreach (var medical_note in patient?.medical_notes)
			{
				print_line += $"	- {medical_note}\n";
			}
		}

		if (patient.unavailable_hours.Any())
		{
			print_line += "Scheduled Appointments:\n";
			foreach (var unavailable_hour in patient?.unavailable_hours)
			{
				print_line += $"	- {unavailable_hour}\n";
			}
		}

		print_line += "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
	}
	Console.WriteLine(print_line);
}

void _printAvailableHours(List<Patient?> patients, List<Physician?> physicians, DateTime appointmentStartDate, Patient chosen_patient)
{
	DateTime loop_start_time = appointmentStartDate.Date.AddHours(8);
	DateTime loop_end_time = appointmentStartDate.Date.AddHours(17);

	string print_line = "";
	// For every hour, print a physician. IF unavailable_hour = current_hour then don't print that physician for that hour
	for (DateTime current_hour = loop_start_time; current_hour <= loop_end_time; current_hour = current_hour.AddHours(1))
	{
		bool patient_is_available = true; // variable is reset every hour to check if the patient is available


		if (chosen_patient.unavailable_hours.Any()) // check if the patient has any unavailable hours to start with
		{
			foreach (var unavailable_hour in chosen_patient?.unavailable_hours)
			{
				if (unavailable_hour.Date == current_hour.Date && unavailable_hour.Hour == current_hour.Hour)
				{
					patient_is_available = false;
				}
			}
		}
		print_line = current_hour.ToString("hh:mm tt");

		if (patient_is_available == true)
		{
			foreach (var physician in physicians) // loop through all of the physicians
			{
				bool physician_is_available = true;
				if (physician.unavailable_hours.Any())
				{
					foreach (var unavailable_hour in physician?.unavailable_hours) // check if they are available for each specific hour
					{
						if (unavailable_hour.Hour == current_hour.Hour && unavailable_hour.Date == current_hour.Date)
						{
							physician_is_available = false;
						}
					}
				}
				if (physician_is_available == true)
				{
					print_line += $" | ({physician?.Id}) {physician?.name}";
				}
			}
		}

		else if (patient_is_available == false)
		{
			print_line += " | Unavailable";
		}
		Console.WriteLine(print_line);
	}
}

int _selectPatientTime(Patient chosen_patient, DateTime appointmentStartDate)
{
	int chosenHour = 0;
	// This section is for the user to select the time
	Console.WriteLine("Please choose the time for the appointment. (8-17)");
	bool isValidHour = true;
	var patient_unavailable_hours = new List<DateTime>(chosen_patient.unavailable_hours);
	do
	{
		isValidHour = true;
		try
		{
			chosenHour = int.Parse(Console.ReadLine());

			if (!(chosenHour >= 8 && chosenHour <= 17))
			{
				Console.WriteLine("Invalid hour. Please choose a value between 8 and 17.");
				isValidHour = false;
			}

			if (patient_unavailable_hours.Any())
			{
				foreach (var patient_unavailable_hour in patient_unavailable_hours)
				{
					if (chosenHour == patient_unavailable_hour.Hour && appointmentStartDate.Date == patient_unavailable_hour.Date)
					{
						Console.WriteLine("Invalid hour. Please choose a time when the patient is available.");
						isValidHour = false;
					}
				}
			}
		}
		catch
		{
			Console.WriteLine("Invalid input. Please enter a number.");
			isValidHour = false;
		}
	} while (isValidHour == false);

	return chosenHour;
}

void _choosePhysicianId(List<Physician?> physicians, List<Appointment?> appointments, Patient chosen_patient, DateTime finalAppointmentTime)
{
	// Choses the physician by the id
	var newAppointment = new Appointment();
	bool physicianSelected = false;
	do
	{
		Console.WriteLine("Enter the ID of the physician you would like to schedule the appointment with:");
		string? inputId = Console.ReadLine();
		if (int.TryParse(inputId, out int physician_id))
		{
			var chosen_physician = physicians.FirstOrDefault(p => p?.Id == physician_id);

			if (chosen_physician == null)
			{
				Console.WriteLine("Invalid ID. Physician not found. Please choose a valid ID.");
			}
			else
			{
				if (chosen_physician.unavailable_hours.Any(uh => uh == finalAppointmentTime))
				{
					Console.WriteLine("Physician is unavailable at this time. Please choose another physician.");
				}
				else
				{
					physicianSelected = true; // Set to true to exit loop
					chosen_patient.unavailable_hours.Add(finalAppointmentTime);
					chosen_physician.unavailable_hours.Add(finalAppointmentTime);
					newAppointment.patients = chosen_patient;
					newAppointment.physicians = chosen_physician;
					newAppointment.hour = finalAppointmentTime;
					appointments.Add(newAppointment);
					Console.WriteLine("Appointment created.");
					return; // Exit the function after successful creation
				}
			}
		}
		else
		{
			Console.WriteLine("Invalid input. Please type in a number.");
		}
	} while (!physicianSelected);
}

void readAppointments(List<Appointment?> appointments)
{
	if (appointments.Any())
	{
		string print_line = "\n\nAPPOINTMENT DETAILS\n";
		print_line += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
		foreach (var appointment in appointments)
		{
			print_line += $"{appointment.hour}\n\n";
			print_line += "PATIENT INFORMATION\n"; 
			print_line += $"Name: ({appointment?.patients?.Id}) {appointment?.patients?.name}";
			print_line += (appointment?.patients?.gender?.ToLower() == "m") ? " 🧔\n" : " 👩\n";

			if (appointment?.patients?.medical_notes.Any() ?? false)
			{
					print_line += "Medical Notes:\n";
			}

			foreach (var medical_note in appointment?.patients?.medical_notes)
			{
				print_line += $"	- {medical_note}\n";
			}
			print_line += "\n"; 
			print_line += "PHYSICIAN INFORMATION\n"; 
			print_line += $"Name: ({appointment?.physicians?.Id}) {appointment?.physicians?.name}\n";

			if (appointment?.physicians?.specializations.Any() ?? false)
			{
					print_line += "Specializations:\n";
			}

			foreach (var specialization in appointment?.physicians?.specializations)
			{
				print_line += $"	- {specialization}\n";
			}

			print_line += "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
		}
		Console.WriteLine(print_line);
	}
	else
	{
		Console.WriteLine("No appointments have been created.");
	}
}

bool continue_loop = true;
do
{
	Console.WriteLine("\nP. Patient Status");
	Console.WriteLine("H. Physician Status");
	Console.WriteLine("A. Appointment Status");
	Console.WriteLine("Q. Quit");
	input = Console.ReadLine();
	switch (input)
	{
	case "P":
	case "p":
		patientFunction(patients);
		break;
	case "H":
	case "h":
		physicianFunction(physicians);
		break;
	case "A":
	case "a":
		appointmentFunction(patients, physicians, appointments);
		break;
	case "Q":
	case "q":
		continue_loop = false;
		break;
	}

} while (continue_loop == true);