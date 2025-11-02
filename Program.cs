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
		Console.WriteLine("U. Update an Appointment");
		Console.WriteLine("D. Delete an Appointment");
		Console.WriteLine("R. Read Appointments");
		Console.WriteLine("B. Back");
		input = Console.ReadLine();
		switch (input)
		{
			case "C":
			case "c":
				MedicalDataService.CreateAppointment(patients, physicians, appointments);
				break;
			case "U":
			case "u":
				MedicalDataService.UpdateAppointment(patients, physicians, appointments);
				break;
			case "D":
			case "d":
				MedicalDataService.DeleteAppointment(appointments);
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

void readAppointments(List<Appointment?> appointments)
{
	if (appointments.Any())
	{
		string print_line = "\n\nAPPOINTMENT DETAILS\n";
		print_line += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
		foreach (var appointment in appointments)
		{
			print_line += $"APPOINTMENT ID: {appointment.Id}\n";
			print_line += $"Time: {appointment.hour}\n\n";
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